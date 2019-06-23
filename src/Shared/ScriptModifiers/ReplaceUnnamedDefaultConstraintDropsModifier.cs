namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using JetBrains.Annotations;
    using Models;

    public class ReplaceUnnamedDefaultConstraintDropsModifier : StringSearchModifierBase,
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
  AND c.name = column_name

EXECUTE (@command)";

        private readonly IDacAccess _dacAccess;
        private readonly ILogger _logger;

        public ReplaceUnnamedDefaultConstraintDropsModifier([NotNull] IDacAccess dacAccess,
                                                            [NotNull] ILogger logger)
        {
            _dacAccess = dacAccess ?? throw new ArgumentNullException(nameof(dacAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<string> ModifyInternalAsync(string input,
                                                       PathCollection paths)
        {
            var (errorsWhileLoading, oldDefaultConstraints, currentDefaultConstraints) = await GetDefaultConstraints(paths);
            if (errorsWhileLoading)
                return input;

            var defaultConstraintsToRemove = oldDefaultConstraints.Where(m => m.ConstraintName == null)
                                                                  .Except(currentDefaultConstraints)
                                                                  .ToDictionary(constraint => constraint, constraint => false);

            var (resultText, regexMatchTimeouts) = ReplaceUnnamedDefaultConstraintStatements(input, defaultConstraintsToRemove);
            if (regexMatchTimeouts > 0)
                await _logger.LogAsync($"WARNING: Regular expression matching in {nameof(ReplaceUnnamedDefaultConstraintDropsModifier)} timed out {regexMatchTimeouts} time(s).");

            return resultText;
        }

        private async Task<(bool ErrorsWhileLoading, DefaultConstraint[] OldDefaultConstraints, DefaultConstraint[] CurrentDefaultConstraints)> GetDefaultConstraints(PathCollection paths)
        {
            var oldDefaultConstraints = await _dacAccess.GetDefaultConstraintsAsync(paths.PreviousDacpacPath);
            if (oldDefaultConstraints.Errors != null)
            {
                await _logger.LogAsync("ERROR: Failed to load the default constraints of the previous DACPAC:");
                foreach (var error in oldDefaultConstraints.Errors)
                    await _logger.LogAsync(error);
            }

            var currentDefaultConstraints = await _dacAccess.GetDefaultConstraintsAsync(paths.NewDacpacPath);
            if (currentDefaultConstraints.Errors != null)
            {
                await _logger.LogAsync("ERROR: Failed to load the default constraints of the current DACPAC:");
                foreach (var error in currentDefaultConstraints.Errors)
                    await _logger.LogAsync(error);
            }

            return (oldDefaultConstraints.Errors != null || currentDefaultConstraints.Errors != null,
                    oldDefaultConstraints.DefaultConstraints,
                    currentDefaultConstraints.DefaultConstraints);
        }

        private (string ResultText, int RegexMatchTimeouts) ReplaceUnnamedDefaultConstraintStatements(string input,
                                                                                                      Dictionary<DefaultConstraint, bool> defaultConstraintsToRemove)
        {
            var tableRegex = new Regex(@"ALTER TABLE \[(?<schemaName>\w+)\]\.\[(?<tableName>\w+)\] DROP CONSTRAINT ;", RegexOptions.Compiled, TimeSpan.FromMilliseconds(10));
            var regexMatchTimeouts = 0;
            return (ForEachMatch(input,
                                 "DROP CONSTRAINT ;",
                                 1,
                                 range =>
                                 {
                                     var lines = range.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                                     string replacement = null;
                                     for (var index = 0; index < lines.Length; index++)
                                     {
                                         var line = lines[index];
                                         var isMatch = IsMatch(line, tableRegex, out var schemaName, out var tableName, out var regexMatchTimeout);
                                         if (regexMatchTimeout)
                                             regexMatchTimeouts++;
                                         if (!isMatch)
                                             continue;

                                         replacement = ReplaceDropAtLine(defaultConstraintsToRemove, schemaName, tableName, lines, index);
                                         break;
                                     }

                                     return replacement ?? range;
                                 }),
                    regexMatchTimeouts);
        }

        private static bool IsMatch(string line,
                                    Regex tableRegex,
                                    out string schemaName,
                                    out string tableName,
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

        private static string ReplaceDropAtLine(Dictionary<DefaultConstraint, bool> defaultConstraintsToRemove,
                                                string schemaName,
                                                string tableName,
                                                string[] lines,
                                                int index)
        {
            var toRemove = defaultConstraintsToRemove.Where(m => m.Key.TableSchema == schemaName
                                                                 && m.Key.TableName == tableName
                                                                 && m.Value == false // Not used for any replacement yet.
                                                           )
                                                     .Select(m => m.Key)
                                                     .FirstOrDefault();

            if (toRemove == null)
                return null;

            // Flag as already used for a replacement.
            defaultConstraintsToRemove[toRemove] = true;

            // Prepare template
            var dropConstraint = string.Format(DropScriptTemplate,
                                               toRemove.TableSchema,
                                               toRemove.TableName,
                                               toRemove.ColumnName);

            // Keep lines before and after
            var combined = CombineLinesBeforeStatementAndLinesAfter(lines, index, dropConstraint);
            return string.Join(Environment.NewLine, combined);
        }

        private static string[] CombineLinesBeforeStatementAndLinesAfter(string[] lines,
                                                                         int index,
                                                                         string dropConstraint)
        {
            var linesBefore = lines.Take(index).ToArray();
            var linesAfter = lines.Skip(index + 1).ToArray();
            var combined = new string[linesBefore.Length + 1 + linesAfter.Length];
            linesBefore.CopyTo(combined, 0);
            combined[linesBefore.Length] = dropConstraint;
            linesAfter.CopyTo(combined, linesBefore.Length + 1);
            return combined;
        }

        Task<string> IScriptModifier.ModifyAsync(string input,
                                                 SqlProject project,
                                                 ConfigurationModel configuration,
                                                 PathCollection paths)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            return ModifyInternalAsync(input, paths);
        }
    }
}