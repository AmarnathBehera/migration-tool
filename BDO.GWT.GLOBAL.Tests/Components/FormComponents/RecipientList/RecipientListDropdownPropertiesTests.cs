using BDO.GWT.GLOBAL.Components.FormComponents.RecipientList;
using Kentico.Forms.Web.Mvc;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.RecipientList;

public class RecipientListDropdownPropertiesTests
{
    // --- Inheritance -------------------------------------------------------------

    [Test]
    public void Properties_InheritsFrom_FormComponentProperties()
    {
        Assert.That(
            typeof(FormComponentProperties<string>).IsAssignableFrom(typeof(RecipientListDropdownProperties)),
            Is.True);
    }

    // --- DefaultValue ------------------------------------------------------------

    [Test]
    public void DefaultValue_DefaultsToEmptyString()
    {
        var props = new RecipientListDropdownProperties();

        Assert.That(props.DefaultValue, Is.EqualTo(string.Empty));
    }

    [Test]
    public void DefaultValue_CanBeSetAndRetrieved()
    {
        var props = new RecipientListDropdownProperties
        {
            DefaultValue = "my-default-list"
        };

        Assert.That(props.DefaultValue, Is.EqualTo("my-default-list"));
    }

    [Test]
    public void DefaultValue_CanBeSetToNull()
    {
        var props = new RecipientListDropdownProperties
        {
            DefaultValue = null!
        };

        Assert.That(props.DefaultValue, Is.Null);
    }

    [Test]
    public void DefaultValue_CanBeOverwritten()
    {
        var props = new RecipientListDropdownProperties { DefaultValue = "first" };

        props.DefaultValue = "second";

        Assert.That(props.DefaultValue, Is.EqualTo("second"));
    }

    // --- Constructor -------------------------------------------------------------

    [Test]
    public void Constructor_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _ = new RecipientListDropdownProperties());
    }

    [Test]
    public void Constructor_ProducesNonNullInstance()
    {
        var props = new RecipientListDropdownProperties();

        Assert.That(props, Is.Not.Null);
    }
}
