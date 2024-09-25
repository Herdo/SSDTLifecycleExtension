namespace SSDTLifecycleExtension.UnitTests.Extension.Windows;

[TestFixture]
public class ToolWindowInitializerTests
{
    [Test]
    public async Task TryInitializeToolWindowAsync_NoProjectSelected_Async()
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedSqlProject()).Returns(null as SqlProject);
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock.Object, loggerMock, cs);
        var twi = new ToolWindowInitializer(vsaMock.Object, dr);
        var windowMock = Mock.Of<IVisualStudioToolWindow>();

        // Act
        var (success, fullProjectPath) = await twi.TryInitializeToolWindowAsync<ViewModelBaseTestImplementationWithSuccessfulInitialization>(windowMock);

        // Assert
        success.Should().BeFalse();
        fullProjectPath.Should().BeNull();
    }

    [Test]
    public async Task TryInitializeToolWindowAsync_WindowContentIsNoView_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedSqlProject()).Returns(project);
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock.Object, loggerMock, cs);
        var twi = new ToolWindowInitializer(vsaMock.Object, dr);
        var windowMock = new Mock<IVisualStudioToolWindow>();
        windowMock.SetupGet(m => m.Content).Returns(null as object);

        // Act
        var (success, fullProjectPath) = await twi.TryInitializeToolWindowAsync<ViewModelBaseTestImplementationWithExceptionInitialization>(windowMock.Object);

        // Assert
        success.Should().BeTrue();
        fullProjectPath.Should().Be("b");
    }

    [Test]
    public async Task TryInitializeToolWindowAsync_ViewModelInitializationFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedSqlProject()).Returns(project);
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock.Object, loggerMock, cs);
        var twi = new ToolWindowInitializer(vsaMock.Object, dr);
        var viewMock = Mock.Of<IView>();
        var windowMock = new Mock<IVisualStudioToolWindow>();
        windowMock.SetupGet(m => m.Content).Returns(viewMock);

        // Act
        var (success, fullProjectPath) = await twi.TryInitializeToolWindowAsync<ViewModelBaseTestImplementationWithFailingInitialization>(windowMock.Object);

        // Assert
        success.Should().BeFalse();
        fullProjectPath.Should().Be("b");
    }

    [Test]
    public async Task TryInitializeToolWindowAsync_ViewModelInitializationSucceeds_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedSqlProject()).Returns(project);
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock.Object, loggerMock, cs);
        var twi = new ToolWindowInitializer(vsaMock.Object, dr);
        var viewMock = new Mock<IView>();
        var windowMock = new Mock<IVisualStudioToolWindow>();
        windowMock.SetupGet(m => m.Content).Returns(viewMock.Object);

        // Act
        var (success, fullProjectPath) = await twi.TryInitializeToolWindowAsync<ViewModelBaseTestImplementationWithSuccessfulInitialization>(windowMock.Object);

        // Assert
        success.Should().BeTrue();
        fullProjectPath.Should().Be("b");
        viewMock.Verify(m => m.SetDataContext(It.Is<IViewModel>(model => model != null && model.GetType() == typeof(ViewModelBaseTestImplementationWithSuccessfulInitialization))));
    }

    private class ViewModelBaseTestImplementationWithSuccessfulInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }
    }

    private class ViewModelBaseTestImplementationWithFailingInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            return Task.FromResult(false);
        }
    }

    private class ViewModelBaseTestImplementationWithExceptionInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            throw new InvalidOperationException();
        }
    }
}