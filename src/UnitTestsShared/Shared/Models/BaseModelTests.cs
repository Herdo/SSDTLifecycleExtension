namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

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
        hasErrors.Should().BeFalse();
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
        invokedSender.Should().NotBeNull();
        invokedSender.Should().BeSameAs(model);
        invokedProperty.Should().NotBeNull();
        invokedProperty.Should().Be(nameof(BaseModelTestImplementation.FakeError));
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
        senderList.Should().HaveCount(2);
        senderList[0].Should().BeSameAs(model);
        senderList[1].Should().BeSameAs(model);
        propertyList.Should().HaveCount(2);
        propertyList[0].Should().Be(nameof(BaseModelTestImplementation.FakeError));
        propertyList[1].Should().Be(nameof(BaseModelTestImplementation.FakeError));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void GetErrors_EmptyListIfNoPropertyNameIsProvided(string propertyName)
    {
        // Arrange
        var model = new BaseModelTestImplementation { FakeError = true };

        // Act
        var errors = model.GetErrors(propertyName);

        // Assert
        errors.Should().NotBeNull();
        errors.OfType<string>().Should().BeEmpty();
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
        errors.Should().NotBeNull();
        errors.OfType<string>().Should().BeEmpty();
    }

    [Test]
    public void GetErrors_FilledErrorCollection()
    {
        // Arrange
        var model = new BaseModelTestImplementation { FakeError = true };

        // Act
        var errors = model.GetErrors(nameof(BaseModelTestImplementation.FakeError));

        // Assert
        errors.Should().NotBeNull();
        var errorArray = errors.OfType<string>().ToArray();
        errorArray.Should().HaveCount(2);
        errorArray[0].Should().Be("Error1");
        errorArray[1].Should().Be("Error2");
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
        hasErrors.Should().Be(expectedHasErrorsState);
    }

    [Test]
    public void SetValidationErrors_ArgumentNullException_PropertyName()
    {
        // Arrange
        var model = new BaseModelTestImplementation();

        // Act & Assert
        Action act = () => model.InvokeSetValidationErrorsWithPropertyNameNull();
        act.Should().Throw<ArgumentNullException>();
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
            return Array.Empty<string>();
        }
    }
}
