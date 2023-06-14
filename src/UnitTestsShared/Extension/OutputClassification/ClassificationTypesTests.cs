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
        Assert.IsTrue(equalAmount, $"There has to be an equal amount of constants ({constants.Length}), static properties ({staticProperties.Length}), and nested types ({nestedTypes.Length}) in the {nameof(ClassificationTypes)} class.");
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
            var classificationName = constant.GetValue(null);

            // Type definition property
            var typeDefinition = staticProperties.SingleOrDefault(m => m.Name == $"{key}TypeDefinition");
            Assert.IsNotNull(typeDefinition, $"Missing type definition for the key '{key}'.");
            var typeDefinitionAttributes = typeDefinition.GetCustomAttributes(false);
            Assert.AreEqual(2, typeDefinitionAttributes.Length, $"Type definition for the key '{key}' has an unexpected amount of attributes.");
            var typeDefinitionExportAttributes = typeDefinitionAttributes.OfType<ExportAttribute>().ToArray();
            var typeDefinitionNameAttributes = typeDefinitionAttributes.OfType<NameAttribute>().ToArray();
            Assert.AreEqual(1, typeDefinitionExportAttributes.Length, $"Type definition for the key '{key}' has an unexpected amount of {typeof(ExportAttribute).FullName} attributes.");
            Assert.AreEqual(1, typeDefinitionNameAttributes.Length, $"Type definition for the key '{key}' has an unexpected amount of {typeof(NameAttribute).FullName} attributes.");
            var typeDefinitionExportType = typeDefinitionExportAttributes[0].ContractType;
            Assert.AreSame(typeof(ClassificationTypeDefinition), typeDefinitionExportType, $"Wrong export type on the type definition property for the key '{key}'.");
            var typeDefinitionName = typeDefinitionNameAttributes[0].Name;
            Assert.AreEqual(classificationName, typeDefinitionName, $"Wrong name attribute on the type definition property for the key '{key}'.");

            // Nested classification class
            var nestedType = nestedTypes.SingleOrDefault(m => m.Name == $"{key}Classification");
            Assert.IsNotNull(nestedType, $"Missing nested type for the key '{key}'.");
            Assert.AreSame(typeof(ClassificationFormatDefinition), nestedType.BaseType);
            var nestedTypeAttributes = nestedType.GetCustomAttributes(false);
            Assert.AreEqual(3, nestedTypeAttributes.Length, $"Nested type for the key '{key}' has an unexpected amount of attributes.");
            var nestedTypeExportAttributes = nestedTypeAttributes.OfType<ExportAttribute>().ToArray();
            var nestedTypeClassificationTypeAttributes = nestedTypeAttributes.OfType<ClassificationTypeAttribute>().ToArray();
            var nestedTypeNameAttributes = nestedTypeAttributes.OfType<NameAttribute>().ToArray();
            Assert.AreEqual(1, nestedTypeExportAttributes.Length, $"Nested type for the key '{key}' has an unexpected amount of {typeof(ExportAttribute).FullName} attributes.");
            Assert.AreEqual(1, nestedTypeClassificationTypeAttributes.Length, $"Nested type for the key '{key}' has an unexpected amount of {typeof(ClassificationTypeAttribute).FullName} attributes.");
            Assert.AreEqual(1, nestedTypeNameAttributes.Length, $"Nested type for the key '{key}' has an unexpected amount of {typeof(NameAttribute).FullName} attributes.");
            var nestedTypeExportType = nestedTypeExportAttributes[0].ContractType;
            Assert.AreSame(typeof(EditorFormatDefinition), nestedTypeExportType, $"Wrong export type on the nested type for the key '{key}'.");
            var nestedTypeClassificationType = nestedTypeClassificationTypeAttributes[0].ClassificationTypeNames;
            Assert.AreEqual(classificationName, nestedTypeClassificationType, $"Wrong classification type on the nested type for the key '{key}'.");
            var nestedTypeName = nestedTypeNameAttributes[0].Name;
            Assert.AreEqual(classificationName, nestedTypeName, $"Wrong name attribute on the nested type for the key '{key}'.");
        }
    }

    [Test]
    public void CorrectCriticalClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.CriticalClassification();

        // Assert
        Assert.AreEqual(Colors.Firebrick, classification.ForegroundColor);
        Assert.IsTrue(classification.IsBold);
    }

    [Test]
    public void CorrectErrorClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.ErrorClassification();

        // Assert
        Assert.AreEqual(Colors.Red, classification.ForegroundColor);
        Assert.IsTrue(classification.IsBold);
    }

    [Test]
    public void CorrectWarningClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.WarningClassification();

        // Assert
        Assert.AreEqual(Colors.DarkOrange, classification.ForegroundColor);
        Assert.IsTrue(classification.IsBold);
    }

    [Test]
    public void CorrectDebugClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.DebugClassification();

        // Assert
        Assert.AreEqual(Colors.Gray, classification.ForegroundColor);
    }

    [Test]
    public void CorrectTraceClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.TraceClassification();

        // Assert
        Assert.AreEqual(Colors.Gray, classification.ForegroundColor);
    }

    [Test]
    public void CorrectDoneClassification()
    {
        // Act
        ClassificationFormatDefinition classification = new ClassificationTypes.DoneClassification();

        // Assert
        Assert.AreEqual(Colors.Green, classification.ForegroundColor);
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
        Assert.AreSame(ctd, ClassificationTypes.CriticalTypeDefinition);
        Assert.AreSame(ctd, ClassificationTypes.ErrorTypeDefinition);
        Assert.AreSame(ctd, ClassificationTypes.WarningTypeDefinition);
        Assert.AreSame(ctd, ClassificationTypes.DebugTypeDefinition);
        Assert.AreSame(ctd, ClassificationTypes.TraceTypeDefinition);
        Assert.AreSame(ctd, ClassificationTypes.DoneTypeDefinition);
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