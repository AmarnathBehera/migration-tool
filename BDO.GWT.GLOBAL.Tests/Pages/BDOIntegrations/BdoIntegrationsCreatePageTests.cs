using BDO;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

[TestFixture]
public class BdoIntegrationsCreatePageTests
{
    [Test]
    public void InheritsFromCreatePage()
    {
        var baseType = typeof(BdoIntegrationsCreatePage).BaseType;
        Assert.That(baseType, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
            Assert.That(baseType.GetGenericArguments()[1], Is.EqualTo(typeof(BdoIntegrationsEditSection)));
        });
    }

    [Test]
    public async Task ConfigurePage_SetsExpectedUiFormName()
    {
        var page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationsCreatePage>();
        BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);

        try
        {
            await page.ConfigurePage();
        }
        catch
        {
        }

        var config = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
        Assert.That(BdoIntegrationsPageTestHelper.GetStringProperty(config, "UIFormName"), Is.EqualTo("FormSubmissionSubscriptions"));
    }

    [Test]
    public void HasExpectedConstructor()
    {
        var ctor = typeof(BdoIntegrationsCreatePage).GetConstructor(new[]
        {
            typeof(IFormComponentMapper),
            typeof(IFormDataBinder),
            typeof(IPageLinkGenerator),
        });

        Assert.That(ctor, Is.Not.Null);
    }

    [Test]
    public void HasExpectedUiPageRegistration()
    {
        var attr = typeof(BdoIntegrationsCreatePage).Assembly
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "UIPageAttribute")
            .FirstOrDefault(a =>
                a.ConstructorArguments.Count >= 3 &&
                Equals(a.ConstructorArguments[2].Value, typeof(BdoIntegrationsCreatePage)));

        Assert.That(attr, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(attr!.ConstructorArguments[0].Value, Is.EqualTo(typeof(BdoIntegrationListingPage)));
            Assert.That(attr.ConstructorArguments[1].Value?.ToString(), Is.EqualTo("create"));
            Assert.That(attr.ConstructorArguments[3].Value?.ToString(), Is.EqualTo("Create Form Submission Integration"));
            Assert.That(attr.ConstructorArguments[4].Value?.ToString(), Is.EqualTo(TemplateNames.EDIT));
            Assert.That((int)attr.ConstructorArguments[5].Value!, Is.EqualTo(20));
        });
    }
}
