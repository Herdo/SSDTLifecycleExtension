using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class PathCollectionTests
    {
        [Test]
        public void Constructor_ArgumentNullException_PublishProfilePath()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_NewDacpacDirectory()
        {
            // Arrange
            const string publishProfilePath = "publishProfile";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(publishProfilePath, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_NewDacpacPath()
        {
            // Arrange
            const string publishProfilePath = "publishProfile";
            const string newDacpacDirectory = "_DIRECTORY_newDacpac";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new PathCollection(publishProfilePath, newDacpacDirectory, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_InvalidOperationException_NeitherScriptPathNorDeployPathSet()
        {
            // Arrange
            const string publishProfilePath = "publishProfile";
            const string newDacpacDirectory = "_DIRECTORY_newDacpac";
            const string newDacpacPath = "_PATH_newDacpac";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<InvalidOperationException>(() => new PathCollection(publishProfilePath, newDacpacDirectory, newDacpacPath, null, null, null));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Constructor_CorrectSettingOfProperties(bool setDeployScriptPath, bool setDeployReportPath)
        {
            // Arrange
            const string publishProfilePath = "publishProfile";
            const string newDacpacDirectory = "_DIRECTORY_newDacpac";
            const string newDacpacPath = "_PATH_newDacpac";
            const string previousDacpacPath = "_PATH_previousDacpac";
            const string deployScriptPath = "_PATH_deployScript";
            const string deployReportPath = "_PATH_deployReport";

            // Act
            var pc = new PathCollection(publishProfilePath,
                                        newDacpacDirectory,
                                        newDacpacPath,
                                        previousDacpacPath,
                                        setDeployScriptPath ? deployScriptPath : null,
                                        setDeployReportPath ? deployReportPath : null);

            // Assert
            Assert.AreEqual(publishProfilePath, pc.PublishProfilePath);
            Assert.AreEqual(newDacpacDirectory, pc.NewDacpacDirectory);
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