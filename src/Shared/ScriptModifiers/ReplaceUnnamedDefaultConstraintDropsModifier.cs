namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
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

            if (oldDefaultConstraints.Errors != null || currentDefaultConstraints.Errors != null)
                return input;

            var defaultConstraintsToRemove = oldDefaultConstraints.DefaultConstraints
                                                                  .Where(m => m.ConstraintName == null)
                                                                  .Except(currentDefaultConstraints.DefaultConstraints)
                                                                  .ToDictionary(constraint => constraint, constraint => false);

            var tableRegex = new Regex(@"ALTER TABLE \[(?<schemaName>\w+)\]\.\[(?<tableName>\w+)\] DROP CONSTRAINT ;");
            return ForEachMatch(input,
                                "DROP CONSTRAINT ;",
                                1,
                                range =>
                                {
                                    var lines = range.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                                    string replacement = null;
                                    for (var index = 0; index < lines.Length; index++)
                                    {
                                        var line = lines[index];
                                        var match = tableRegex.Match(line);
                                        if (!match.Success)
                                            continue;

                                        var schemaName = match.Groups["schemaName"].Value;
                                        var tableName = match.Groups["tableName"].Value;
                                        var toRemove = defaultConstraintsToRemove.Where(m => m.Key.TableSchema == schemaName
                                                                                             && m.Key.TableName == tableName
                                                                                             && m.Value == false // Not used for any replacement yet.
                                                                                       )
                                                                                 .Select(m => m.Key)
                                                                                 .FirstOrDefault();
                                        if (toRemove != null)
                                        {
                                            defaultConstraintsToRemove[toRemove] = true; // Flag as already used for a replacement;

                                            // Prepare template
                                            var dropConstraint = string.Format(DropScriptTemplate,
                                                                               toRemove.TableSchema,
                                                                               toRemove.TableName,
                                                                               toRemove.ColumnName);

                                            // Keep lines before and after
                                            var linesBefore = lines.Take(index).ToArray();
                                            var linesAfter = lines.Skip(index + 1).ToArray();
                                            var combined = new string[linesBefore.Length + 1 + linesAfter.Length];
                                            linesBefore.CopyTo(combined, 0);
                                            combined[linesBefore.Length] = dropConstraint;
                                            linesAfter.CopyTo(combined, linesBefore.Length + 1);
                                            replacement = string.Join(Environment.NewLine, combined);
                                        }

                                        break;
                                    }

                                    return replacement ?? range;
                                });
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