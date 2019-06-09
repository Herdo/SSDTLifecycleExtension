namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface ISqlProjectService
    {
        /// <summary>
        /// Tries to load the <see cref="SqlProject.ProjectProperties"/> of the <paramref name="project"/>.
        /// </summary>
        /// <param name="project">The <see cref="SqlProject"/> to load the <see cref="SqlProjectProperties"/> for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="project"/> is <b>null</b>.</exception>
        /// <returns><b>True</b>, if the properties were loaded successfully, otherwise <b>false</b>.</returns>
        Task<bool> TryLoadSqlProjectPropertiesAsync([NotNull] SqlProject project);
    }
}