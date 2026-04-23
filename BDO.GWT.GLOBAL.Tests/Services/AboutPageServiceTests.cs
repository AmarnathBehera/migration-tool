using BDO;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class AboutPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private AboutPageService<IHero> _service = null!;

        [SetUp]
        public void SetUp()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _service = new AboutPageService<IHero>(_heroServiceMock.Object);
        }

        private static Mock<IHero> BuildPage(
            string layout = HeroLayout.Hero,
            string? title = null,
            string? description = null,
            string? bgImageUrl = null,
            string? bgImageAltText = null,
            string? mobileImageUrl = null,
            string? mobileImageAltText = null,
            string? ctaButtonType = null,
            string? ctaButtonText = null,
            string? ctaUrlPath = null)
        {
            var page = new Mock<IHero>();
            page.SetupGet(p => p.HeroLayout).Returns(layout);
            page.SetupGet(p => p.HeroTitle).Returns(title!);
            page.SetupGet(p => p.HeroDescription).Returns(description!);
            page.SetupGet(p => p.HeroBackgroundImageAltText).Returns(bgImageAltText!);
            page.SetupGet(p => p.HeroBackgroundImage_MobileAltText).Returns(mobileImageAltText!);
            page.SetupGet(p => p.HeroCallToActionButtonType).Returns(ctaButtonType!);
            page.SetupGet(p => p.HeroCallToActionButtonText).Returns(ctaButtonText!);

            var bgAttachment = bgImageUrl != null
                ? new List<ContentAttachment> { new() { ContentAttachmentAsset = new ContentItemAsset { Url = bgImageUrl } } }
                : new List<ContentAttachment>();
            page.SetupGet(p => p.HeroBackgroundImage).Returns(bgAttachment);

            var mobileAttachment = mobileImageUrl != null
                ? new List<ContentAttachment> { new() { ContentAttachmentAsset = new ContentItemAsset { Url = mobileImageUrl } } }
                : new List<ContentAttachment>();
            page.SetupGet(p => p.HeroBackgroundImage_Mobile).Returns(mobileAttachment);

            if (ctaUrlPath != null)
            {
                var webPageMock = new Mock<IWebPageFieldsSource>();
                webPageMock.SetupGet(w => w.SystemFields).Returns(new WebPageFields { WebPageUrlPath = ctaUrlPath });
                page.SetupGet(p => p.HeroCallToActionButtonUrl).Returns(new List<IWebPageFieldsSource> { webPageMock.Object });
            }
            else
            {
                page.SetupGet(p => p.HeroCallToActionButtonUrl).Returns(new List<IWebPageFieldsSource>());
            }

            return page;
        }

        [Test]
        public async Task GetAboutDtoAsync_ReturnsAboutDto_WithHeroFromService()
        {
            var expectedHero = new HeroDto { Layout = HeroLayout.Hero, Title = "Title" };
            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHero);

            var page = BuildPage();
            var result = await _service.GetAboutDtoAsync(page.Object, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.SameAs(expectedHero));
        }

        [Test]
        public async Task GetAboutDtoAsync_MapsAllHeroDataFields_ToHeroService()
        {
            HeroData? capturedData = null;
            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => capturedData = d)
                .ReturnsAsync(new HeroDto { Layout = HeroLayout.Hero });

            var page = BuildPage(
                layout: HeroLayout.Hero,
                title: "About Us",
                description: "Our story",
                bgImageUrl: "/media/bg.jpg",
                bgImageAltText: "Background",
                mobileImageUrl: "/media/mobile.jpg",
                mobileImageAltText: "Mobile background",
                ctaButtonType: CallToActionButtonType.Primary,
                ctaButtonText: "Learn More",
                ctaUrlPath: "/about"
            );

            await _service.GetAboutDtoAsync(page.Object, CancellationToken.None);

            Assert.That(capturedData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedData!.Layout, Is.EqualTo(HeroLayout.Hero));
                Assert.That(capturedData.Title, Is.EqualTo("About Us"));
                Assert.That(capturedData.Description, Is.EqualTo("Our story"));
                Assert.That(capturedData.BackgroundImage!.ToHtmlCompatibleString(), Is.EqualTo("/media/bg.jpg"));
                Assert.That(capturedData.BackgroundImageAltText, Is.EqualTo("Background"));
                Assert.That(capturedData.MobileBackgroundImage!.ToHtmlCompatibleString(), Is.EqualTo("/media/mobile.jpg"));
                Assert.That(capturedData.MobileBackgroundImageAltText, Is.EqualTo("Mobile background"));
                Assert.That(capturedData.CallToActionButtonType, Is.EqualTo(CallToActionButtonType.Primary));
                Assert.That(capturedData.CallToActionButtonText, Is.EqualTo("Learn More"));
                Assert.That(capturedData.CallToActionButtonUrl!.ToHtmlCompatibleString(), Is.EqualTo("/about"));
            });
        }

        [Test]
        public async Task GetAboutDtoAsync_WhenBackgroundImageIsEmpty_SetsBackgroundImageToNull()
        {
            HeroData? capturedData = null;
            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => capturedData = d)
                .ReturnsAsync(new HeroDto { Layout = HeroLayout.Hero });

            var page = BuildPage(bgImageUrl: null, mobileImageUrl: null);

            await _service.GetAboutDtoAsync(page.Object, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedData!.BackgroundImage, Is.Null);
                Assert.That(capturedData.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetAboutDtoAsync_WhenCtaUrlIsEmpty_SetsCtaUrlToNull()
        {
            HeroData? capturedData = null;
            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((d, _) => capturedData = d)
                .ReturnsAsync(new HeroDto { Layout = HeroLayout.Hero });

            var page = BuildPage(ctaUrlPath: null);

            await _service.GetAboutDtoAsync(page.Object, CancellationToken.None);

            Assert.That(capturedData!.CallToActionButtonUrl, Is.Null);
        }

        [Test]
        public async Task GetAboutDtoAsync_PassesCancellationToken_ToHeroService()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            CancellationToken capturedToken = default;

            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((_, t) => capturedToken = t)
                .ReturnsAsync(new HeroDto { Layout = HeroLayout.Hero });

            var page = BuildPage();

            await _service.GetAboutDtoAsync(page.Object, token);

            Assert.That(capturedToken, Is.EqualTo(token));
        }

        [Test]
        public async Task GetAboutDtoAsync_CallsHeroServiceExactlyOnce()
        {
            _heroServiceMock
                .Setup(s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Layout = HeroLayout.Hero });

            var page = BuildPage();

            await _service.GetAboutDtoAsync(page.Object, CancellationToken.None);

            _heroServiceMock.Verify(
                s => s.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
