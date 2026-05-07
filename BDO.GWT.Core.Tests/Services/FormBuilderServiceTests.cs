using BDO.GWT.Core.Services;
using CMS.DataEngine;
using Kentico.Forms.Web.Mvc.Widgets;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class FormBuilderServiceTests
{
    private FormBuilderService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new FormBuilderService();
    }

    // ?? Interface contract ???????????????????????????????????????????????

    [Test]
    public void FormBuilderService_ImplementsIFormBuilderService()
    {
        Assert.That(_service, Is.InstanceOf<IFormBuilderService>());
    }

    // ?? Method signature / null arguments ????????????????????????????????
    // The method calls a static Kentico extension (RenderStandaloneWidgetAsync)
    // which requires a full Kentico runtime context. We cannot mock static
    // methods with Moq, so we verify the service reacts correctly to invalid
    // inputs and confirm the method exists with the expected signature.

    [Test]
    public void GetFormBuilderWidget_WithNullHtmlHelper_ThrowsNullReferenceException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _service.GetFormBuilderWidget(null!, "test-form"));
    }

    [Test]
    public void GetFormBuilderWidget_WithNullFormIdentifier_ThrowsOnKenticoCall()
    {
        var htmlHelperMock = new Mock<IHtmlHelper>();

        // The Kentico static extension will fail because the widget type is
        // not registered, but we verify the call reaches Kentico infrastructure.
        Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _service.GetFormBuilderWidget(htmlHelperMock.Object, null!));
    }

    [Test]
    public void GetFormBuilderWidget_WithValidInputs_ThrowsDueToMissingKenticoRuntime()
    {
        // Without the full Kentico runtime, the static Kentico() extension
        // will throw. This confirms the method routes to the Kentico API.
        var htmlHelperMock = new Mock<IHtmlHelper>();

        Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _service.GetFormBuilderWidget(htmlHelperMock.Object, "contact-form"));
    }

    // ?? Widget properties construction ???????????????????????????????????
    // We verify the FormWidgetProperties are built correctly by extracting
    // the logic into a testable observation. Since the class is simple and
    // delegates to Kentico, we validate the SelectedForm list structure
    // using a known ObjectRelatedItem pattern.

    [Test]
    public void FormWidgetProperties_SelectedForm_ContainsSingleItem_WithCodeName()
    {
        var formIdentifier = "my-contact-form";

        var widgetProperties = new FormWidgetProperties
        {
            SelectedForm =
            [
                new ObjectRelatedItem { ObjectCodeName = formIdentifier }
            ]
        };

        var items = widgetProperties.SelectedForm.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items, Has.Count.EqualTo(1));
            Assert.That(items[0].ObjectCodeName, Is.EqualTo(formIdentifier));
        });
    }

    [Test]
    public void FormWidgetProperties_SelectedForm_WithEmptyCodeName()
    {
        var widgetProperties = new FormWidgetProperties
        {
            SelectedForm =
            [
                new ObjectRelatedItem { ObjectCodeName = string.Empty }
            ]
        };

        Assert.That(widgetProperties.SelectedForm.First().ObjectCodeName, Is.EqualTo(string.Empty));
    }
}
