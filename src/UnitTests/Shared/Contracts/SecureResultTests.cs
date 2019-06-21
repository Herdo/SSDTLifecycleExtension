using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System.IO;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class SecureResultTests
    {
        [Test]
        public void Constructor_ConsumeProperties()
        {
            // Arrange
            var s = "foo";
            var e = new IOException("test exception");

            // Act
            var sr = new SecureResult<string>(s, e);

            // Assert
            Assert.AreEqual(s, sr.Value);
            Assert.AreSame(e, sr.Exception);
        }
    }
}