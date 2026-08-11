using BDO.GWT.GLOBAL.Components.FormComponents.RecipientList;
using CMS.ContactManagement;
using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.RecipientList;

public class RecipientListDropdownComponentTests
{
    private Mock<IInfoProvider<ContactGroupInfo>> _recipientListInfoProviderMock = null!;
    private RecipientListDropdownComponent _component = null!;

    [SetUp]
    public void Setup()
    {
        _recipientListInfoProviderMock = new Mock<IInfoProvider<ContactGroupInfo>>();
        _component = new RecipientListDropdownComponent(_recipientListInfoProviderMock.Object);
    }

    // --- IDENTIFIER --------------------------------------------------------------

    [Test]
    public void Identifier_HasExpectedValue()
    {
        Assert.That(RecipientListDropdownComponent.IDENTIFIER, Is.EqualTo("BDO.GWT.RecipientListDropdown"));
    }

    // --- Inheritance -------------------------------------------------------------

    [Test]
    public void Component_InheritsFrom_FormComponent()
    {
        Assert.That(
            typeof(FormComponent<RecipientListDropdownProperties, string>)
                .IsAssignableFrom(typeof(RecipientListDropdownComponent)),
            Is.True);
    }

    // --- Value property ----------------------------------------------------------

    [Test]
    public void Value_DefaultsToEmptyString()
    {
        Assert.That(_component.Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Value_CanBeSetAndRetrieved()
    {
        _component.Value = "newsletter-list";

        Assert.That(_component.Value, Is.EqualTo("newsletter-list"));
    }

    // --- GetValue ----------------------------------------------------------------

    [Test]
    public void GetValue_ReturnsValue_WhenValueIsSet()
    {
        _component.Value = "my-list";

        Assert.That(_component.GetValue(), Is.EqualTo("my-list"));
    }

    [Test]
    public void GetValue_ReturnsEmptyString_WhenValueIsEmpty()
    {
        _component.Value = string.Empty;

        Assert.That(_component.GetValue(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetValue_ReturnsEmptyString_WhenValueIsNull()
    {
        _component.Value = null!;

        Assert.That(_component.GetValue(), Is.EqualTo(string.Empty));
    }

    // --- SetValue ----------------------------------------------------------------

    [Test]
    public void SetValue_SetsValue_WhenNonEmptyStringProvided()
    {
        _component.SetValue("my-newsletter");

        Assert.That(_component.Value, Is.EqualTo("my-newsletter"));
    }

    [Test]
    public void SetValue_UsesDefaultValue_WhenNullProvided()
    {
        _component.Properties.DefaultValue = "default-list";

        _component.SetValue(null!);

        Assert.That(_component.Value, Is.EqualTo("default-list"));
    }

    [Test]
    public void SetValue_UsesDefaultValue_WhenEmptyStringProvided()
    {
        _component.Properties.DefaultValue = "default-list";

        _component.SetValue(string.Empty);

        Assert.That(_component.Value, Is.EqualTo("default-list"));
    }

    [Test]
    public void SetValue_UsesDefaultValue_WhenWhiteSpaceProvided()
    {
        _component.Properties.DefaultValue = "default-list";

        _component.SetValue("   ");

        Assert.That(_component.Value, Is.EqualTo("default-list"));
    }

    [Test]
    public void SetValue_FallsBackToEmptyString_WhenValueEmptyAndNoDefaultValue()
    {
        _component.Properties.DefaultValue = string.Empty;

        _component.SetValue(string.Empty);

        Assert.That(_component.Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public void SetValue_DoesNotModifyOtherProperties()
    {
        _component.Properties.DefaultValue = "default-list";
        _component.SetValue("new-value");

        Assert.That(_component.Properties.DefaultValue, Is.EqualTo("default-list"));
    }

    // --- Options — provider interaction ------------------------------------------

    [Test]
    public void Options_CallsGet_OnInfoProvider()
    {
        // ObjectQuery<T> cannot be seeded with test data without a real Kentico DB.
        // We verify the provider's Get() is invoked and let the pipeline throw naturally.
        _recipientListInfoProviderMock
            .Setup(p => p.Get())
            .Returns(new ObjectQuery<ContactGroupInfo>());

        // Act — access Options; the ObjectQuery pipeline will throw internally
        // because there is no DB, but we only care that Get() was called.
        try { _ = _component.Options.ToList(); } catch { }

        _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void Options_ThrowsOrReturns_WhenProviderGetThrows()
    {
        // If the provider itself throws (e.g. infrastructure failure),
        // the exception must propagate through Options rather than being swallowed.
        _recipientListInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("provider failure"));

        Assert.Throws<InvalidOperationException>(() => _ = _component.Options.ToList());
    }

    [Test]
    public void Options_Property_ReturnsIEnumerableOfSelectListItem()
    {
        var optionsType = typeof(RecipientListDropdownComponent)
            .GetProperty(nameof(RecipientListDropdownComponent.Options))!
            .PropertyType;

        Assert.That(
            typeof(IEnumerable<SelectListItem>).IsAssignableFrom(optionsType),
            Is.True);
    }

    // --- BindableProperty attribute on Value ------------------------------------

    [Test]
    public void Value_Property_HasBindablePropertyAttribute()
    {
        var prop = typeof(RecipientListDropdownComponent)
            .GetProperty(nameof(RecipientListDropdownComponent.Value));
        var attr = prop?.GetCustomAttributes(typeof(BindablePropertyAttribute), inherit: false);

        Assert.That(attr, Is.Not.Null.And.Not.Empty);
    }

    // --- RegisterFormComponent assembly attribute --------------------------------

    [Test]
    public void Assembly_HasRegisterFormComponentAttribute_ForRecipientListDropdown()
    {
        var asm = typeof(RecipientListDropdownComponent).Assembly;

        var found = asm.GetCustomAttributesData()
            .Any(a =>
                a.AttributeType.Name == "RegisterFormComponentAttribute" &&
                a.ConstructorArguments.Any(arg =>
                    arg.Value?.ToString() == RecipientListDropdownComponent.IDENTIFIER));

        Assert.That(found, Is.True,
            "Expected [assembly: RegisterFormComponent(IDENTIFIER, ...)] to be present.");
    }
}
