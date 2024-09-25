namespace SSDTLifecycleExtension.UnitTests.Extension;

[TestFixture]
public class DependencyResolverTests
{
    [Test]
    public void Get_ObjectDisposedException_AfterDisposal()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        dr.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => dr.Get<ILogger>());
    }

    [Test]
    public void Get_GetSameVisualStudioAccess()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        IVisualStudioAccess returnedInstance;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        returnedInstance = dr.Get<IVisualStudioAccess>();

        // Assert
        returnedInstance.Should().BeSameAs(vsaMock);
    }

    [Test]
    public void Get_GetSameLogger()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        ILogger returnedInstance;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        returnedInstance = dr.Get<ILogger>();

        // Assert
        returnedInstance.Should().BeSameAs(loggerMock);
    }

    [Test]
    public void Get_GetSameCommandService()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        OleMenuCommandService returnedInstance;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        returnedInstance = dr.Get<OleMenuCommandService>();

        // Assert
        returnedInstance.Should().BeSameAs(cs);
    }

    [Test]
    public void GetViewModel_ObjectDisposedException_AfterDisposal()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        dr.Dispose();

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ObjectDisposedException>(() => dr.GetViewModel<IViewModel>(null));
    }

    [Test]
    public void GetViewModel_DifferentInstancesForDifferentProjects()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var p1 = new SqlProject("", "", "1");
        var p2 = new SqlProject("", "", "2");
        ViewModelTestImplementation vm1;
        ViewModelTestImplementation vm2;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        {
            vm1 = dr.GetViewModel<ViewModelTestImplementation>(p1);
            vm2 = dr.GetViewModel<ViewModelTestImplementation>(p2);
        }

        // Assert
        vm1.Should().NotBeSameAs(vm2);
        vm1.Project.Should().BeSameAs(p1);
        vm2.Project.Should().BeSameAs(p2);
    }

    [Test]
    public void GetViewModel_SameInstanceForSameProjects_SingleSqlProjectInstance()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var p = new SqlProject("", "", "1");
        ViewModelTestImplementation vm1;
        ViewModelTestImplementation vm2;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        {
            vm1 = dr.GetViewModel<ViewModelTestImplementation>(p);
            vm2 = dr.GetViewModel<ViewModelTestImplementation>(p);
        }

        // Assert
        vm1.Should().BeSameAs(vm2);
        vm1.Project.Should().BeSameAs(p);
        vm2.Project.Should().BeSameAs(p);
    }

    [Test]
    public void GetViewModel_SameInstanceForSameProjects_DifferentSqlProjectInstancesWithSameUniqueName()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var p1 = new SqlProject("", "", "1");
        var p2 = new SqlProject("", "", "1");
        ViewModelTestImplementation vm1;
        ViewModelTestImplementation vm2;
        using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

        // Act
        {
            vm1 = dr.GetViewModel<ViewModelTestImplementation>(p1);
            vm2 = dr.GetViewModel<ViewModelTestImplementation>(p2);
        }

        // Assert
        vm1.Should().BeSameAs(vm2);
        vm1.Project.Should().BeSameAs(p1);
        vm2.Project.Should().BeSameAs(p1);
    }

    [Test]
    public void Dispose_NoExceptionWhenDisposingTwice()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        dr.Dispose();

        // Act & Assert
        Assert.DoesNotThrow(() => dr.Dispose());
    }

    [Test]
    public void RegisterPackage_ObjectDisposedException()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        dr.Dispose();

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ObjectDisposedException>(() => dr.RegisterPackage<AsyncPackageTestImplementation>(null));
    }

    [Test]
    public void RegisterPackage_Successfully()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        var package = new AsyncPackageTestImplementation();

        // Act
        dr.RegisterPackage(package);

        // Assert
        var registeredPackage = dr.Get<AsyncPackageTestImplementation>();
        registeredPackage.Should().BeSameAs(package);
    }

    [Test]
    public void DependencyResolver_AllSharedInterfacesShouldBeConstructable()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        var package = new AsyncPackageTestImplementation();
        dr.RegisterPackage(package);
        var interfacesToConstruct = GetInterfacesToConstruct();
        var getMethod = typeof(DependencyResolver).GetMethod("Get");
        getMethod.Should().NotBeNull();
        var failedConstructions = new List<(string Interface, string Error)>();

        // Act
        foreach (var interfaceToConstruct in interfacesToConstruct)
        {
            var getRef = getMethod.MakeGenericMethod(interfaceToConstruct);
            try
            {
                getRef.Invoke(dr, null);
            }
            catch (Exception e)
            {
                failedConstructions.Add((interfaceToConstruct.FullName, e.ToString()));
            }
        }

        // Assert
        failedConstructions.Should().BeEmpty();
    }

    private static IEnumerable<Type> GetInterfacesToConstruct()
    {
        var sharedAssembly = typeof(SSDTLifecycleExtension.Shared.Constants).Assembly;
        var interfacesToExclude = new []
        {
            typeof(IScriptModifier),
            typeof(IWorkUnit<>),
            typeof(IBaseModel),
            typeof(IStateModel)
        };
        return sharedAssembly.GetTypes()
                             .Where(m => m.IsInterface && !interfacesToExclude.Contains(m))
                             .ToArray();
    }

    private class ViewModelTestImplementation : ViewModelBase
    {
        public SqlProject Project { get; }

        public ViewModelTestImplementation(SqlProject project)
        {
            Project = project;
        }

        public override Task<bool> InitializeAsync()
        {
            throw new NotSupportedException();
        }
    }

    private class AsyncPackageTestImplementation : IAsyncPackage
    {

    }
}