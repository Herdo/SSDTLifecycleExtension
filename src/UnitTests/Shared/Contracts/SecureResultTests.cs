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
            var stream = new MemoryStream();
            const string s = "foo";
            var e = new IOException("test exception");

            // Act
            var sr = new SecureStreamResult<string>(stream, s, e);

            // Assert
            Assert.AreEqual(s, sr.Result);
            Assert.AreSame(e, sr.Exception);
        }
    }
}