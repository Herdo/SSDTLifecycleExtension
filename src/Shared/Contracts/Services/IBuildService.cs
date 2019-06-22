namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IBuildService
    {
        Task<bool> BuildProjectAsync([NotNull] SqlProject project);

        /// <summary>
        /// Copies the build result (*.dacpac) from the <paramref name="project"/> to the <paramref name="targetDirectory"/>.
        /// </summary>
        /// <param name="project">The project to copy the DACPAC files from.</param>
        /// <param name="targetDirectory">The target directory to copy the files into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="project"/> or <paramref name="targetDirectory"/> are <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><see cref="SqlProjectProperties.BinaryDirectory"/> of <paramref name="project"/> is not filled.</exception>
        /// <returns><b>True</b>, when the copy was successful, otherwise <b>false</b>.</returns>
        Task<bool> CopyBuildResultAsync([NotNull] SqlProject project,
                                        [NotNull] string targetDirectory);
    }
}