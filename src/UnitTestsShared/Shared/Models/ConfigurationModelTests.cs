namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

[TestFixture]
public class ConfigurationModelTests
{
    [Test]
    public void GetDefault_ReturnsInstance()
    {
        // Act
        var model = ConfigurationModel.GetDefault();

        // Assert
        model.Should().NotBeNull();
    }

    [Test]
    public void GetDefault_DefaultInstanceShouldBeValid()
    {
        // Arrange
        var model = ConfigurationModel.GetDefault();

        // Act
        model.ValidateAll();

        // Assert
        model.HasErrors.Should().BeFalse();
    }

    [Test]
    public void ArtifactsPath_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const string testValue = "_Deployment_Pipeline";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.ArtifactsPath = testValue;
        var returnedValue = model.ArtifactsPath;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.ArtifactsPath));
        invokedErrorsChangedSender.Should().BeSameAs(model);
        invokedErrorsChangedProperty.Should().Be(nameof(ConfigurationModel.ArtifactsPath));
    }

    [Test]
    public void PublishProfilePath_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const string testValue = "test.profile.xml";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.PublishProfilePath = testValue;
        var returnedValue = model.PublishProfilePath;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.PublishProfilePath));
        invokedErrorsChangedSender.Should().BeSameAs(model);
        invokedErrorsChangedProperty.Should().Be(nameof(ConfigurationModel.PublishProfilePath));
    }

    [Test]
    public void SharedDacpacRepositoryPaths_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const string testValue = "C:\\Test\\Repository\\";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.SharedDacpacRepositoryPaths = testValue;
        var returnedValue = model.SharedDacpacRepositoryPaths;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.SharedDacpacRepositoryPaths));
        invokedErrorsChangedSender.Should().BeSameAs(model);
        invokedErrorsChangedProperty.Should().Be(nameof(ConfigurationModel.SharedDacpacRepositoryPaths));
    }

    [Test]
    public void BuildBeforeScriptCreation_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = false;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.BuildBeforeScriptCreation = testValue;
        var returnedValue = model.BuildBeforeScriptCreation;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.BuildBeforeScriptCreation));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void CreateDocumentationWithScriptCreation_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = false;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.CreateDocumentationWithScriptCreation = testValue;
        var returnedValue = model.CreateDocumentationWithScriptCreation;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.CreateDocumentationWithScriptCreation));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void CommentOutUnnamedDefaultConstraintDrops_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const bool testValue = true;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        var invokedErrorsChangedSenderList = new List<object>();
        var invokedErrorsChangedPropertyList = new List<string>();
        model.ErrorsChanged += (sender, args) =>
        {
            if (sender != null)
                invokedErrorsChangedSenderList.Add(sender);
            if (args != null)
                invokedErrorsChangedPropertyList.Add(args.PropertyName);
        };

        // Act
        model.CommentOutUnnamedDefaultConstraintDrops = testValue;
        var returnedValue = model.CommentOutUnnamedDefaultConstraintDrops;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops));
        invokedErrorsChangedSenderList.Should().HaveCount(2);
        invokedErrorsChangedSenderList[0].Should().BeSameAs(model);
        invokedErrorsChangedSenderList[1].Should().BeSameAs(model);
        invokedErrorsChangedPropertyList.Should().HaveCount(2);
        invokedErrorsChangedPropertyList[0].Should().Be(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops));
        invokedErrorsChangedPropertyList[1].Should().Be(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops));
    }

    [Test]
    public void ReplaceUnnamedDefaultConstraintDrops_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const bool testValue = true;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        var invokedErrorsChangedSenderList = new List<object>();
        var invokedErrorsChangedPropertyList = new List<string>();
        model.ErrorsChanged += (sender, args) =>
        {
            if (sender != null)
                invokedErrorsChangedSenderList.Add(sender);
            if (args != null)
                invokedErrorsChangedPropertyList.Add(args.PropertyName);
        };

        // Act
        model.ReplaceUnnamedDefaultConstraintDrops = testValue;
        var returnedValue = model.ReplaceUnnamedDefaultConstraintDrops;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops));
        invokedErrorsChangedSenderList.Should().HaveCount(2);
        invokedErrorsChangedSenderList[0].Should().BeSameAs(model);
        invokedErrorsChangedSenderList[1].Should().BeSameAs(model);
        invokedErrorsChangedPropertyList.Should().HaveCount(2);
        invokedErrorsChangedPropertyList[0].Should().Be(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops));
        invokedErrorsChangedPropertyList[1].Should().Be(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops));
    }

    [Test]
    public void VersionPattern_Get_Set_PropertyChanged_ErrorsChanged()
    {
        // Arrange
        const string testValue = "{MAJOR}.0.0";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.VersionPattern = testValue;
        var returnedValue = model.VersionPattern;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.VersionPattern));
        invokedErrorsChangedSender.Should().BeSameAs(model);
        invokedErrorsChangedProperty.Should().Be(nameof(ConfigurationModel.VersionPattern));
    }

    [Test]
    public void TrackDacpacVersion_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = true;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.TrackDacpacVersion = testValue;
        var returnedValue = model.TrackDacpacVersion;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.TrackDacpacVersion));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void CustomHeader_Get_Set_PropertyChanged()
    {
        // Arrange
        const string testValue = "-- Short header";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.CustomHeader = testValue;
        var returnedValue = model.CustomHeader;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.CustomHeader));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void CustomFooter_Get_Set_PropertyChanged()
    {
        // Arrange
        const string testValue = "-- Short footer";
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.CustomFooter = testValue;
        var returnedValue = model.CustomFooter;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.CustomFooter));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void RemoveSqlCmdStatements_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = true;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.RemoveSqlCmdStatements = testValue;
        var returnedValue = model.RemoveSqlCmdStatements;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.RemoveSqlCmdStatements));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void DeleteRefactorlogAfterVersionedScriptGeneration_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = true;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.DeleteRefactorlogAfterVersionedScriptGeneration = testValue;
        var returnedValue = model.DeleteRefactorlogAfterVersionedScriptGeneration;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.DeleteRefactorlogAfterVersionedScriptGeneration));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void DeleteLatestAfterVersionedScriptGeneration_Get_Set_PropertyChanged()
    {
        // Arrange
        const bool testValue = false;
        var model = new ConfigurationModel();
        object invokedPropertyChangedSender = null;
        string invokedPropertyChangedProperty = null;
        model.PropertyChanged += (sender, args) =>
        {
            invokedPropertyChangedSender = sender;
            invokedPropertyChangedProperty = args?.PropertyName;
        };
        object invokedErrorsChangedSender = null;
        string invokedErrorsChangedProperty = null;
        model.ErrorsChanged += (sender, args) =>
        {
            invokedErrorsChangedSender = sender;
            invokedErrorsChangedProperty = args?.PropertyName;
        };

        // Act
        model.DeleteLatestAfterVersionedScriptGeneration = testValue;
        var returnedValue = model.DeleteLatestAfterVersionedScriptGeneration;

        // Assert
        returnedValue.Should().Be(testValue);
        invokedPropertyChangedSender.Should().BeSameAs(model);
        invokedPropertyChangedProperty.Should().Be(nameof(ConfigurationModel.DeleteLatestAfterVersionedScriptGeneration));
        invokedErrorsChangedSender.Should().BeNull();
        invokedErrorsChangedProperty.Should().BeNull();
    }

    [Test]
    public void Copy_DifferentInstanceWithSameValues()
    {
        // Arrange
        var model = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };

        // Act
        var copy = model.Copy();

        // Assert
        copy.Should().NotBeNull();
        copy.Should().NotBeSameAs(model);
        copy.ArtifactsPath.Should().Be(model.ArtifactsPath);
        copy.PublishProfilePath.Should().Be(model.PublishProfilePath);
        copy.SharedDacpacRepositoryPaths.Should().Be(model.SharedDacpacRepositoryPaths);
        copy.ReplaceUnnamedDefaultConstraintDrops.Should().Be(model.ReplaceUnnamedDefaultConstraintDrops);
        copy.VersionPattern.Should().Be(model.VersionPattern);
        copy.CommentOutUnnamedDefaultConstraintDrops.Should().Be(model.CommentOutUnnamedDefaultConstraintDrops);
        copy.CreateDocumentationWithScriptCreation.Should().Be(model.CreateDocumentationWithScriptCreation);
        copy.CustomHeader.Should().Be(model.CustomHeader);
        copy.CustomFooter.Should().Be(model.CustomFooter);
        copy.BuildBeforeScriptCreation.Should().Be(model.BuildBeforeScriptCreation);
        copy.TrackDacpacVersion.Should().Be(model.TrackDacpacVersion);
        copy.RemoveSqlCmdStatements.Should().Be(model.RemoveSqlCmdStatements);
        copy.DeleteRefactorlogAfterVersionedScriptGeneration.Should().Be(model.DeleteRefactorlogAfterVersionedScriptGeneration);
        copy.DeleteLatestAfterVersionedScriptGeneration.Should().Be(model.DeleteLatestAfterVersionedScriptGeneration);
    }

    [Test]
    public void Equals_FalseWhenNull()
    {
        // Arrange
        var model = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };

        // Act
        var areEqual = model.Equals(null);

        // Assert
        areEqual.Should().BeFalse();
    }

    [Test]
    public void Equals_TrueWhenSameInstance()
    {
        // Arrange
        var model = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };

        // Act
        var areEqual = model.Equals(model);

        // Assert
        areEqual.Should().BeTrue();
    }

    [Test]
    public void Equals_TrueWhenEqualInstances()
    {
        // Arrange
        var model1 = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };
        var model2 = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };

        // Act
        var areEqual = model1.Equals(model2);

        // Assert
        areEqual.Should().BeTrue();
    }

    [Test]
    public void Equals_TrueWhenUnequalInstances()
    {
        // Arrange
        var model1 = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };
        var model2 = new ConfigurationModel
        {
            ArtifactsPath = "TestPath1",
            PublishProfilePath = "TestPath2",
            SharedDacpacRepositoryPaths = "C:\\Temp\\Repository\\",
            ReplaceUnnamedDefaultConstraintDrops = true,
            VersionPattern = "TestPattern",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true,
            DeleteRefactorlogAfterVersionedScriptGeneration = true,
            DeleteLatestAfterVersionedScriptGeneration = true
        };

        // Act
        var areEqual = model1.Equals(model2);

        // Assert
        areEqual.Should().BeFalse();
    }
}