namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

[TestFixture]
public class VersionModelTests
{
    [Test]
    public void Default_AllEmpty()
    {
        // Act
        var vm = new VersionModel();

        // Assert
        vm.UnderlyingVersion.Should().Be(new Version(0, 0));
        vm.IsNewestVersion.Should().BeFalse();
        vm.DisplayName.Should().Be("0.0");
    }

    [Test]
    public void UnderlyingVersion_NoChange()
    {
        // Arrange
        var changedProperties = new List<string>();
        var vm = new VersionModel();
        vm.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

        // Act
        vm.UnderlyingVersion = new Version(0, 0);

        // Assert
        vm.UnderlyingVersion.Should().Be(new Version(0, 0));
        changedProperties.Should().BeEmpty();
        vm.DisplayName.Should().Be("0.0");
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
        vm.UnderlyingVersion.Should().BeSameAs(newVersion);
        changedProperties.Should().HaveCount(2);
        changedProperties[0].Should().Be(nameof(VersionModel.UnderlyingVersion));
        changedProperties[1].Should().Be(nameof(VersionModel.DisplayName));
        vm.DisplayName.Should().Be("1.2.3.4");
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
        vm.IsNewestVersion.Should().BeFalse();
        changedProperties.Should().BeEmpty();
        vm.DisplayName.Should().Be("0.0");
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
        vm.IsNewestVersion.Should().BeTrue();
        changedProperties.Should().HaveCount(2);
        changedProperties[0].Should().Be(nameof(VersionModel.IsNewestVersion));
        changedProperties[1].Should().Be(nameof(VersionModel.DisplayName));
        vm.DisplayName.Should().Be("0.0 (newest)");
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
        vm.UnderlyingVersion.Should().BeSameAs(newVersion);
        vm.IsNewestVersion.Should().BeTrue();
        changedProperties.Should().HaveCount(4);
        changedProperties[0].Should().Be(nameof(VersionModel.UnderlyingVersion));
        changedProperties[1].Should().Be(nameof(VersionModel.DisplayName));
        changedProperties[2].Should().Be(nameof(VersionModel.IsNewestVersion));
        changedProperties[3].Should().Be(nameof(VersionModel.DisplayName));
        vm.DisplayName.Should().Be("3.2.3 (newest)");
    }
}