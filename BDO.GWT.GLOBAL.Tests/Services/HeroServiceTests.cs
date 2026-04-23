using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using NUnit.Framework;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class HeroServiceTests
    {
        private HeroService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new HeroService();
        }

        [Test]
        public async Task GetHeroDtoAsync_MapsBasicFieldsCorrectly()
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Hero,
                Title = "Test Title",
                Description = "Test Description",
                WidgetID = "widget-1",
                BackgroundImage = new RelativeLink { Path = "media/bg.jpg" },
                BackgroundImageAltText = "BG Alt",
                MobileBackgroundImage = new RelativeLink { Path = "media/mobile.jpg" },
                MobileBackgroundImageAltText = "Mobile Alt",
                CallToActionButtonType = CallToActionButtonType.Primary,
                CallToActionButtonText = "Click",
                CallToActionButtonUrl = new RelativeLink { Path = "about" },
                CtaButtonUrlTarget = "_blank"
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.Layout, Is.EqualTo(HeroLayout.Hero));
                Assert.That(result.Title, Is.EqualTo("Test Title"));
                Assert.That(result.Description, Is.EqualTo("Test Description"));
                Assert.That(result.WidgetID, Is.EqualTo("widget-1"));
                Assert.That(result.BackgroundImage, Is.EqualTo("/media/bg.jpg"));
                Assert.That(result.BackgroundImageAltText, Is.EqualTo("BG Alt"));
                Assert.That(result.MobileBackgroundImage, Is.EqualTo("/media/mobile.jpg"));
                Assert.That(result.MobileBackgroundImageAltText, Is.EqualTo("Mobile Alt"));
                Assert.That(result.CtaButtonType, Is.EqualTo(CallToActionButtonType.Primary));
                Assert.That(result.CtaButtonText, Is.EqualTo("Click"));
                Assert.That(result.CtaButtonUrl, Is.EqualTo("/about"));
                Assert.That(result.CtaButtonUrlTarget, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public async Task GetHeroDtoAsync_WhenLayoutIsBanner_DoesNotSetImageFields()
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Banner,
                Title = "Banner Title",
                BackgroundImage = new RelativeLink { Path = "media/bg.jpg" },
                BackgroundImageAltText = "BG Alt",
                MobileBackgroundImage = new RelativeLink { Path = "media/mobile.jpg" },
                MobileBackgroundImageAltText = "Mobile Alt",
                CallToActionButtonType = CallToActionButtonType.None
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.Layout, Is.EqualTo(HeroLayout.Banner));
                Assert.That(result.BackgroundImage, Is.Null);
                Assert.That(result.BackgroundImageAltText, Is.Null);
                Assert.That(result.MobileBackgroundImage, Is.Null);
                Assert.That(result.MobileBackgroundImageAltText, Is.Null);
            });
        }

        [Test]
        public async Task GetHeroDtoAsync_WhenCtaButtonTypeIsNone_DoesNotSetCtaFields()
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Hero,
                CallToActionButtonType = CallToActionButtonType.None,
                CallToActionButtonText = "Click",
                CallToActionButtonUrl = new RelativeLink { Path = "about" },
                CtaButtonUrlTarget = "_blank"
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.CtaButtonText, Is.Null);
                Assert.That(result.CtaButtonUrl, Is.Null);
                Assert.That(result.CtaButtonUrlTarget, Is.Null);
            });
        }

        [Test]
        public async Task GetHeroDtoAsync_WhenCtaButtonTypeIsNull_DoesNotSetCtaFields()
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Hero,
                CallToActionButtonType = null,
                CallToActionButtonText = "Click",
                CallToActionButtonUrl = new RelativeLink { Path = "about" },
                CtaButtonUrlTarget = "_self"
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.CtaButtonText, Is.Null);
                Assert.That(result.CtaButtonUrl, Is.Null);
                Assert.That(result.CtaButtonUrlTarget, Is.Null);
            });
        }

        [Test]
        [TestCase(CallToActionButtonType.Primary)]
        [TestCase(CallToActionButtonType.Secondary)]
        [TestCase(CallToActionButtonType.Tertiary)]
        public async Task GetHeroDtoAsync_WhenCtaButtonTypeIsSet_MapsCtaFields(string buttonType)
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Hero,
                CallToActionButtonType = buttonType,
                CallToActionButtonText = "Go",
                CallToActionButtonUrl = new RelativeLink { Path = "page" },
                CtaButtonUrlTarget = "_self"
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.CtaButtonType, Is.EqualTo(buttonType));
                Assert.That(result.CtaButtonText, Is.EqualTo("Go"));
                Assert.That(result.CtaButtonUrl, Is.EqualTo("/page"));
                Assert.That(result.CtaButtonUrlTarget, Is.EqualTo("_self"));
            });
        }

        [Test]
        [TestCase(HeroLayout.Hero)]
        [TestCase(HeroLayout.BannerWithImage)]
        public async Task GetHeroDtoAsync_WhenLayoutIsNotBanner_SetsImageFields(string layout)
        {
            var data = new HeroData
            {
                Layout = layout,
                BackgroundImage = new RelativeLink { Path = "media/bg.jpg" },
                BackgroundImageAltText = "BG Alt",
                MobileBackgroundImage = new RelativeLink { Path = "media/mobile.jpg" },
                MobileBackgroundImageAltText = "Mobile Alt",
                CallToActionButtonType = CallToActionButtonType.None
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.BackgroundImage, Is.EqualTo("/media/bg.jpg"));
                Assert.That(result.BackgroundImageAltText, Is.EqualTo("BG Alt"));
                Assert.That(result.MobileBackgroundImage, Is.EqualTo("/media/mobile.jpg"));
                Assert.That(result.MobileBackgroundImageAltText, Is.EqualTo("Mobile Alt"));
            });
        }

        [Test]
        public async Task GetHeroDtoAsync_WhenNullImagesAndNoCtaOnBanner_ReturnsNullImageAndCtaFields()
        {
            var data = new HeroData
            {
                Layout = HeroLayout.Banner,
                BackgroundImage = null,
                MobileBackgroundImage = null,
                CallToActionButtonType = null
            };

            var result = await _service.GetHeroDtoAsync(data);

            Assert.Multiple(() =>
            {
                Assert.That(result.BackgroundImage, Is.Null);
                Assert.That(result.MobileBackgroundImage, Is.Null);
                Assert.That(result.CtaButtonText, Is.Null);
                Assert.That(result.CtaButtonUrl, Is.Null);
            });
        }
    }
}
