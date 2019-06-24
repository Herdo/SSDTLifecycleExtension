namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Factories;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ModifyDeploymentScriptUnit : IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IScriptModifierFactory _scriptModifierFactory;
        [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
        [NotNull] private readonly ILogger _logger;

        public ModifyDeploymentScriptUnit([NotNull] IScriptModifierFactory scriptModifierFactory,
                                          [NotNull] IFileSystemAccess fileSystemAccess,
                                          [NotNull] ILogger logger)
        {
            _scriptModifierFactory = scriptModifierFactory ?? throw new ArgumentNullException(nameof(scriptModifierFactory));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task ModifyCreatedScriptAsync(SqlProject project,
                                                    ConfigurationModel configuration,
                                                    PathCollection paths)
        {
            var modifiers = GetScriptModifiers(configuration);
            if (!modifiers.Any())
                return;

            var scriptContent = await _fileSystemAccess.ReadFileAsync(paths.DeployScriptPath);

            foreach (var m in modifiers.OrderBy(m => m.Key))
            {
                await _logger.LogAsync($"Modifying script: {m.Key}");

                scriptContent = await m.Value.ModifyAsync(scriptContent,
                                                          project,
                                                          configuration,
                                                          paths);
            }

            await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, scriptContent);
        }

        private IReadOnlyDictionary<ScriptModifier, IScriptModifier> GetScriptModifiers(ConfigurationModel configuration)
        {
            var result = new Dictionary<ScriptModifier, IScriptModifier>();

            if (configuration.CommentOutUnnamedDefaultConstraintDrops)
                result[ScriptModifier.CommentOutUnnamedDefaultConstraintDrops] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops);

            if (configuration.ReplaceUnnamedDefaultConstraintDrops)
                result[ScriptModifier.ReplaceUnnamedDefaultConstraintDrops] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops);

            if (!string.IsNullOrWhiteSpace(configuration.CustomHeader))
                result[ScriptModifier.AddCustomHeader] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomHeader);

            if (!string.IsNullOrWhiteSpace(configuration.CustomFooter))
                result[ScriptModifier.AddCustomFooter] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomFooter);

            if (configuration.TrackDacpacVersion)
                result[ScriptModifier.TrackDacpacVersion] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.TrackDacpacVersion);

            return result;
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            await ModifyCreatedScriptAsync(stateModel.Project, stateModel.Configuration, stateModel.Paths);
            stateModel.CurrentState = StateModelState.ModifiedDeploymentScript;
        }
    }
}