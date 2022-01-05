using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using SSDTLifecycleExtension.Shared.Models;

    [TestFixture]
    public class VersionModelTests
    {
        [Test]
        public void Default_AllEmpty()
        {
            // Act
            var vm = new VersionModel();

            // Assert
            Assert.IsNull(vm.UnderlyingVersion);
            Assert.IsFalse(vm.IsNewestVersion);
            Assert.AreEqual("<null>", vm.DisplayName);
        }

        [Test]
        public void UnderlyingVersion_NoChange()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new VersionModel();
            vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            // Act
            vm.UnderlyingVersion = null;

            // Assert
            Assert.IsNull(vm.UnderlyingVersion);
            Assert.AreEqual(0, changedProperties.Count);
            Assert.AreEqual("<null>", vm.DisplayName);
        }

        [Test]
        public void UnderlyingVersion_Change()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new VersionModel();
            vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
            var newVersion = new Version(1, 2, 3, 4);

            // Act
            vm.UnderlyingVersion = newVersion;

            // Assert
            Assert.AreSame(newVersion, vm.UnderlyingVersion);
            Assert.AreEqual(2, changedProperties.Count);
            Assert.AreEqual(nameof(VersionModel.UnderlyingVersion), changedProperties[0]);
            Assert.AreEqual(nameof(VersionModel.DisplayName), changedProperties[1]);
            Assert.AreEqual("1.2.3.4", vm.DisplayName);
        }

        [Test]
        public void IsNewestVersion_NoChange()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new VersionModel();
            vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            // Act
            vm.IsNewestVersion = false;

            // Assert
            Assert.IsFalse(vm.IsNewestVersion);
            Assert.AreEqual(0, changedProperties.Count);
            Assert.AreEqual("<null>", vm.DisplayName);
        }

        [Test]
        public void IsNewestVersion_Change()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new VersionModel();
            vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            // Act
            vm.IsNewestVersion = true;

            // Assert
            Assert.IsTrue(vm.IsNewestVersion);
            Assert.AreEqual(2, changedProperties.Count);
            Assert.AreEqual(nameof(VersionModel.IsNewestVersion), changedProperties[0]);
            Assert.AreEqual(nameof(VersionModel.DisplayName), changedProperties[1]);
            Assert.AreEqual("<null>", vm.DisplayName);
        }

        [Test]
        public void DisplayName_UnderlyingVersionIsNewestVersion()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new VersionModel();
            var newVersion = new Version(3, 2, 3);
            vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            // Act
            vm.UnderlyingVersion = newVersion;
            vm.IsNewestVersion = true;

            // Assert
            Assert.AreSame(newVersion, vm.UnderlyingVersion);
            Assert.IsTrue(vm.IsNewestVersion);
            Assert.AreEqual(4, changedProperties.Count);
            Assert.AreEqual(nameof(VersionModel.UnderlyingVersion), changedProperties[0]);
            Assert.AreEqual(nameof(VersionModel.DisplayName), changedProperties[1]);
            Assert.AreEqual(nameof(VersionModel.IsNewestVersion), changedProperties[2]);
            Assert.AreEqual(nameof(VersionModel.DisplayName), changedProperties[3]);
            Assert.AreEqual("3.2.3 (newest)", vm.DisplayName);
        }
    }
}