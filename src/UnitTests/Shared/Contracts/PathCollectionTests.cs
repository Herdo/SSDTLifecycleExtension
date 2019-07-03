using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class PathCollectionTests
    {
        [Test]
        public void Constructor_ArgumentNullException_ProjectDirectory()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(null, null, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_PublishProfilePath()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(projectDirectory, null, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_LatestArtifactsDirectory()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(projectDirectory, publishProfilePath, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_NewArtifactsDirectory()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";
            const string latestArtifactsDirectory = "_DIRECTORY_latestArtifacts";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(projectDirectory, publishProfilePath, latestArtifactsDirectory, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_NewDacpacPath()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";
            const string latestArtifactsDirectory = "_DIRECTORY_latestArtifacts";
            const string newArtifactsDirectory = "_DIRECTORY_newArtifacts";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(projectDirectory, publishProfilePath, latestArtifactsDirectory, newArtifactsDirectory, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_InvalidOperationException_NeitherScriptPathNorDeployPathSet_WhenPreviousDacpacPathIsSet()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";
            const string latestArtifactsDirectory = "_DIRECTORY_latestArtifacts";
            const string newArtifactsDirectory = "_DIRECTORY_newArtifacts";
            const string newDacpacPath = "_PATH_newDacpac";
            const string previousDacpacPath = "_PATH_previousDacpac";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<InvalidOperationException>(() => new PathCollection(projectDirectory, publishProfilePath, latestArtifactsDirectory, newArtifactsDirectory, newDacpacPath, previousDacpacPath, null, null));
        }

        [Test]
        public void Constructor_NoInvalidOperationException_NeitherScriptPathNorDeployPathSet_WhenPreviousDacpacPathIsNotSet()
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";
            const string latestArtifactsDirectory = "_DIRECTORY_latestArtifacts";
            const string newArtifactsDirectory = "_DIRECTORY_newArtifacts";
            const string newDacpacPath = "_PATH_newDacpac";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.DoesNotThrow(() => new PathCollection(projectDirectory, publishProfilePath, latestArtifactsDirectory, newArtifactsDirectory, newDacpacPath, null, null, null));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Constructor_CorrectSettingOfProperties(bool setDeployScriptPath, bool setDeployReportPath)
        {
            // Arrange
            const string projectDirectory = "projectDirectory";
            const string publishProfilePath = "publishProfile";
            const string latestArtifactsDirectory = "_DIRECTORY_latestArtifacts";
            const string newArtifactsDirectory = "_DIRECTORY_newArtifacts";
            const string newDacpacPath = "_PATH_newDacpac";
            const string previousDacpacPath = "_PATH_previousDacpac";
            const string deployScriptPath = "_PATH_deployScript";
            const string deployReportPath = "_PATH_deployReport";

            // Act
            var pc = new PathCollection(projectDirectory,
                                        publishProfilePath,
                                        latestArtifactsDirectory,
                                        newArtifactsDirectory,
                                        newDacpacPath,
                                        previousDacpacPath,
                                        setDeployScriptPath ? deployScriptPath : null,
                                        setDeployReportPath ? deployReportPath : null);

            // Assert
            Assert.AreEqual(projectDirectory, pc.ProjectDirectory);
            Assert.AreEqual(publishProfilePath, pc.PublishProfilePath);
            Assert.AreEqual(latestArtifactsDirectory, pc.LatestArtifactsDirectory);
            Assert.AreEqual(newArtifactsDirectory, pc.NewArtifactsDirectory);
            Assert.AreEqual(newDacpacPath, pc.NewDacpacPath);
            Assert.AreEqual(previousDacpacPath, pc.PreviousDacpacPath);
            if (setDeployScriptPath)
                Assert.AreEqual(deployScriptPath, pc.DeployScriptPath);
            else
                Assert.IsNull(pc.DeployScriptPath);
            if (setDeployReportPath)
                Assert.AreEqual(deployReportPath, pc.DeployReportPath);
            else
                Assert.IsNull(pc.DeployReportPath);
        }
    }
}