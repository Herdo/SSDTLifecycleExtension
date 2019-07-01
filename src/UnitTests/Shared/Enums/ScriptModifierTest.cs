using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Enums
{
    using SSDTLifecycleExtension.Shared.Contracts.Enums;

    [TestFixture]
    public class ScriptModifierTest
    {
        [Test]
        public void RemoveSqlCmdStatements_AlwaysHaveHighestValue_ForSortOrder()
        {
            // Act
            var intValue = (int)ScriptModifier.RemoveSqlCmdStatements;

            // Assert
            Assert.AreEqual(int.MaxValue, intValue);
        }
    }
}