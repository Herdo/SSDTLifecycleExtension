namespace SSDTLifecycleExtension.UnitTests.Extension.OutputClassification;

[TestFixture]
public class ClassificationTypesTests
{
    [Test]
    public void CorrectAmountOfMembers()
    {
        // Arrange
        var t = typeof(ClassificationTypes);

        // Act
        var constants = GetConstants(t);
        var staticProperties = GetStaticProperties(t);
        var nestedTypes = GetNestedTypes(t);

        // Assert
        var equalAmount = constants.Length == staticProperties.Length && staticProperties.Length == nestedTypes.Length;
        equalAmount.Should().BeTrue();
    }

    [Test]
    public void CorrectAttributes()
    {
        // Arrange
        var t = typeof(ClassificationTypes);

        // Act & Assert
        var constants = GetConstants(t);
        var staticProperties = GetStaticProperties(t);
        var nestedTypes = GetNestedTypes(t);
        foreach (var constant in constants)
        {
            // Constant
            var key = constant.Name;
            var classificationName = (string)constant.GetValue(null);

            // Type definition property
            var typeDefinition = staticProperties.SingleOrDefault(m => m.Name == $"{key}TypeDefinition");
            typeDefinition.Should().NotBeNull();
            var typeDefinitionAttributes = typeDefinition.GetCustomAttributes(false);
            typeDefinitionAttributes.Should().HaveCount(2);
            var typeDefinitionExportAttributes = typeDefinitionAttributes.OfType<ExportAttribute>().ToArray();
            var typeDefinitionNameAttributes = typeDefinitionAttributes.OfType<NameAttribute>().ToArray();
            typeDefinitionExportAttributes.Should().ContainSingle();
            typeDefinitionNameAttributes.Should().ContainSingle();
            var typeDefinitionExportType = typeDefinitionExportAttributes[0].ContractType;
            typeDefinitionExportType.Should().Be(typeof(ClassificationTypeDefinition));
            var typeDefinitionName = typeDefinitionNameAttributes[0].Name;
            typeDefinitionName.Should().Be(classificationName);

            // Nested classification class
            var nestedType = nestedTypes.SingleOrDefault(m => m.Name == $"{key}Classification");
            nestedType.Should().NotBeNull();
            nestedType.BaseType.Should().Be(typeof(ClassificationFormatDefinition));
            var nestedTypeAttributes = nestedType.GetCustomAttributes(false);
            nestedTypeAttributes.Should().HaveCountGreaterThanOrEqualTo(3);
            var nestedTypeExportAttributes = nestedTypeAttributes.OfType<ExportAttribute>().ToArray();
            var nestedTypeClassificationTypeAttributes = nestedTypeAttributes.OfType<ClassificationTypeAttribute>().ToArray();
            var nestedTypeNameAttributes = nestedTypeAttributes.OfType<NameAttribute>().ToArray();
            nestedTypeExportAttributes.Should().ContainSingle();
            nestedTypeClassificationTypeAttributes.Should().ContainSingle();
            nestedTypeNameAttributes.Should().ContainSingle();
            var nestedTypeExportType = nestedTypeExportAttributes[0].ContractType;
            nestedTypeExportType.Should().Be(typeof(EditorFormatDefinition));
            var nestedTypeClassificationType = nestedTypeClassificationTypeAttributes[0].ClassificationTypeNames;
            nestedTypeClassificationType.Should().Be(classificationName);
            var nestedTypeName = nestedTypeNameAttributes[0].Name;
            nestedTypeName.Should().Be(classificationName);
        }
    }

    [Test]
    public void CorrectCriticalClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.CriticalClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.Firebrick);
        classification.IsBold.Should().BeTrue();
    }

    [Test]
    public void CorrectErrorClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.ErrorClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.Red);
        classification.IsBold.Should().BeTrue();
    }

    [Test]
    public void CorrectWarningClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.WarningClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.DarkOrange);
        classification.IsBold.Should().BeTrue();
    }

    [Test]
    public void CorrectDebugClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.DebugClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.Gray);
    }

    [Test]
    public void CorrectTraceClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.TraceClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.Gray);
    }

    [Test]
    public void CorrectDoneClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.DoneClassification();

        // Assert
        classification.ForegroundColor.Should().Be(Colors.Green);
    }

    [Test]
    public void GetAndSetForAllTypeDefinitions()
    {
        // Arrange
        var ctd = new ClassificationTypeDefinition();

        // Act
        ClassificationTypes.CriticalTypeDefinition = ctd;
        ClassificationTypes.ErrorTypeDefinition = ctd;
        ClassificationTypes.WarningTypeDefinition = ctd;
        ClassificationTypes.DebugTypeDefinition = ctd;
        ClassificationTypes.TraceTypeDefinition = ctd;
        ClassificationTypes.DoneTypeDefinition = ctd;

        // Assert
        ClassificationTypes.CriticalTypeDefinition.Should().BeSameAs(ctd);
        ClassificationTypes.ErrorTypeDefinition.Should().BeSameAs(ctd);
        ClassificationTypes.WarningTypeDefinition.Should().BeSameAs(ctd);
        ClassificationTypes.DebugTypeDefinition.Should().BeSameAs(ctd);
        ClassificationTypes.TraceTypeDefinition.Should().BeSameAs(ctd);
        ClassificationTypes.DoneTypeDefinition.Should().BeSameAs(ctd);
    }

    private static FieldInfo[] GetConstants(IReflect type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                   .Where(m => m.IsLiteral && !m.IsInitOnly)
                   .ToArray();
    }

    private static PropertyInfo[] GetStaticProperties(IReflect type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Static);
    }

    private static Type[] GetNestedTypes(Type type)
    {
        return type.GetNestedTypes(BindingFlags.Public);
    }
}
