using BDO.GWT.Core;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class ExternalNavigationWidgetTests
    {
        private ExternalNavigationWidget _widget;

        [SetUp]
        public void SetUp()
        {
            _widget = new ExternalNavigationWidget();
        }

        [Test]
        public void Invoke_ShouldMapPropertiesCorrectly_AndReturnView()
        {
            // Arrange
            var properties = new ExternalNavigationWidgetProperties
            {
                LinkText = "Go to Google",
                ExternalLink = "https://www.google.com",
                PrimaryButtonLinkTarget = "_blank"
            };

            var model = new ComponentViewModel<ExternalNavigationWidgetProperties>();
            typeof(ComponentViewModel<ExternalNavigationWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);

            // Assert
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            Assert.That(viewResult?.ViewName,
                        Is.EqualTo("~/Components/Widget/ExternalNavigationWidget/_ExternalNavigationWidget.cshtml"));

            var dto = viewResult?.ViewData?.Model as ExternalNavigationData;
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Link.Title, Is.EqualTo("Go to Google"));
            Assert.That(dto.Link.URL, Is.EqualTo("https://www.google.com"));
            Assert.That(dto.openInNewTab, Is.EqualTo("_blank"));
        }
        [Test]
        public void Invoke_WithDefaultPrimaryButtonLinkTarget_ShouldUseSelf()
        {
            // Arrange
            var properties = new ExternalNavigationWidgetProperties
            {
                LinkText = "Default Target",
                ExternalLink = "https://example.com"
                // PrimaryButtonLinkTarget not set, uses default "_self"
            };

            var model = new ComponentViewModel<ExternalNavigationWidgetProperties>();
            typeof(ComponentViewModel<ExternalNavigationWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var dto = (result as ViewViewComponentResult)?.ViewData?.Model as ExternalNavigationData;

            // Assert
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.openInNewTab, Is.EqualTo("_self"));
        }
        [Test]
        public void Invoke_WithEmptyProperties_ShouldHandleGracefully()
        {
            // Arrange
            var properties = new ExternalNavigationWidgetProperties
            {
                LinkText = string.Empty,
                ExternalLink = string.Empty
            };

            var model = new ComponentViewModel<ExternalNavigationWidgetProperties>();
            typeof(ComponentViewModel<ExternalNavigationWidgetProperties>)
                .GetProperty("Properties")?.SetValue(model, properties);

            // Act
            var result = _widget.Invoke(model);
            var dto = (result as ViewViewComponentResult)?.ViewData?.Model as ExternalNavigationData;

            // Assert
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Link.Title, Is.EqualTo(string.Empty));
            Assert.That(dto.Link.URL, Is.EqualTo(string.Empty));
            Assert.That(dto.openInNewTab, Is.EqualTo("_self")); // default
        }
    }
}