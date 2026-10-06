using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Tests.Data;

[TestFixture]
public class ChannelcodeNameDropdownTests
{
    [Test]
    public void Value_DefaultsToEmptyString()
    {
        var dropdown = new ChannelcodeNameDropdown();

        Assert.That(dropdown.Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Value_CanBeAssigned()
    {
        var dropdown = new ChannelcodeNameDropdown { Value = "Ind" };

        Assert.That(dropdown.Value, Is.EqualTo("Ind"));
    }

    [Test]
    public void TypeConverterAttribute_PointsToDropdownConverter()
    {
        var converter = TypeDescriptor.GetConverter(typeof(ChannelcodeNameDropdown));

        Assert.That(converter, Is.TypeOf<ChannelCodeNameDropdownConverter>());
    }

    [Test]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new ChannelCodeNameDropdownConverter();

        Assert.That(converter.CanConvertFrom(null, typeof(string)), Is.True);
    }

    [Test]
    public void CanConvertFrom_NonString_ReturnsFalse()
    {
        var converter = new ChannelCodeNameDropdownConverter();

        Assert.That(converter.CanConvertFrom(null, typeof(int)), Is.False);
    }

    [Test]
    public void ConvertFrom_String_ReturnsDropdownWithValue()
    {
        var converter = new ChannelCodeNameDropdownConverter();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Global");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<ChannelcodeNameDropdown>());
            Assert.That(((ChannelcodeNameDropdown)result!).Value, Is.EqualTo("Global"));
        });
    }

    [Test]
    public void ConvertFrom_EmptyString_ReturnsDropdownWithEmptyValue()
    {
        var converter = new ChannelCodeNameDropdownConverter();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, string.Empty);

        Assert.That(((ChannelcodeNameDropdown)result!).Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ConvertFrom_NonString_Throws()
    {
        var converter = new ChannelCodeNameDropdownConverter();

        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertFrom(null, CultureInfo.InvariantCulture, 42));
    }
}
