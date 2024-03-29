﻿namespace SSDTLifecycleExtension.UnitTests.Extension.Windows;

[TestFixture]
public class ToolWindowInitializerTests
{
    [Test]
    public void Constructor_ArgumentNullException_VisualStudioAccess()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new ToolWindowInitializer(null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_DependencyResolver()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new ToolWindowInitializer(vsaMock, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void TryInitializeToolWindowAsync_ArgumentNullException_Window()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var dr = new DependencyResolver(vsaMock, loggerMock, cs);
        var twi = new ToolWindowInitializer(vsaMock, dr);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => twi.TryInitializeToolWindowAsync<ViewModelBaseTestImplementationWithSuccessfulInitialization>(null));
    }

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
        Assert.IsFalse(success);
        Assert.IsNull(fullProjectPath);
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
        Assert.IsTrue(success);
        Assert.AreEqual("b", fullProjectPath);
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
        Assert.IsFalse(success);
        Assert.AreEqual("b", fullProjectPath);
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
        Assert.IsTrue(success);
        Assert.AreEqual("b", fullProjectPath);
        viewMock.Verify(m => m.SetDataContext(It.Is<IViewModel>(model => model != null && model.GetType() == typeof(ViewModelBaseTestImplementationWithSuccessfulInitialization))));
    }

    [UsedImplicitly]
    private class ViewModelBaseTestImplementationWithSuccessfulInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }
    }

    [UsedImplicitly]
    private class ViewModelBaseTestImplementationWithFailingInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            return Task.FromResult(false);
        }
    }

    [UsedImplicitly]
    private class ViewModelBaseTestImplementationWithExceptionInitialization : ViewModelBase
    {
        public override Task<bool> InitializeAsync()
        {
            throw new InvalidOperationException();
        }
    }
}