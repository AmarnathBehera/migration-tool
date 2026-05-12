using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class IPageBuilderServiceTests
    {
        [Test]
        public void Interface_ExposesExpectedMembers()
        {
            var type = typeof(IPageBuilderService);

            Assert.Multiple(() =>
            {
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddAdvancedSections)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddAdvancedWidgets)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddIdentifier)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddSections)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddWidgets)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.AddDefaultWidgets)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.BuildEditableAreaAsync)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.BuildWidgetZoneAsync)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.GetPageBuilderScripts)), Is.Not.Null);
                Assert.That(type.GetMethod(nameof(IPageBuilderService.GetPageBuilderStyles)), Is.Not.Null);
            });
        }

        [Test]
        public void BuildEditableAreaAsync_ReturnsHtmlContentTask()
        {
            var method = typeof(IPageBuilderService).GetMethod(nameof(IPageBuilderService.BuildEditableAreaAsync))!;
            Assert.That(method.ReturnType, Is.EqualTo(typeof(Task<IHtmlContent>)));
        }

        [Test]
        public void GetPageBuilderScripts_ReturnsHtmlContent()
        {
            var method = typeof(IPageBuilderService).GetMethod(nameof(IPageBuilderService.GetPageBuilderScripts))!;
            Assert.That(method.ReturnType, Is.EqualTo(typeof(IHtmlContent)));
            Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IHtmlHelper)));
        }
    }
}
