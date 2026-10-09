using BDO.GWT.GLOBAL.API.Constants;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.MacroEngine;
using Moq;
using System.Collections.Immutable;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class PageContentValidatorTests
{
    private static readonly string[] s_abcValues = ["a", "b", "c"];
    private static readonly string[] s_oneTwoValues = ["one", "two"];

    [Test]
    public void GetClassNameFromPageContentType_ResolvesGeneratedXbyKContentType()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetClassNameFromPageContentType");

        var className = method.Invoke(null, [typeof(DealPageContent)]);

        Assert.That(className, Is.EqualTo(Deal.CONTENT_TYPE_NAME));
    }

    [TestCase("MetadataShowTitle", nameof(DealPageContent.ShowTitle))]
    [TestCase("DealMetadataRssCategory", nameof(DealPageContent.MetadataRssCategory))]
    [TestCase("DealShowSocialMediaButtons", nameof(DealPageContent.ShowSocialMediaButtons))]
    [TestCase("RelevantContactPersonDeal", nameof(DealPageContent.RelevantContactPerson))]
    public void TryGetContentProperty_ResolvesXbyKFieldAliases(string kenticoFieldName, string expectedPropertyName)
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", kenticoFieldName, null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(expectedPropertyName));
        });
    }

    [Test]
    public void TryGetContentProperty_ReturnsFalseForUnmappedField()
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", "FieldThatDoesNotExist", null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.False);
            Assert.That(args[3], Is.Null);
        });
    }

    [Test]
    public void GetClassNameFromPageContentType_ThrowsForTypeWithoutContentTypeName()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetClassNameFromPageContentType");

        var ex = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, [typeof(string)]));

        Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [TestCase("BDO.GWT.Deal", "Deal")]
    [TestCase("BDO.Insight", "Insight")]
    [TestCase("Deal", "Deal")]
    public void GetContentTypeCodeNameSegment_ReturnsLastDotSegment(string className, string expectedSegment)
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetContentTypeCodeNameSegment");

        var segment = method.Invoke(null, [className]);

        Assert.That(segment, Is.EqualTo(expectedSegment));
    }

    [Test]
    public void TryGetContentProperty_ResolvesDirectMatch()
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", nameof(DealPageContent.DealContent), null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(nameof(DealPageContent.DealContent)));
        });
    }

    [TestCase("MetadataTitle", nameof(InsightPageContent.MetadataTitle))]
    [TestCase("InsightMetadataRssCategory", nameof(InsightPageContent.MetadataRssCategory))]
    [TestCase("InsightShowSocialMediaButtons", nameof(InsightPageContent.ShowSocialMediaButtons))]
    [TestCase("RelevantContactPersonInsight", nameof(InsightPageContent.RelevantContactPerson))]
    public void TryGetContentProperty_ResolvesInsightXbyKFieldAliases(string kenticoFieldName, string expectedPropertyName)
    {
        var properties = typeof(InsightPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Insight", kenticoFieldName, null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(expectedPropertyName));
        });
    }

    [Test]
    public void TryGetContentProperty_ReturnsFalseForUnmappedInsightField()
    {
        var properties = typeof(InsightPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Insight", "FieldThatDoesNotExist", null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.False);
            Assert.That(args[3], Is.Null);
        });
    }

    private static MethodInfo GetPrivateStaticMethod(string typeName, string methodName)
    {
        return typeof(PageContentValidator).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static) ??
            throw new AssertionException($"Could not find {typeName}.{methodName}.");
    }

    // ---- Instance-method coverage ---------------------------------------------------

    private static MethodInfo GetPrivateInstanceMethod(string methodName)
    {
        return typeof(PageContentValidator).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance) ??
            throw new AssertionException($"Could not find PageContentValidator.{methodName}.");
    }

    private static PageContentValidator CreateValidator(
        out Mock<IValidationState> validationState,
        out Mock<IMacroResolver> macroResolver)
    {
        validationState = new Mock<IValidationState>();
        macroResolver = new Mock<IMacroResolver>();
        return new PageContentValidator(macroResolver.Object, validationState.Object);
    }

    private static FormFieldInfo CreateField(
        string name = "TestField",
        string dataType = FieldDataType.Text,
        bool allowEmpty = true,
        int size = 0)
    {
        return new FormFieldInfo
        {
            Name = name,
            DataType = dataType,
            AllowEmpty = allowEmpty,
            Size = size,
            Visible = true,
            Enabled = true,
        };
    }

    private static object? InvokeInstance(PageContentValidator validator, string methodName, params object?[] args)
    {
        try
        {
            return GetPrivateInstanceMethod(methodName).Invoke(validator, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    #region GetContentPropertyNameCandidates

    [Test]
    public void GetContentPropertyNameCandidates_ReturnsDistinctCandidates()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetContentPropertyNameCandidates");

        var result = (IEnumerable<string>)method.Invoke(null, ["Deal", "DealMetadataTitle"])!;
        var candidates = result.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(candidates, Does.Contain("DealMetadataTitle"));
            Assert.That(candidates, Does.Contain("MetadataTitle"));
            Assert.That(candidates, Does.Contain("Title"));
            Assert.That(candidates, Is.Unique);
        });
    }

    #endregion

    #region TrimPrefix / TrimSuffix

    [TestCase("MetadataTitle", "Metadata", "Title")]
    [TestCase("Title", "Metadata", "Title")]
    public void TrimPrefix_RemovesPrefixWhenPresent(string value, string prefix, string expected)
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TrimPrefix");

        var result = method.Invoke(null, [value, prefix]);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("TitleDeal", "Deal", "Title")]
    [TestCase("Title", "Deal", "Title")]
    public void TrimSuffix_RemovesSuffixWhenPresent(string value, string suffix, string expected)
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TrimSuffix");

        var result = method.Invoke(null, [value, suffix]);

        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region FormatStringValues

    [Test]
    public void FormatStringValues_QuotesAndJoinsValues()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "FormatStringValues");

        var result = method.Invoke(null, [s_abcValues]);

        Assert.That(result, Is.EqualTo("'a', 'b', 'c'"));
    }

    #endregion

    #region ValidateAndNormalizeString

    [Test]
    public void ValidateAndNormalizeString_EmptyValueWithoutAllowEmpty_AddsError()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(allowEmpty: false);
        Func<FormFieldInfo, string, string> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeString", field, null, passthrough);

        Assert.That(result, Is.Null);
        validationState.Verify(x => x.AddError(field.Name, FormErrorMessage.EmptyValue), Times.Once);
    }

    [Test]
    public void ValidateAndNormalizeString_TruncatesValueExceedingSize()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(allowEmpty: true, size: 3);
        Func<FormFieldInfo, string, string> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeString", field, "abcdef", passthrough);

        Assert.That(result, Is.EqualTo("abc"));
    }

    [Test]
    public void ValidateAndNormalizeString_ValidValue_InvokesNormalizer()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(allowEmpty: false);
        Func<FormFieldInfo, string, string> upper = (_, v) => v.ToUpperInvariant();

        var result = InvokeInstance(validator, "ValidateAndNormalizeString", field, "abc", upper);

        Assert.That(result, Is.EqualTo("ABC"));
        validationState.Verify(x => x.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region ValidateAndNormalizeBool / DateTime / Guid / Int

    [Test]
    public void ValidateAndNormalizeBool_NullWithoutAllowEmpty_AddsError()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(dataType: FieldDataType.Boolean, allowEmpty: false);
        Func<FormFieldInfo, bool, bool> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeBool", field, (bool?)null, passthrough);

        Assert.That(result, Is.Null);
        validationState.Verify(x => x.AddError(field.Name, FormErrorMessage.EmptyValue), Times.Once);
    }

    [Test]
    public void ValidateAndNormalizeDateTime_NullWithoutAllowEmpty_AddsError()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(dataType: FieldDataType.Date, allowEmpty: false);
        Func<FormFieldInfo, DateTime, DateTime> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeDateTime", field, (DateTime?)null, passthrough);

        Assert.That(result, Is.Null);
        validationState.Verify(x => x.AddError(field.Name, FormErrorMessage.EmptyValue), Times.Once);
    }

    [Test]
    public void ValidateAndNormalizeGuid_EmptyGuidWithoutAllowEmpty_AddsError()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(dataType: FieldDataType.Guid, allowEmpty: false);
        Func<FormFieldInfo, Guid, Guid> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeGuid", field, (Guid?)Guid.Empty, passthrough);

        Assert.That(result, Is.EqualTo(Guid.Empty));
        validationState.Verify(x => x.AddError(field.Name, FormErrorMessage.EmptyValue), Times.Once);
    }

    [Test]
    public void ValidateAndNormalizeInt_NullWithoutAllowEmpty_AddsError()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(dataType: FieldDataType.Integer, allowEmpty: false);
        Func<FormFieldInfo, int, int> passthrough = (_, v) => v;

        var result = InvokeInstance(validator, "ValidateAndNormalizeInt", field, (int?)null, passthrough);

        Assert.That(result, Is.Null);
        validationState.Verify(x => x.AddError(field.Name, FormErrorMessage.EmptyValue), Times.Once);
    }

    [Test]
    public void ValidateAndNormalizeInt_ValidValue_InvokesNormalizer()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Integer, allowEmpty: false);
        Func<FormFieldInfo, int, int> increment = (_, v) => v + 1;

        var result = InvokeInstance(validator, "ValidateAndNormalizeInt", field, (int?)5, increment);

        Assert.That(result, Is.EqualTo(6));
    }

    #endregion

    #region ValidateAndNormalizeUri

    [Test]
    public void ValidateAndNormalizeUri_ValidUri_ReturnsResult()
    {
        var validator = CreateValidator(out var validationState, out _);
        var field = CreateField(allowEmpty: false);

        var result = InvokeInstance(validator, "ValidateAndNormalizeUri", field, "https://example.com");

        Assert.That(result, Is.EqualTo("True"));
        validationState.Verify(x => x.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetValidFieldOptions / ValidateFieldOption(s)

    [Test]
    public void GetValidFieldOptions_NoOptionsSetting_ReturnsNull()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField();
        field.Settings[FormFieldSetting.Options] = null;

        var result = InvokeInstance(validator, "GetValidFieldOptions", field);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetValidFieldOptions_WithOptions_ReturnsResolvedKeys()
    {
        var validator = CreateValidator(out _, out var macroResolver);
        var field = CreateField();
        field.Settings[FormFieldSetting.Options] = "one;One\ntwo;Two";
        macroResolver
            .Setup(x => x.ResolveMacros(It.IsAny<string>(), It.IsAny<MacroSettings>()))
            .Returns<string, MacroSettings>((text, _) => text);

        var result = (HashSet<string>)InvokeInstance(validator, "GetValidFieldOptions", field)!;

        Assert.That(result, Is.EquivalentTo(s_oneTwoValues));
    }

    [Test]
    public void ValidateFieldOption_InvalidValue_AddsError()
    {
        var validator = CreateValidator(out var validationState, out var macroResolver);
        var field = CreateField();
        field.Settings[FormFieldSetting.Options] = "one\ntwo";
        macroResolver
            .Setup(x => x.ResolveMacros(It.IsAny<string>(), It.IsAny<MacroSettings>()))
            .Returns<string, MacroSettings>((text, _) => text);

        var method = typeof(PageContentValidator)
            .GetMethod("ValidateFieldOption", BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(typeof(string));

        var result = method.Invoke(validator, [field, "three"]);

        Assert.That(result, Is.EqualTo("three"));
        validationState.Verify(x => x.AddError(field.Name, It.Is<string>(m => m.Contains("one") && m.Contains("two"))), Times.Once);
    }

    [Test]
    public void ValidateFieldOptions_InvalidValues_AddsError()
    {
        var validator = CreateValidator(out var validationState, out var macroResolver);
        var field = CreateField();
        field.Settings[FormFieldSetting.Options] = "one\ntwo";
        macroResolver
            .Setup(x => x.ResolveMacros(It.IsAny<string>(), It.IsAny<MacroSettings>()))
            .Returns<string, MacroSettings>((text, _) => text);

        var invalid = new HashSet<string> { "one", "bad" };
        var result = (HashSet<string>)InvokeInstance(validator, "ValidateFieldOptions", field, invalid)!;

        Assert.That(result, Is.SameAs(invalid));
        validationState.Verify(x => x.AddError(field.Name, It.Is<string>(m => m.Contains("bad"))), Times.Once);
    }

    #endregion

    #region ValidateWebPagePathString / ValidateWebPageRelatedItem / ValidateContentItemReferenceFieldDataType

    [Test]
    public void ValidateWebPagePathString_EmptyControlName_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField();

        Assert.Throws<ArgumentException>(() =>
            InvokeInstance(validator, "ValidateWebPagePathString", field, "", "some/path"));
    }

    [Test]
    public void ValidateContentItemReferenceFieldDataType_EmptyControlName_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField();

        Assert.Throws<ArgumentException>(() =>
            InvokeInstance(validator, "ValidateContentItemReferenceFieldDataType", field, " ", (object?)"value"));
    }

    [Test]
    public void ValidateContentItemReferenceFieldDataType_ValidControlName_PassesValueThrough()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(allowEmpty: true);

        var result = InvokeInstance(validator, "ValidateContentItemReferenceFieldDataType", field, "kentico.contentitemselector", (object?)"item-guid");

        Assert.That(result, Is.EqualTo("item-guid"));
    }

    #endregion

    #region ValidateBooleanFieldDataType / ValidateDateFieldDataType

    [Test]
    public void ValidateBooleanFieldDataType_UnsupportedControl_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Boolean);

        Assert.Throws<NotImplementedException>(() =>
            InvokeInstance(validator, "ValidateBooleanFieldDataType", field, "unsupported.control", (object?)true));
    }

    [Test]
    public void ValidateBooleanFieldDataType_Checkbox_ReturnsValue()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Boolean, allowEmpty: true);

        var result = InvokeInstance(validator, "ValidateBooleanFieldDataType", field, "kentico.checkbox", (object?)true);

        Assert.That(result, Is.True);
    }

    [Test]
    public void ValidateDateFieldDataType_UnsupportedControl_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Date);

        Assert.Throws<NotImplementedException>(() =>
            InvokeInstance(validator, "ValidateDateFieldDataType", field, "unsupported.control", (object?)DateTime.Now));
    }

    #endregion

    #region ValidateIntegerFieldDataType / ValidateLongTextFieldDataType

    [Test]
    public void ValidateIntegerFieldDataType_UnsupportedControl_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Integer);

        Assert.Throws<NotImplementedException>(() =>
            InvokeInstance(validator, "ValidateIntegerFieldDataType", field, "unsupported.control", (object?)1));
    }

    [Test]
    public void ValidateIntegerFieldDataType_ObjectSelector_ReturnsValue()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.Integer, allowEmpty: true);

        var result = InvokeInstance(validator, "ValidateIntegerFieldDataType", field, "kentico.objectselector", (object?)7);

        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void ValidateLongTextFieldDataType_UnsupportedControl_Throws()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.LongText);

        Assert.Throws<NotImplementedException>(() =>
            InvokeInstance(validator, "ValidateLongTextFieldDataType", field, "unsupported.control", (object?)"text"));
    }

    [Test]
    public void ValidateLongTextFieldDataType_RichText_ReturnsValue()
    {
        var validator = CreateValidator(out _, out _);
        var field = CreateField(dataType: FieldDataType.LongText, allowEmpty: true);

        var result = InvokeInstance(validator, "ValidateLongTextFieldDataType", field, "kentico.richtext", (object?)"<p>Hi</p>");

        Assert.That(result, Is.EqualTo("<p>Hi</p>"));
    }

    #endregion
}