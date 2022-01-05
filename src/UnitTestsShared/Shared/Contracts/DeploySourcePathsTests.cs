using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class DeploySourcePathsTests
    {
        [Test]
        public void Constructor_ArgumentNullException_NewDacpacPath()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DeploySourcePaths(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_PublishProfilePath()
        {
            // Arrange
            const string newDacpacPath = "newDacpacPath";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DeploySourcePaths(newDacpacPath, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectSettingOfProperties()
        {
            // Arrange
            const string newDacpacPath = "newDacpacPath";
            const string publishProfilePath = "publishProfilePath";
            const string previousDacpacPath = "previousDacpacPath";

            // Act
            var dsp = new DeploySourcePaths(newDacpacPath, publishProfilePath, previousDacpacPath);

            // Assert
            Assert.AreEqual(newDacpacPath, dsp.NewDacpacPath);
            Assert.AreEqual(publishProfilePath, dsp.PublishProfilePath);
            Assert.AreEqual(previousDacpacPath, dsp.PreviousDacpacPath);
        }
    }
}