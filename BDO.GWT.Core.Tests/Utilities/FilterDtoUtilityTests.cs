using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Utilities;

public class FilterDtoUtilityTests
{
    [Test]
    public void CreateFilterItemDto_ReturnsCorrectlyPopulatedDto()
    {
        var options = new[]
        {
            new OptionDto { Label = "Option 1", Value = "opt1" },
            new OptionDto { Label = "Option 2", Value = "opt2" }
        };

        var result = FilterDtoUtility.CreateFilterItemDto("dropdown", "location", "Location", "opt1", options);

        Assert.Multiple(() =>
        {
            Assert.That(result.Type, Is.EqualTo("dropdown"));
            Assert.That(result.Name, Is.EqualTo("location"));
            Assert.That(result.Title, Is.EqualTo("Location"));
            Assert.That(result.DefaultValue, Is.EqualTo("opt1"));
            Assert.That(result.Options, Has.Length.EqualTo(2));
        });
    }

    [Test]
    public void CreateFilterItemDto_SetsNullDefaultValue()
    {
        var options = new[] { new OptionDto { Label = "All", Value = "" } };

        var result = FilterDtoUtility.CreateFilterItemDto("select", "filter", "Filter", null, options);

        Assert.That(result.DefaultValue, Is.Null);
    }

    [Test]
    public void CreateFilterItemDto_WithEmptyOptions_ReturnsEmptyArray()
    {
        var result = FilterDtoUtility.CreateFilterItemDto<OptionDto>("checkbox", "test", "Test", null, []);

        Assert.That(result.Options, Is.Empty);
    }

    [Test]
    public void CreateOptionDto_ReturnsCorrectlyPopulatedDto()
    {
        var result = FilterDtoUtility.CreateOptionDto("New York", "ny");

        Assert.Multiple(() =>
        {
            Assert.That(result.Label, Is.EqualTo("New York"));
            Assert.That(result.Value, Is.EqualTo("ny"));
        });
    }

    [Test]
    public void CreateOptionDto_WithNullValues_SetsNulls()
    {
        var result = FilterDtoUtility.CreateOptionDto(null!, null!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Label, Is.Null);
            Assert.That(result.Value, Is.Null);
        });
    }
}