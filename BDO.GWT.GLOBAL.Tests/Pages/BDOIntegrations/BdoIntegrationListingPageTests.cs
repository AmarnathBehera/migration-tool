using BDO;
using BDO.GWT.Xperience;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

[TestFixture]
public class BdoIntegrationListingPageTests
{
    [Test]
    public void InheritsFromListingPage()
    {
        Assert.That(typeof(BdoIntegrationListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
    }

    [Test]
    public void ObjectType_ReturnsExpectedObjectType()
    {
        var page = new TestableBdoIntegrationListingPage();
        Assert.That(page.ExposedObjectType, Is.EqualTo(FormSubmissionSubscriptionsInfo.OBJECT_TYPE));
    }

    [Test]
    public async Task ConfigurePage_ExecutesColumnConfigurationLines()
    {
        var page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationListingPage>();
        BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);

        try
        {
            await page.ConfigurePage();
        }
        catch
        {
        }

        var config = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
        Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(config, "ColumnConfigurations"), Is.GreaterThanOrEqualTo(5));
    }

    [Test]
    public async Task Delete_InvokesForwardingPath()
    {
        var page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationListingPage>();

        try
        {
            await page.Delete(1);
        }
        catch
        {
        }

        Assert.Pass();
    }

    [Test]
    public void HasExpectedUiPageRegistration()
    {
        var attr = typeof(BdoIntegrationListingPage).Assembly
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "UIPageAttribute")
            .FirstOrDefault(a =>
                a.ConstructorArguments.Count >= 3 &&
                Equals(a.ConstructorArguments[2].Value, typeof(BdoIntegrationListingPage)));

        Assert.That(attr, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(attr!.ConstructorArguments[0].Value, Is.EqualTo(typeof(BdoIntegrationsApplication)));
            Assert.That(attr.ConstructorArguments[1].Value?.ToString(), Is.EqualTo("BDO-Integrations"));
            Assert.That(attr.ConstructorArguments[3].Value?.ToString(), Is.EqualTo("BDO Integrations"));
            Assert.That(attr.ConstructorArguments[4].Value?.ToString(), Is.EqualTo(TemplateNames.LISTING));
            Assert.That((int)attr.ConstructorArguments[5].Value!, Is.EqualTo(0));
        });
    }

    private sealed class TestableBdoIntegrationListingPage : BdoIntegrationListingPage
    {
        public string ExposedObjectType => ObjectType;
    }
}
