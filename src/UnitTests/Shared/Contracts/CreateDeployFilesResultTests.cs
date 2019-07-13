using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class CreateDeployFilesResultTests
    {
        [Test]
        public void Constructor_Errors_ArgumentNullException()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CreateDeployFilesResult(null));
        }

        [Test]
        public void Constructor_Errors_CorrectInitialization()
        {
            // Arrange
            var errors = new string[1];

            // Act
            var result = new CreateDeployFilesResult(errors);

            // Assert
            Assert.IsNull(result.DeployScriptContent);
            Assert.IsNull(result.DeployReportContent);
            Assert.IsNull(result.PreDeploymentScript);
            Assert.IsNull(result.PostDeploymentScript);
            Assert.AreSame(errors, result.Errors);
        }

        [Test]
        public void Constructor_Contents_CorrectInitialization()
        {
            // Arrange
            const string script = "script";
            const string report = "report";
            const string pre = "pre";
            const string post = "post";

            // Act
            var result = new CreateDeployFilesResult(script, report, pre, post);

            // Assert
            Assert.AreEqual(script, result.DeployScriptContent);
            Assert.AreEqual(report, result.DeployReportContent);
            Assert.AreEqual(pre, result.PreDeploymentScript);
            Assert.AreEqual(post, result.PostDeploymentScript);
            Assert.IsNull(result.Errors);
        }
    }
}