using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels
{
    using System.Collections.Generic;
    using SSDTLifecycleExtension.ViewModels;

    [TestFixture]
    public class ViewModelBaseTests
    {
        [Test]
        public void PropertyChanged_ImplicitName()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new ViewModelBaseTestImplementation();
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                Assert.IsNotNull(sender);
                Assert.AreSame(vm, sender);
                Assert.IsNotNull(args);
                changedProperties.Add(args.PropertyName);
            };

            // Act
            vm.ImplicitName = "test";

            // Assert
            Assert.AreEqual("test", vm.ImplicitName);
            Assert.IsNull(vm.ExplicitName);
            Assert.AreEqual(1, changedProperties.Count);
            Assert.AreEqual("ImplicitName", changedProperties[0]);
        }

        [Test]
        public void PropertyChanged_ExplicitName()
        {
            // Arrange
            var changedProperties = new List<string>();
            var vm = new ViewModelBaseTestImplementation();
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                Assert.IsNotNull(sender);
                Assert.AreSame(vm, sender);
                Assert.IsNotNull(args);
                changedProperties.Add(args.PropertyName);
            };

            // Act
            vm.ExplicitName = "test";

            // Assert
            Assert.IsNull(vm.ImplicitName);
            Assert.AreEqual("test", vm.ExplicitName);
            Assert.AreEqual(1, changedProperties.Count);
            Assert.AreEqual("ExplicitNameExtra", changedProperties[0]);
        }

        private sealed class ViewModelBaseTestImplementation : ViewModelBase
        {
            private string _implicitName;
            private string _explicitName;

            internal string ImplicitName
            {
                get => _implicitName;
                set
                {
                    if (value == _implicitName)
                        return;
                    _implicitName = value;
                    OnPropertyChanged();
                }
            }

            internal string ExplicitName
            {
                get => _explicitName;
                set
                {
                    if (value == _explicitName)
                        return;
                    _explicitName = value;
                    OnPropertyChanged(nameof(ExplicitName) + "Extra");
                }
            }
        }
    }
}