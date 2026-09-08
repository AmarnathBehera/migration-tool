using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.FormEngine;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers;

[TestFixture]
public class SettingsCrudHelperTests
{
    private ValidationState _validationState = null!;
    private SettingsCrudHelper _helper = null!;

    [SetUp]
    public void Setup()
    {
        _validationState = new ValidationState();
        _helper = new SettingsCrudHelper(_validationState);
    }

    private static FormFieldInfo CreateField(string dataType)
    {
        var field = (FormFieldInfo)Activator.CreateInstance(typeof(FormFieldInfo), nonPublic: true)!;
        field.DataType = dataType;
        return field;
    }

    [Test]
    public void ValidateAndNormalize_NullField_AddsCodeNameError_AndThrows()
    {
        var input = new GlobalSettingInput { Value = "anything" };

        Assert.Throws<ValidationException>(() =>
            _helper.ValidateAndNormalizeSettingInput(null, "MySetting", input));

        Assert.That(_validationState.HasErrors(), Is.True);
        Assert.That(_validationState.GetErrors().ContainsKey("codeName"), Is.True);
    }

    [Test]
    public void ValidateAndNormalize_NullInputValue_DoesNothing()
    {
        var field = CreateField(FieldDataType.Text);
        var input = new GlobalSettingInput { Value = null };

        Assert.DoesNotThrow(() =>
            _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input));

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.Null);
    }

    [TestCase(FieldDataType.Boolean, "true", true)]
    [TestCase(FieldDataType.Boolean, "false", false)]
    [TestCase(FieldDataType.Integer, "42", 42)]
    [TestCase(FieldDataType.LongInteger, "9999999999", 9999999999L)]
    [TestCase(FieldDataType.Double, "3.14", 3.14)]
    [TestCase(FieldDataType.Text, "hello", "hello")]
    [TestCase(FieldDataType.LongText, "long", "long")]
    public void ValidateAndNormalize_ValidValues_NormalizesInput(string dataType, string rawValue, object expected)
    {
        var field = CreateField(dataType);
        var input = new GlobalSettingInput { Value = rawValue };

        _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input);

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.EqualTo(expected));
    }

    [Test]
    public void ValidateAndNormalize_DateTime_ValidValue_Normalizes()
    {
        var field = CreateField(FieldDataType.DateTime);
        var input = new GlobalSettingInput { Value = "2024-01-15T10:30:00" };

        _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input);

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.InstanceOf<DateTime>());
        Assert.That((DateTime)input.Value!, Is.EqualTo(new DateTime(2024, 1, 15, 10, 30, 0)));
    }

    [Test]
    public void ValidateAndNormalize_Guid_ValidValue_Normalizes()
    {
        var guid = Guid.NewGuid();
        var field = CreateField(FieldDataType.Guid);
        var input = new GlobalSettingInput { Value = guid.ToString() };

        _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input);

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.EqualTo(guid));
    }

    [Test]
    public void ValidateAndNormalize_UnknownDataType_PassesValueThrough()
    {
        var field = CreateField("SomethingCustom");
        var input = new GlobalSettingInput { Value = "raw" };

        _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input);

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.EqualTo("raw"));
    }

    [TestCase(FieldDataType.Boolean, "not-a-bool", "Please enter an boolean value (true/false).")]
    [TestCase(FieldDataType.Double, "not-a-num", "Please enter a number.")]
    [TestCase(FieldDataType.Integer, "not-a-num", "Please enter an integer.")]
    [TestCase(FieldDataType.LongInteger, "not-a-num", "Please enter an integer.")]
    [TestCase(FieldDataType.DateTime, "not-a-date", "Please enter a valid date/time.")]
    [TestCase(FieldDataType.Guid, "not-a-guid", "Please enter a valid GUID.")]
    public void ValidateAndNormalize_InvalidValues_AddsValueError_AndThrows(string dataType, string rawValue, string expectedMessage)
    {
        var field = CreateField(dataType);
        var input = new GlobalSettingInput { Value = rawValue };

        Assert.Throws<ValidationException>(() =>
            _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input));

        var errors = _validationState.GetErrors();
        Assert.That(errors.ContainsKey("value"), Is.True);
        Assert.That(errors["value"], Is.EqualTo(expectedMessage));
    }

    [Test]
    public void ValidateAndNormalize_NonStringConvertibleValue_UsesToString()
    {
        var field = CreateField(FieldDataType.Integer);
        var input = new GlobalSettingInput { Value = 123 };

        _helper.ValidateAndNormalizeSettingInput(field, "MySetting", input);

        Assert.That(_validationState.HasErrors(), Is.False);
        Assert.That(input.Value, Is.EqualTo(123));
    }

    [Test]
    public void SettingsCrudHelper_ImplementsInterface()
    {
        Assert.That(_helper, Is.InstanceOf<ISettingsCrudHelper>());
    }

    [Test]
    public void SettingsCrudHelper_UsesInjectedValidationState()
    {
        var mockValidationState = new Mock<IValidationState>();
        mockValidationState.Setup(v => v.HasErrors()).Returns(true);
        var helper = new SettingsCrudHelper(mockValidationState.Object);

        Assert.Throws<ValidationException>(() =>
            helper.ValidateAndNormalizeSettingInput(null, "code", new GlobalSettingInput { Value = "x" }));

        mockValidationState.Verify(v => v.AddError("codeName", It.IsAny<string>()), Times.Once);
    }
}
