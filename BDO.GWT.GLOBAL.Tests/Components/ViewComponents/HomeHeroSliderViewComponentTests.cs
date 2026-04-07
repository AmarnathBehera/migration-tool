using BDO.GWT.Core.DTOs;
using BDO.GWT.Global.Components.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class HomeHeroSliderViewComponentTests
    {
        private HomeHeroSliderViewComponent _component;

        [SetUp]
        public void SetUp()
        {
            _component = new HomeHeroSliderViewComponent();
        }

        [Test]
        public async Task InvokeAsync_ShouldReturnViewWithCorrectModel()
        {
            // Act
            var result = await _component.InvokeAsync();

            // Assert result type
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());

            var viewResult = result as ViewViewComponentResult;
            Assert.That(viewResult, Is.Not.Null);

            // Assert model type
            var model = viewResult?.ViewData?.Model as HomeHeroSlideDto;
            Assert.That(model, Is.Not.Null);

            // Assert properties
            Assert.That(model.Title, Is.EqualTo("Welcome to Our Platform"));
            Assert.That(model.Description, Is.EqualTo("This is a sample description used for testing metadata rendering."));

            Assert.That(model.Button, Is.Not.Null);
            Assert.That(model.Button.Title, Is.EqualTo("Learn More"));
            Assert.That(model.Button.URL, Is.EqualTo("/learn-more"));
            Assert.That(model.Button.Target, Is.EqualTo("_self"));

            Assert.That(model.ImageURL, Is.EqualTo(string.Empty));
            Assert.That(model.ImageAltText, Is.EqualTo("Alt Test"));

            Assert.That(model.MobileImageAltText, Is.EqualTo("Alt Test"));

            Assert.That(model.ThumbnailImageURL, Is.EqualTo("~//wwwroot//Hippo.jpeg"));
            Assert.That(model.ThumbnailImageAltText, Is.EqualTo("Alt Test"));
        }
    }
}