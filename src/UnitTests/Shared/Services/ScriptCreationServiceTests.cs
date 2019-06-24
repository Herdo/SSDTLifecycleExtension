using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScriptCreationServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_WorkUnitFactory()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(wufMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(wufMock, vsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }
    }
}