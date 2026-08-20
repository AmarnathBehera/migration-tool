using BDO;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

[TestFixture]
public class BdoIntegrationsEditSectionTests
{
    [Test]
    public void InheritsFromEditSectionPage()
    {
        var baseType = typeof(BdoIntegrationsEditSection).BaseType;
        Assert.That(baseType, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
        });
    }

    [Test]
    public void HasExpectedUiPageRegistration()
    {
        var attr = typeof(BdoIntegrationsEditSection).Assembly
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "UIPageAttribute")
            .FirstOrDefault(a =>
                a.ConstructorArguments.Count >= 3 &&
                Equals(a.ConstructorArguments[2].Value, typeof(BdoIntegrationsEditSection)));

        Assert.That(attr, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(attr!.ConstructorArguments[0].Value, Is.EqualTo(typeof(BdoIntegrationListingPage)));
            Assert.That(attr.ConstructorArguments[1].Value?.ToString(), Is.EqualTo(PageParameterConstants.PARAMETERIZED_SLUG));
            Assert.That(attr.ConstructorArguments[3].Value?.ToString(), Is.EqualTo("Edit"));
            Assert.That(attr.ConstructorArguments[4].Value?.ToString(), Is.EqualTo(TemplateNames.SECTION_LAYOUT));
            Assert.That((int)attr.ConstructorArguments[5].Value!, Is.EqualTo(10));
        });
    }
}
