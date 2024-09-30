namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

public class ReplaceUnnamedDefaultConstraintDropsModifier(IDacAccess _dacAccess,
                                                          ILogger _logger)
    : StringSearchModifierBase,
    IScriptModifier
{
    private const string DropScriptTemplate =
        @"DECLARE @schema_name sysname
DECLARE @table_name  sysname
DECLARE @column_name sysname
DECLARE @command     nvarchar(1000)

SET @schema_name = N'{0}'
SET @table_name = N'{1}'
SET @column_name = N'{2}'

SELECT @command = 'ALTER TABLE [' + @schema_name + '].[' + @table_name + '] DROP CONSTRAINT ' + d.name
 FROM sys.tables t
 JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
 JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = schema_id(@schema_name)
  AND c.name = @column_name

EXECUTE (@command)";

    private async Task ModifyInternalAsync(ScriptModificationModel model)
    {
        var (errorsWhileLoading, oldDefaultConstraints, currentDefaultConstraints) = await GetDefaultConstraints(model.Paths);
        if (errorsWhileLoading)
            return;

        var defaultConstraintsToRemove = oldDefaultConstraints.Where(m => m.ConstraintName == null)
                                                              .Except(currentDefaultConstraints)
                                                              .ToDictionary(constraint => constraint, constraint => false);

        var (resultText, regexMatchTimeouts, failedReplacements) = ReplaceUnnamedDefaultConstraintStatements(model.CurrentScript, defaultConstraintsToRemove);
        if (regexMatchTimeouts > 0)
            await _logger.LogWarningAsync(
                $"{nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Regular expression matching timed out {regexMatchTimeouts} time(s).");
        if (failedReplacements > 0)
            await _logger.LogWarningAsync(
                $"{nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Script defines {failedReplacements} unnamed default constraint(s) more to drop than the DACPAC models provide.");

        model.CurrentScript = resultText;
    }

    private async Task<(bool ErrorsWhileLoading, DefaultConstraint[]? OldDefaultConstraints, DefaultConstraint[]? CurrentDefaultConstraints)>
        GetDefaultConstraints(PathCollection paths)
    {
        var (oldDefaultConstraints, errors) = await _dacAccess.GetDefaultConstraintsAsync(paths.DeploySources.PreviousDacpacPath!);
        if (errors is not null)
        {
            await _logger.LogErrorAsync("Failed to load the default constraints of the previous DACPAC:");
            foreach (var error in errors)
                await _logger.LogErrorAsync(error);
        }

        (var currentDefaultConstraints, errors) = await _dacAccess.GetDefaultConstraintsAsync(paths.DeploySources.NewDacpacPath);
        if (errors is not null)
        {
            await _logger.LogErrorAsync("Failed to load the default constraints of the current DACPAC:");
            foreach (var error in errors)
                await _logger.LogErrorAsync(error);
        }

        return (errors is not null || errors is not null,
            oldDefaultConstraints,
            currentDefaultConstraints);
    }

    private (string ResultText, int RegexMatchTimeouts, int FailedReplacements) ReplaceUnnamedDefaultConstraintStatements(string input,
        IDictionary<DefaultConstraint, bool> defaultConstraintsToRemove)
    {
        var tableRegex = new Regex(@"^ALTER TABLE \[(?<schemaName>\w+)\]\.\[(?<tableName>\w+)\] DROP CONSTRAINT ;$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(10));
        var regexMatchTimeouts = 0;
        var failedReplacements = 0;
        return (ForEachMatch(input,
                             "DROP CONSTRAINT ;",
                             1,
                             range =>
                             {
                                 var lines = range.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                                 string? replacement = null;
                                 for (var index = 0; index < lines.Length; index++)
                                 {
                                     var line = lines[index];
                                     var isMatch = IsMatch(line, tableRegex, out var schemaName, out var tableName, out var regexMatchTimeout);
                                     if (regexMatchTimeout)
                                         regexMatchTimeouts++;
                                     if (!isMatch)
                                         continue;

                                     var defaultConstraintToRemove = GetDefaultConstraintToRemove(defaultConstraintsToRemove, schemaName!, tableName!);
                                     if (defaultConstraintToRemove is null)
                                         failedReplacements++;
                                     else
                                         replacement = BuildReplacement(defaultConstraintToRemove, lines, index);

                                     break;
                                 }

                                 return replacement ?? range;
                             }),
            regexMatchTimeouts,
            failedReplacements);
    }

    private static bool IsMatch(string line,
        Regex tableRegex,
        out string? schemaName,
        out string? tableName,
        out bool regexMatchTimeout)
    {
        schemaName = null;
        tableName = null;
        regexMatchTimeout = false;

        // Basic checks before executing the regular expression
        if (string.IsNullOrWhiteSpace(line))
            return false;

        if (line.StartsWith("GO"))
            return false;

        if (line.StartsWith("PRINT"))
            return false;

        // Finally execute the regular expression.
        Match match;
        try
        {
            match = tableRegex.Match(line);
        }
        catch (RegexMatchTimeoutException)
        {
            regexMatchTimeout = true;
            return false;
        }

        if (!match.Success)
            return false;

        schemaName = match.Groups["schemaName"].Value;
        tableName = match.Groups["tableName"].Value;
        return true;
    }

    private static DefaultConstraint? GetDefaultConstraintToRemove(IDictionary<DefaultConstraint, bool> defaultConstraintsToRemove,
        string schemaName,
        string tableName)
    {
        var toRemove = defaultConstraintsToRemove.Where(m => m.Key.TableSchema == schemaName
                                                          && m.Key.TableName == tableName
                                                          && !m.Value // Not used for any replacement yet.
                                                 )
                                                 .Select(m => m.Key)
                                                 .FirstOrDefault();

        if (toRemove is not null)
            // Flag as already used for a replacement.
            defaultConstraintsToRemove[toRemove] = true;

        return toRemove;
    }

    private static string BuildReplacement(DefaultConstraint defaultConstraint,
        string[] lines,
        int indexOfMatchedLine)
    {
        // Prepare template
        var dropConstraint = string.Format(DropScriptTemplate,
                                           defaultConstraint.TableSchema,
                                           defaultConstraint.TableName,
                                           defaultConstraint.ColumnName);

        // Keep lines before and after
        var combined = CombineLinesBeforeStatementAndLinesAfter(lines, indexOfMatchedLine, dropConstraint);
        return string.Join(Environment.NewLine, combined);
    }

    private static string[] CombineLinesBeforeStatementAndLinesAfter(string[] lines,
        int indexOfMatchedLine,
        string dropConstraint)
    {
        var linesBefore = lines.Take(indexOfMatchedLine).ToArray();
        var linesAfter = lines.Skip(indexOfMatchedLine + 1).ToArray();
        var combined = new string[linesBefore.Length + 1 + linesAfter.Length];
        linesBefore.CopyTo(combined, 0);
        combined[linesBefore.Length] = dropConstraint;
        linesAfter.CopyTo(combined, linesBefore.Length + 1);
        return combined;
    }

    Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
    {
        Guard.IsNotNullOrWhiteSpace(model.Paths.DeploySources.PreviousDacpacPath);
        Guard.IsNotNullOrWhiteSpace(model.Paths.DeploySources.NewDacpacPath);

        return ModifyInternalAsync(model);
    }
}