using System.Reflection;
using System.Runtime.CompilerServices;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class CsvFileUploadComponentTests
{
    [Test]
    public void Component_IsInternalSealedClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(CsvFileUploadComponent).IsNotPublic, Is.True);
            Assert.That(typeof(CsvFileUploadComponent).IsSealed, Is.True);
        });
    }

    [Test]
    public void Component_InheritsExpectedFormComponentType()
    {
        Assert.That(typeof(CsvFileUploadComponent).BaseType?.Name, Is.EqualTo("FormComponent`3"));
    }

    [Test]
    public void Component_Identifier_HasExpectedValue()
    {
        Assert.That(CsvFileUploadComponent.IDENTIFIER, Is.EqualTo("XperienceCommunity.Redirects.CsvFileUpload"));
    }

    [Test]
    public void Component_ClientComponentName_ReturnsExpectedValue()
    {
        var component = (CsvFileUploadComponent)RuntimeHelpers.GetUninitializedObject(typeof(CsvFileUploadComponent));

        Assert.That(component.ClientComponentName, Is.EqualTo("@bdo/custom-components/CsvFileUpload"));
    }

    [Test]
    public async Task ConfigureClientProperties_WhenValueIsNull_SetsEmptyString()
    {
        var component = (CsvFileUploadComponent)RuntimeHelpers.GetUninitializedObject(typeof(CsvFileUploadComponent));
        var properties = new CsvFileUploadComponentClientProperties();

        await InvokeConfigureClientProperties(component, properties);

        Assert.That(properties.Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task ConfigureClientProperties_WhenValueAlreadySet_KeepsOriginalValue()
    {
        var component = (CsvFileUploadComponent)RuntimeHelpers.GetUninitializedObject(typeof(CsvFileUploadComponent));
        var properties = new CsvFileUploadComponentClientProperties { Value = "abc" };

        await InvokeConfigureClientProperties(component, properties);

        Assert.That(properties.Value, Is.EqualTo("abc"));
    }

    [Test]
    public void Assembly_HasRegisterFormComponentAttribute_ForCsvComponent()
    {
        var attrs = typeof(CsvFileUploadComponent).Assembly
            .GetCustomAttributes()
            .Where(a => a.GetType().Name == "RegisterFormComponentAttribute")
            .ToList();

        Assert.That(attrs, Is.Not.Empty);
    }

    [Test]
    public void CsvFileUploadComponentAttribute_InheritsFormComponentAttribute()
    {
        Assert.That(typeof(CsvFileUploadComponentAttribute).BaseType, Is.EqualTo(typeof(FormComponentAttribute)));
    }

    [Test]
    public void CsvFileUploadComponentAttribute_IsPublicSealed()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(CsvFileUploadComponentAttribute).IsPublic, Is.True);
            Assert.That(typeof(CsvFileUploadComponentAttribute).IsSealed, Is.True);
        });
    }

    [Test]
    public void CsvFileUploadComponentClientProperties_InheritsExpectedBaseType()
    {
        Assert.That(typeof(CsvFileUploadComponentClientProperties).BaseType, Is.EqualTo(typeof(FormComponentClientProperties<string>)));
    }

    [Test]
    public void CsvFileUploadComponentProperties_InheritsFormComponentProperties()
    {
        Assert.That(typeof(CsvFileUploadComponentProperties).BaseType, Is.EqualTo(typeof(FormComponentProperties)));
    }

    private static Task InvokeConfigureClientProperties(
        CsvFileUploadComponent component,
        CsvFileUploadComponentClientProperties properties)
    {
        var method = typeof(CsvFileUploadComponent)
            .GetMethod("ConfigureClientProperties", BindingFlags.Instance | BindingFlags.NonPublic)!;

        return (Task)method.Invoke(component, new object[] { properties })!;
    }
}
