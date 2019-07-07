using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Models
{
    using System.Collections.Generic;
    using SSDTLifecycleExtension.Shared.Models;

    [TestFixture]
    public class ConfigurationModelTests
    {
        [Test]
        public void GetDefault_ReturnsInstance()
        {
            // Act
            var model = ConfigurationModel.GetDefault();

            // Assert
            Assert.IsNotNull(model);
        }

        [Test]
        public void GetDefault_DefaultInstanceShouldBeValid()
        {
            // Arrange
            var model = ConfigurationModel.GetDefault();

            // Act
            model.ValidateAll();

            // Assert
            Assert.IsFalse(model.HasErrors);
        }

        [Test]
        public void ArtifactsPath_Get_Set_PropertyChanged_ErrorsChanged()
        {
            // Arrange
            const string testValue = "_Deployment_Pipeline";
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.ArtifactsPath = testValue;
            var returnedValue = model.ArtifactsPath;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.ArtifactsPath), invokedPropertyChangedProperty);
            Assert.AreSame(model, invokedErrorsChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.ArtifactsPath), invokedErrorsChangedProperty);
        }

        [Test]
        public void PublishProfilePath_Get_Set_PropertyChanged_ErrorsChanged()
        {
            // Arrange
            const string testValue = "test.profile.xml";
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.PublishProfilePath = testValue;
            var returnedValue = model.PublishProfilePath;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.PublishProfilePath), invokedPropertyChangedProperty);
            Assert.AreSame(model, invokedErrorsChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.PublishProfilePath), invokedErrorsChangedProperty);
        }

        [Test]
        public void BuildBeforeScriptCreation_Get_Set_PropertyChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.BuildBeforeScriptCreation = testValue;
            var returnedValue = model.BuildBeforeScriptCreation;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.BuildBeforeScriptCreation), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void CreateDocumentationWithScriptCreation_Get_Set_PropertyChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.CreateDocumentationWithScriptCreation = testValue;
            var returnedValue = model.CreateDocumentationWithScriptCreation;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.CreateDocumentationWithScriptCreation), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void CommentOutUnnamedDefaultConstraintDrops_Get_Set_PropertyChanged_ErrorsChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            var invokedErrorsChangedSenderList = new List<object>();
            var invokedErrorsChangedPropertyList = new List<string>();
            model.ErrorsChanged += (sender,
                                    args) =>
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
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops), invokedPropertyChangedProperty);
            Assert.AreEqual(2, invokedErrorsChangedSenderList.Count);
            Assert.AreSame(model, invokedErrorsChangedSenderList[0]);
            Assert.AreSame(model, invokedErrorsChangedSenderList[1]);
            Assert.AreEqual(2, invokedErrorsChangedPropertyList.Count);
            Assert.AreEqual(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops), invokedErrorsChangedPropertyList[0]);
            Assert.AreEqual(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops), invokedErrorsChangedPropertyList[1]);
        }

        [Test]
        public void ReplaceUnnamedDefaultConstraintDrops_Get_Set_PropertyChanged_ErrorsChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            var invokedErrorsChangedSenderList = new List<object>();
            var invokedErrorsChangedPropertyList = new List<string>();
            model.ErrorsChanged += (sender,
                                    args) =>
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
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops), invokedPropertyChangedProperty);
            Assert.AreEqual(2, invokedErrorsChangedSenderList.Count);
            Assert.AreSame(model, invokedErrorsChangedSenderList[0]);
            Assert.AreSame(model, invokedErrorsChangedSenderList[1]);
            Assert.AreEqual(2, invokedErrorsChangedPropertyList.Count);
            Assert.AreEqual(nameof(ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops), invokedErrorsChangedPropertyList[0]);
            Assert.AreEqual(nameof(ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops), invokedErrorsChangedPropertyList[1]);
        }

        [Test]
        public void VersionPattern_Get_Set_PropertyChanged_ErrorsChanged()
        {
            // Arrange
            const string testValue = "{MAJOR}.0.0";
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.VersionPattern = testValue;
            var returnedValue = model.VersionPattern;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.VersionPattern), invokedPropertyChangedProperty);
            Assert.AreSame(model, invokedErrorsChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.VersionPattern), invokedErrorsChangedProperty);
        }

        [Test]
        public void TrackDacpacVersion_Get_Set_PropertyChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.TrackDacpacVersion = testValue;
            var returnedValue = model.TrackDacpacVersion;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.TrackDacpacVersion), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void CustomHeader_Get_Set_PropertyChanged()
        {
            // Arrange
            const string testValue = "-- Short header";
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.CustomHeader = testValue;
            var returnedValue = model.CustomHeader;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.CustomHeader), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void CustomFooter_Get_Set_PropertyChanged()
        {
            // Arrange
            const string testValue = "-- Short footer";
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.CustomFooter = testValue;
            var returnedValue = model.CustomFooter;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.CustomFooter), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void RemoveSqlCmdStatements_Get_Set_PropertyChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.RemoveSqlCmdStatements = testValue;
            var returnedValue = model.RemoveSqlCmdStatements;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.RemoveSqlCmdStatements), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void DeleteRefactorlogAfterVersionedScriptGeneration_Get_Set_PropertyChanged()
        {
            // Arrange
            const bool testValue = true;
            var model = new ConfigurationModel();
            object invokedPropertyChangedSender = null;
            string invokedPropertyChangedProperty = null;
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedPropertyChangedSender = sender;
                invokedPropertyChangedProperty = args?.PropertyName;
            };
            object invokedErrorsChangedSender = null;
            string invokedErrorsChangedProperty = null;
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                invokedErrorsChangedSender = sender;
                invokedErrorsChangedProperty = args?.PropertyName;
            };

            // Act
            model.DeleteRefactorlogAfterVersionedScriptGeneration = testValue;
            var returnedValue = model.DeleteRefactorlogAfterVersionedScriptGeneration;

            // Assert
            Assert.AreEqual(testValue, returnedValue);
            Assert.AreSame(model, invokedPropertyChangedSender);
            Assert.AreEqual(nameof(ConfigurationModel.DeleteRefactorlogAfterVersionedScriptGeneration), invokedPropertyChangedProperty);
            Assert.IsNull(invokedErrorsChangedSender);
            Assert.IsNull(invokedErrorsChangedProperty);
        }

        [Test]
        public void Copy_DifferentInstanceWithSameValues()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };

            // Act
            var copy = model.Copy();

            // Assert
            Assert.IsNotNull(copy);
            Assert.AreNotSame(model, copy);
            Assert.AreEqual(model.ArtifactsPath, copy.ArtifactsPath);
            Assert.AreEqual(model.PublishProfilePath, copy.PublishProfilePath);
            Assert.AreEqual(model.ReplaceUnnamedDefaultConstraintDrops, copy.ReplaceUnnamedDefaultConstraintDrops);
            Assert.AreEqual(model.VersionPattern, copy.VersionPattern);
            Assert.AreEqual(model.CommentOutUnnamedDefaultConstraintDrops, copy.CommentOutUnnamedDefaultConstraintDrops);
            Assert.AreEqual(model.CreateDocumentationWithScriptCreation, copy.CreateDocumentationWithScriptCreation);
            Assert.AreEqual(model.CustomHeader, copy.CustomHeader);
            Assert.AreEqual(model.CustomFooter, copy.CustomFooter);
            Assert.AreEqual(model.BuildBeforeScriptCreation, copy.BuildBeforeScriptCreation);
            Assert.AreEqual(model.TrackDacpacVersion, copy.TrackDacpacVersion);
            Assert.AreEqual(model.RemoveSqlCmdStatements, copy.RemoveSqlCmdStatements);
        }

        [Test]
        public void Equals_FalseWhenNull()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };

            // Act
            var areEqual = model.Equals(null);

            // Assert
            Assert.IsFalse(areEqual);
        }

        [Test]
        public void Equals_TrueWhenSameInstance()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };

            // Act
            var areEqual = model.Equals(model);

            // Assert
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void Equals_TrueWhenEqualInstances()
        {
            // Arrange
            var model1 = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };
            var model2 = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };

            // Act
            var areEqual = model1.Equals(model2);

            // Assert
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void Equals_TrueWhenUnequalInstances()
        {
            // Arrange
            var model1 = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };
            var model2 = new ConfigurationModel
            {
                ArtifactsPath = "TestPath1",
                PublishProfilePath = "TestPath2",
                ReplaceUnnamedDefaultConstraintDrops = true,
                VersionPattern = "TestPattern",
                CommentOutUnnamedDefaultConstraintDrops = true,
                CreateDocumentationWithScriptCreation = false,
                CustomHeader = "TestHeader",
                CustomFooter = "TestFooter",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = true,
                RemoveSqlCmdStatements = true,
                DeleteRefactorlogAfterVersionedScriptGeneration = true
            };

            // Act
            var areEqual = model1.Equals(model2);

            // Assert
            Assert.IsFalse(areEqual);
        }
    }
}