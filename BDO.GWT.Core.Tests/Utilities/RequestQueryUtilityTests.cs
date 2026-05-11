using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using System.Collections.Generic;

namespace BDO.GWT.Core.Tests.Utilities;

public class RequestQueryUtilityTests
{
    [Test]
    public void ExtractFilterQueryParameters_ReturnsKnownFilters()
    {
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            [FilterParameterName.OfficeLocation] = "New York",
            [FilterParameterName.City] = "Chicago"
        });

        var result = RequestQueryUtility.ExtractFilterQueryParameters(queryCollection);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[FilterParameterName.OfficeLocation], Is.EqualTo("New York"));
            Assert.That(result[FilterParameterName.City], Is.EqualTo("Chicago"));
        });
    }

    [Test]
    public void ExtractFilterQueryParameters_ExcludesUnknownParameters()
    {
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            [FilterParameterName.OfficeLocation] = "Boston",
            ["unknownParam"] = "shouldBeExcluded"
        });

        var result = RequestQueryUtility.ExtractFilterQueryParameters(queryCollection);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.ContainsKey("unknownParam"), Is.False);
        });
    }

    [Test]
    public void ExtractFilterQueryParameters_ReturnsEmpty_WhenNoKnownFilters()
    {
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["page"] = "1",
            ["sort"] = "asc"
        });

        var result = RequestQueryUtility.ExtractFilterQueryParameters(queryCollection);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ExtractFilterQueryParameters_ReturnsEmpty_WhenQueryIsEmpty()
    {
        var queryCollection = new QueryCollection();

        var result = RequestQueryUtility.ExtractFilterQueryParameters(queryCollection);

        Assert.That(result, Is.Empty);
    }
}