using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Utilities;
using NUnit.Framework;
using System.Collections.Generic;

namespace BDO.GWT.Core.Tests.Utilities;

public class FilterUtilityTests
{
    [Test]
    public void GetFilterParameterData_ReturnsData_ForOfficeLocation()
    {
        var result = FilterUtility.GetFilterParameterData(FilterParameterName.OfficeLocation);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.EntityType, Is.EqualTo(typeof(LocationOffice)));
            Assert.That(result.FilterParameterType, Is.EqualTo(FilterParameterType.Page));
        });
    }

    [Test]
    public void GetFilterParameterData_ReturnsData_ForCity()
    {
        var result = FilterUtility.GetFilterParameterData(FilterParameterName.City);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EntityType, Is.EqualTo(typeof(LocationCity)));
    }

    [Test]
    public void GetFilterParameterData_ReturnsData_ForPerson()
    {
        var result = FilterUtility.GetFilterParameterData(FilterParameterName.Person);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EntityType, Is.EqualTo(typeof(Person)));
    }

    [Test]
    public void GetFilterParameterData_ReturnsData_ForProfileGroup()
    {
        var result = FilterUtility.GetFilterParameterData(FilterParameterName.ProfileGroup);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FilterParameterType, Is.EqualTo(FilterParameterType.Taxonomy));
    }

    [Test]
    public void GetFilterParameterData_ReturnsData_ForCredential()
    {
        var result = FilterUtility.GetFilterParameterData(FilterParameterName.Credential);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FilterParameterType, Is.EqualTo(FilterParameterType.Taxonomy));
    }

    [Test]
    public void GetFilterParameterData_ReturnsNull_ForUnknownParameter()
    {
        var result = FilterUtility.GetFilterParameterData("unknownFilter");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetFilterParameterValueByName_ReturnsOnlyKnownFilterParameters()
    {
        var input = new Dictionary<string, string>
        {
            [FilterParameterName.OfficeLocation] = "New York",
            [FilterParameterName.City] = "Chicago",
            ["unknownFilter"] = "should be excluded"
        };

        var result = FilterUtility.GetFilterParameterValueByName(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.ContainsKey(FilterParameterName.OfficeLocation), Is.True);
            Assert.That(result.ContainsKey(FilterParameterName.City), Is.True);
            Assert.That(result.ContainsKey("unknownFilter"), Is.False);
        });
    }

    [Test]
    public void GetFilterParameterValueByName_ReturnsEmpty_WhenNoKnownFilters()
    {
        var input = new Dictionary<string, string>
        {
            ["foo"] = "bar",
            ["baz"] = "qux"
        };

        var result = FilterUtility.GetFilterParameterValueByName(input);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFilterParameterValueByName_ReturnsEmpty_WhenInputIsEmpty()
    {
        var input = new Dictionary<string, string>();

        var result = FilterUtility.GetFilterParameterValueByName(input);

        Assert.That(result, Is.Empty);
    }
}