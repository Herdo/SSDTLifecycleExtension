using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.Shell;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.ViewModels;

    [TestFixture]
    public class DependencyResolverTests
    {
        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DependencyResolver(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DependencyResolver(vsaMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_CommandService()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DependencyResolver(vsaMock, loggerMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

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
            Assert.AreSame(vsaMock, returnedInstance);
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
            Assert.AreSame(loggerMock, returnedInstance);
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
            Assert.AreSame(cs, returnedInstance);
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
        public void GetViewModel_ArgumentNullException_Project()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var spMock = Mock.Of<IServiceProvider>();
            var cs = new OleMenuCommandService(spMock);
            using (var dr = new DependencyResolver(vsaMock, loggerMock, cs))

                // Act & Assert
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => dr.GetViewModel<IViewModel>(null));
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
            Assert.IsNotNull(vm1);
            Assert.IsNotNull(vm2);
            Assert.AreNotSame(vm1, vm2);
            Assert.AreSame(p1, vm1.Project);
            Assert.AreSame(p2, vm2.Project);
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
            Assert.IsNotNull(vm1);
            Assert.IsNotNull(vm2);
            Assert.AreSame(vm1, vm2);
            Assert.AreSame(p, vm1.Project);
            Assert.AreSame(p, vm2.Project);
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
            Assert.IsNotNull(vm1);
            Assert.IsNotNull(vm2);
            Assert.AreSame(vm1, vm2);
            Assert.AreSame(p1, vm1.Project);
            Assert.AreSame(p1, vm2.Project);
        }

        [UsedImplicitly]
        private class ViewModelTestImplementation : ViewModelBase
        {
            public SqlProject Project { get; }

            public ViewModelTestImplementation(SqlProject project)
            {
                Project = project;
            }
        }
    }
}