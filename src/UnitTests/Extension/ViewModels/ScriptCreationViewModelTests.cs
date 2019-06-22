using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.ViewModels;

    [TestFixture]
    public class ScriptCreationViewModelTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Project()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(null, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ConfigurationService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScaffoldingService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScriptCreationService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectInitialization()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();

            // Act
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock);

            // Assert
            Assert.IsNotNull(vm.ExistingVersions);
            Assert.AreEqual(0, vm.ExistingVersions.Count);
            Assert.IsNotNull(vm.ScaffoldDevelopmentVersionCommand);
            Assert.IsNotNull(vm.ScaffoldCurrentProductionVersionCommand);
            Assert.IsNotNull(vm.StartLatestCreationCommand);
            Assert.IsNotNull(vm.StartVersionedCreationCommand);
        }

        [Test]
        public void SelectedBaseVersion_NoPropertyChangedForSameInstance()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var version = new VersionModel();
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock)
            {
                SelectedBaseVersion = version
            };
            object invokedSender = null;
            string invokedProperty = null;
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
            };

            // Act
            vm.SelectedBaseVersion = version;

            // Assert
            Assert.IsNull(invokedSender);
            Assert.IsNull(invokedProperty);
        }

        [Test]
        public void SelectedBaseVersion_Get_Set_PropertyChanged()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var version = new VersionModel();
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock);
            object invokedSender = null;
            string invokedProperty = null;
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
            };

            // Act
            vm.SelectedBaseVersion = version;
            var setVersion = vm.SelectedBaseVersion;

            // Assert
            Assert.AreSame(vm, invokedSender);
            Assert.AreSame(version, setVersion);
            Assert.AreEqual(nameof(ScriptCreationViewModel.SelectedBaseVersion), invokedProperty);
        }

        [Test]
        public void ArtifactCommands_CanExecute_NotWhenNotInitialized()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock);

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
        }

        [Test]
        public async Task ArtifactCommands_CanExecute_NotWhenConfigurationHasErrors_Async()
        {
            // Arrange
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\1.2.3.4"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock, vsaMock, fsaMock.Object, loggerMock);
            await vm.InitializeAsync();

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
        }

        [Test]
        public async Task ArtifactCommands_CanExecute_NotWhenScaffoldingIsInProgress_Async()
        {
            // Arrange
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = new Mock<IScaffoldingService>();
            ssMock.SetupGet(m => m.IsScaffolding)
                  .Returns(true);
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\1.2.3.4"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock, vsaMock, fsaMock.Object, loggerMock);
            await vm.InitializeAsync();

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
        }

        [Test]
        public async Task ArtifactCommands_CanExecute_AfterScaffoldingCompleted_Async()
        {
            // Arrange
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = new Mock<IScaffoldingService>();
            ssMock.SetupGet(m => m.IsScaffolding)
                  .Returns(true);
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\1.2.3.4"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock, vsaMock, fsaMock.Object, loggerMock);
            await vm.InitializeAsync();

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };
            ssMock.SetupGet(m => m.IsScaffolding)
                  .Returns(false);
            ssMock.Raise(service => service.IsScaffoldingChanged += null, EventArgs.Empty);
            var canExecuteListAfterCompletion = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
            Assert.IsTrue(canExecuteListAfterCompletion.All(m => m));
        }

        [Test]
        public async Task ArtifactCommands_CanExecute_NotWhenScriptCreationIsInProgress_Async()
        {
            // Arrange
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\1.2.3.4"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock, fsaMock.Object, loggerMock);
            await vm.InitializeAsync();

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
        }

        [Test]
        public async Task ArtifactCommands_CanExecute_AfterScriptCreationCompleted_Async()
        {
            // Arrange
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\1.2.3.4"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock, fsaMock.Object, loggerMock);
            await vm.InitializeAsync();

            // Act
            var canExecuteList = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(false);
            scsMock.Raise(service => service.IsCreatingChanged += null, EventArgs.Empty);
            var canExecuteListAfterCompletion = new[]
            {
                vm.ScaffoldDevelopmentVersionCommand.CanExecute(),
                vm.ScaffoldCurrentProductionVersionCommand.CanExecute(),
                vm.StartLatestCreationCommand.CanExecute(),
                vm.StartVersionedCreationCommand.CanExecute()
            };

            // Assert
            Assert.IsTrue(canExecuteList.All(m => m == false));
            Assert.IsTrue(canExecuteListAfterCompletion.All(m => m));
        }

        [Test]
        public async Task InitializeAsync_ShowModalError_NoProjectDirectory_Async()
        {
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock.Object, fsaMock.Object, loggerMock);

            // Act
            var initialized = await vm.InitializeAsync();

            // Assert
            Assert.IsFalse(initialized);
            vsaMock.Verify(m => m.ShowModalError("ERROR: Cannot determine project directory."), Times.Once);
        }

        [Test]
        public async Task InitializeAsync_ShowModalError_ExceptionGettingDirectories_Async()
        {
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
                   .Throws(new InvalidOperationException("test exception"));
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock.Object, fsaMock.Object, loggerMock);

            // Act
            var initialized = await vm.InitializeAsync();

            // Assert
            Assert.IsFalse(initialized);
            vsaMock.Verify(m => m.ShowModalError("ERROR: Failed to open script creation window: test exception"), Times.Once);
        }

        [Test]
        public async Task InitializeAsync_NoValidDirectories_Async()
        {
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
                   .Returns(new []
                    {
                        @"C:\TestProject\_Deployment\foo"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock.Object, fsaMock.Object, loggerMock);

            // Act
            var initialized = await vm.InitializeAsync();

            // Assert
            Assert.IsTrue(initialized);
            Assert.AreEqual(0, vm.ExistingVersions.Count);
            Assert.IsNull(vm.SelectedBaseVersion);
        }

        [Test]
        public async Task InitializeAsync_ValidDirectories_SelectHighest_Async()
        {
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
                   .Returns(new[]
                    {
                        @"C:\TestProject\_Deployment\4.0.0",
                        @"C:\TestProject\_Deployment\5.0"
                    });
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock.Object, fsaMock.Object, loggerMock);

            // Act
            var initialized = await vm.InitializeAsync();

            // Assert
            Assert.IsTrue(initialized);
            Assert.AreEqual(2, vm.ExistingVersions.Count);
            Assert.IsNotNull(vm.SelectedBaseVersion);
            Assert.AreSame(vm.SelectedBaseVersion, vm.ExistingVersions[0]);
            Assert.IsTrue(vm.ExistingVersions[0].IsNewestVersion);
            Assert.AreEqual(new Version(5, 0), vm.ExistingVersions[0].UnderlyingVersion);
            Assert.IsFalse(vm.ExistingVersions[1].IsNewestVersion);
            Assert.AreEqual(new Version(4, 0, 0), vm.ExistingVersions[1].UnderlyingVersion);
        }

        [Test]
        public async Task InitializeAsync_ShowModalError_InvalidConfig_Async()
        {
            var config = new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = "TestProfile.publish.xml",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "1.2.3.4",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
                  .ReturnsAsync(config);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating)
                   .Returns(true);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
                   .Returns(new string[0]);
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock, scsMock.Object, vsaMock.Object, fsaMock.Object, loggerMock);

            // Act
            var initialized = await vm.InitializeAsync();

            // Assert
            Assert.IsTrue(initialized);
            vsaMock.Verify(m => m.ShowModalError("The SSDT Lifecycle configuration for this project is not correct. " +
                                                 "Please verify that the SSDT Lifecycle configuration for this project exists and has no errors."), Times.Once);
        }
    }
}