namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels;

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
            sender.Should().NotBeNull();
            sender.Should().BeSameAs(vm);
            args.Should().NotBeNull();
            changedProperties.Add(args.PropertyName);
        };

        // Act
        vm.ImplicitName = "test";

        // Assert
        vm.ImplicitName.Should().Be("test");
        vm.ExplicitName.Should().BeNull();
        changedProperties.Should().ContainSingle()
            .Which.Should().Be("ImplicitName");
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
            sender.Should().NotBeNull();
            sender.Should().BeSameAs(vm);
            args.Should().NotBeNull();
            changedProperties.Add(args.PropertyName);
        };

        // Act
        vm.ExplicitName = "test";

        // Assert
        vm.ImplicitName.Should().BeNull();
        vm.ExplicitName.Should().Be("test");
        changedProperties.Should().ContainSingle()
            .Which.Should().Be("ExplicitNameExtra");
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

        public override Task<bool> InitializeAsync()
        {
            throw new NotSupportedException();
        }
    }
}
