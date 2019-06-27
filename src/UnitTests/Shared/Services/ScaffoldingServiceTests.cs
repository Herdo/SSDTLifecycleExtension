using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScaffoldingServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_WorkUnitFactory()
        {
            // Arrange
            var loggerMock = Mock.Of<ILogger>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var e = Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(null, null, loggerMock));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("workUnitFactory", e.ParamName);
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var loggerMock = Mock.Of<ILogger>();
            var wufMock = Mock.Of<IWorkUnitFactory>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var e = Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(wufMock, null, loggerMock));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("visualStudioAccess", e.ParamName);
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var e = Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(wufMock, vsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("logger", e.ParamName);
        }
    }
}