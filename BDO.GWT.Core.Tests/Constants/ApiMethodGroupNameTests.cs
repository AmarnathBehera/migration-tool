using System.Reflection;
using BDO.GWT.Domain.Core.Constants;

namespace BDO.GWT.Core.Tests.Constants;

[TestFixture]
public class ApiMethodGroupNameTests
{
    private static readonly (string FieldName, string ExpectedValue)[] ExpectedGroupNames =
    [
        (nameof(ApiMethodGroupName.Users), "Users"),
        (nameof(ApiMethodGroupName.Events), "Events"),
        (nameof(ApiMethodGroupName.News), "News"),
        (nameof(ApiMethodGroupName.People), "People"),
        (nameof(ApiMethodGroupName.Webinars), "Webinars"),
        (nameof(ApiMethodGroupName.BusinessLines), "Business lines"),
        (nameof(ApiMethodGroupName.Services), "Services"),
        (nameof(ApiMethodGroupName.ServiceSections), "Service sections"),
        (nameof(ApiMethodGroupName.ServiceAreas), "Service areas"),
        (nameof(ApiMethodGroupName.EventSections), "Event sections"),
        (nameof(ApiMethodGroupName.Content), "Content"),
        (nameof(ApiMethodGroupName.CareerJobOpportunities), "Career Job Opportunities"),
        (nameof(ApiMethodGroupName.Forms), "Forms"),
        (nameof(ApiMethodGroupName.Insights), "Insights"),
        (nameof(ApiMethodGroupName.PageAttachments), "Page Attachments"),
        (nameof(ApiMethodGroupName.LocationOffices), "Location Offices"),
        (nameof(ApiMethodGroupName.Deals), "Deals"),
        (nameof(ApiMethodGroupName.MediaFile), "Media File"),
        (nameof(ApiMethodGroupName.Channel), "Channel"),
        (nameof(ApiMethodGroupName.Careers), "Careers"),
        (nameof(ApiMethodGroupName.Footer), "Footer"),
        (nameof(ApiMethodGroupName.ChannelSettings), "ChannelSettings"),
        (nameof(ApiMethodGroupName.GlobalSettings), "GlobalSettings")
    ];

    [TestCaseSource(nameof(ExpectedGroupNames))]
    public void Constant_ShouldMatchExpectedValue((string FieldName, string ExpectedValue) item)
    {
        var field = typeof(ApiMethodGroupName).GetField(item.FieldName, BindingFlags.Public | BindingFlags.Static);

        Assert.That(field, Is.Not.Null, $"Field '{item.FieldName}' should exist.");

        var actual = field!.GetValue(null) as string;

        Assert.That(actual, Is.EqualTo(item.ExpectedValue));
    }

    [Test]
    public void AllConstantValues_ShouldBeUnique()
    {
        var values = typeof(ApiMethodGroupName)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => field.GetValue(null) as string)
            .Where(value => value is not null)
            .ToArray();

        var distinctValues = values.Distinct(StringComparer.Ordinal).ToArray();

        Assert.That(distinctValues, Has.Length.EqualTo(values.Length), "API method group names must be unique.");
    }
}
