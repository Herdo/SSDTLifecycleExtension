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
    using Contracts.Models;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ModifyDeploymentScriptUnit : IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IScriptModifierProviderService _scriptModifierProviderService;
        [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
        [NotNull] private readonly ILogger _logger;

        public ModifyDeploymentScriptUnit([NotNull] IScriptModifierProviderService scriptModifierProviderService,
                                          [NotNull] IFileSystemAccess fileSystemAccess,
                                          [NotNull] ILogger logger)
        {
            _scriptModifierProviderService = scriptModifierProviderService ?? throw new ArgumentNullException(nameof(scriptModifierProviderService));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task ModifyCreatedScriptInternal(IStateModel stateModel,
                                                       SqlProject project,
                                                       ConfigurationModel configuration,
                                                       PathCollection paths)
        {
            var modifiers = _scriptModifierProviderService.GetScriptModifiers(configuration);
            if (!modifiers.Any())
            {
                stateModel.CurrentState = StateModelState.ModifiedDeploymentScript;
                return;
            }

            await ApplyAllModifiers(project, configuration, paths, modifiers);
            stateModel.CurrentState = StateModelState.ModifiedDeploymentScript;
        }

        private async Task ApplyAllModifiers(SqlProject project,
                                             ConfigurationModel configuration,
                                             PathCollection paths,
                                             IReadOnlyDictionary<ScriptModifier, IScriptModifier> modifiers)
        {
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

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return ModifyCreatedScriptInternal(stateModel, stateModel.Project, stateModel.Configuration, stateModel.Paths);
        }
    }
}