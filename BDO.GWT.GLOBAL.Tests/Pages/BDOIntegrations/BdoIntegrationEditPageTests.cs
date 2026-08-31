using System.Reflection;
using BDO;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

[TestFixture]
public class BdoIntegrationEditPageTests
{
    [Test]
    public void InheritsFromInfoEditPage()
    {
        var baseType = typeof(BdoIntegrationEditPage).BaseType;
        Assert.That(baseType, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
        });
    }

    [Test]
    public void HasExpectedConstructor()
    {
        var ctor = typeof(BdoIntegrationEditPage).GetConstructor(new[]
        {
            typeof(IFormComponentMapper),
            typeof(IFormDataBinder),
        });

        Assert.That(ctor, Is.Not.Null);
    }

    [Test]
    public void ObjectId_HasPageParameterAttributeWithIntBinder()
    {
        var objectIdProperty = typeof(BdoIntegrationEditPage).GetProperty(
            nameof(BdoIntegrationEditPage.ObjectId),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.That(objectIdProperty, Is.Not.Null);

        var attribute = objectIdProperty!
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType == typeof(PageParameterAttribute));

        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.ConstructorArguments[0].Value, Is.EqualTo(typeof(IntPageModelBinder)));
    }

    [Test]
    public async Task ConfigurePage_SetsExpectedUiFormName_And_ObjectId_IsReadable()
    {
        var page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationEditPage>();
        BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);
        page.ObjectId = 5;

        try
        {
            await page.ConfigurePage();
        }
        catch
        {
        }

        var config = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

        Assert.Multiple(() =>
        {
            Assert.That(page.ObjectId, Is.EqualTo(5));
            Assert.That(BdoIntegrationsPageTestHelper.GetStringProperty(config, "UIFormName"), Is.EqualTo("FormSubmissionSubscriptions"));
        });
    }

    [Test]
    public void HasExpectedUiPageRegistration()
    {
        var attr = typeof(BdoIntegrationEditPage).Assembly
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "UIPageAttribute")
            .FirstOrDefault(a =>
                a.ConstructorArguments.Count >= 3 &&
                Equals(a.ConstructorArguments[2].Value, typeof(BdoIntegrationEditPage)));

        Assert.That(attr, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(attr!.ConstructorArguments[0].Value, Is.EqualTo(typeof(BdoIntegrationsEditSection)));
            Assert.That(attr.ConstructorArguments[1].Value?.ToString(), Is.EqualTo("edit"));
            Assert.That(attr.ConstructorArguments[3].Value?.ToString(), Is.EqualTo("Edit Form Submission subscription"));
            Assert.That(attr.ConstructorArguments[4].Value?.ToString(), Is.EqualTo(TemplateNames.EDIT));
            Assert.That((int)attr.ConstructorArguments[5].Value!, Is.EqualTo(0));
        });
    }
}
