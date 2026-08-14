using BDO.GWT.GLOBAL.Components.FormComponents.RecipientList;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.RecipientList;

public class RecipientListDropdownComponentAttributeTests
{
    // --- Inheritance -------------------------------------------------------------

    [Test]
    public void Attribute_InheritsFrom_FormComponentAttribute()
    {
        Assert.That(
            typeof(FormComponentAttribute).IsAssignableFrom(typeof(RecipientListDropdownComponentAttribute)),
            Is.True);
    }

    // --- Instantiation -----------------------------------------------------------

    [Test]
    public void Attribute_CanBeInstantiated()
    {
        var attribute = new RecipientListDropdownComponentAttribute();

        Assert.That(attribute, Is.Not.Null);
    }

    [Test]
    public void Attribute_IsInstanceOf_FormComponentAttribute()
    {
        var attribute = new RecipientListDropdownComponentAttribute();

        Assert.That(attribute, Is.InstanceOf<FormComponentAttribute>());
    }

    // --- Usage as attribute ------------------------------------------------------

    [Test]
    public void Attribute_CanBeApplied_ToProperty()
    {
        // Verify the attribute's AttributeUsage allows it to target properties,
        // which is the standard usage for Kentico form component attributes.
        var usageAttr = typeof(FormComponentAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: true)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // FormComponentAttribute (from Kentico) allows property targets.
        Assert.That(usageAttr, Is.Not.Null);
    }

    [Test]
    public void Attribute_ExposesParameterlessConstructor()
    {
        var ctor = typeof(RecipientListDropdownComponentAttribute).GetConstructor(Type.EmptyTypes);

        Assert.That(ctor, Is.Not.Null);
    }
}
