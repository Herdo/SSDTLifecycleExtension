using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SSDTLifecycleExtension.Shared.Models;

    [TestFixture]
    public class BaseModelTests
    {
        [Test]
        public void NoValidationErrorsWithoutExplicitValidation()
        {
            // Arrange
            var model = new BaseModelTestImplementation();

            // Act
            var hasErrors = model.HasErrors;

            // Assert
            Assert.IsFalse(hasErrors);
        }

        [Test]
        public void PropertyChanged_Invoked()
        {
            // Arrange
            object invokedSender = null;
            string invokedProperty = null;
            var model = new BaseModelTestImplementation();
            model.PropertyChanged += (sender,
                                      args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
            };

            // Act
            model.FakeError = true;

            // Assert
            Assert.IsNotNull(invokedSender);
            Assert.AreSame(model, invokedSender);
            Assert.IsNotNull(invokedProperty);
            Assert.AreEqual(nameof(BaseModelTestImplementation.FakeError), invokedProperty);
        }

        [Test]
        public void ErrorsChanged_Invoked()
        {
            // Arrange
            var senderList = new List<object>();
            var propertyList = new List<string>();
            var model = new BaseModelTestImplementation();
            model.ErrorsChanged += (sender,
                                    args) =>
            {
                if (sender != null)
                    senderList.Add(sender);
                if (args?.PropertyName != null)
                    propertyList.Add(args.PropertyName);
            };

            // Act
            model.FakeError = true;
            model.FakeError = false;

            // Assert
            Assert.AreEqual(2, senderList.Count);
            Assert.AreSame(model, senderList[0]);
            Assert.AreSame(model, senderList[1]);
            Assert.AreEqual(2, propertyList.Count);
            Assert.AreEqual(nameof(BaseModelTestImplementation.FakeError), propertyList[0]);
            Assert.AreEqual(nameof(BaseModelTestImplementation.FakeError), propertyList[1]);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void GetErrors_EmptyListIfNoPropertyNameIsProvided(string propertyName)
        {
            // Arrange
            var model = new BaseModelTestImplementation {FakeError = true};

            // Act
            var errors = model.GetErrors(propertyName);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.OfType<string>().Count());
        }

        [Test]
        [TestCase("foobar")]
        [TestCase("foo")]
        [TestCase("bar")]
        public void GetErrors_EmptyListIfUnknownPropertyNameIsProvided(string propertyName)
        {
            // Arrange
            var model = new BaseModelTestImplementation { FakeError = true };

            // Act
            var errors = model.GetErrors(propertyName);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.OfType<string>().Count());
        }

        [Test]
        public void GetErrors_FilledErrorCollection()
        {
            // Arrange
            var model = new BaseModelTestImplementation { FakeError = true };

            // Act
            var errors = model.GetErrors(nameof(BaseModelTestImplementation.FakeError));

            // Assert
            Assert.IsNotNull(errors);
            var errorArray = errors.OfType<string>().ToArray();
            Assert.AreEqual(2, errorArray.Length);
            Assert.AreEqual("Error1", errorArray[0]);
            Assert.AreEqual("Error2", errorArray[1]);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void HasErrors_CorrectResult(bool fakeError, bool expectedHasErrorsState)
        {
            // Arrange
            var model = new BaseModelTestImplementation { FakeError = fakeError };

            // Act
            var hasErrors = model.HasErrors;

            // Assert
            Assert.AreEqual(expectedHasErrorsState, hasErrors);
        }

        [Test]
        public void SetValidationErrors_ArgumentNullException_PropertyName()
        {
            // Arrange
            var model = new BaseModelTestImplementation();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => model.InvokeSetValidationErrorsWithPropertyNameNull());
        }

        private class BaseModelTestImplementation : BaseModel
        {
            private bool _fakeError;

            internal bool FakeError
            {
                set
                {
                    if (value == _fakeError) return;
                    _fakeError = value;
                    OnPropertyChanged();
                    SetValidationErrors(GetValidationErrors(value));
                }
            }

            internal void InvokeSetValidationErrorsWithPropertyNameNull()
            {
                SetValidationErrors(null, null);
            }

            private static ICollection<string> GetValidationErrors(bool shouldFakeErrors)
            {
                if (shouldFakeErrors)
                {
                    return new List<string>
                    {
                        "Error1",
                        "Error2"
                    };
                }
                return new string[0];
            }
        }
    }
}