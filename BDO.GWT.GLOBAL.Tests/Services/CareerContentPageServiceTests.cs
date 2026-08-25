using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class CareerContentPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private CareerContentPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _service = new CareerContentPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object);
        }

        [Test]
        public async Task GetCareerContentDtoAsync_ReturnsEmptyCareerDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((CareerContent)null!);

            var result = await _service.GetCareerContentDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetCareerContentDtoAsync_ReturnsCareerDtoWithHero_WhenPageExists()
        {
            var page = new CareerContent
            {
                HeroLayout = "FullWidth",
                HeroTitle = "Career Opportunities",
                HeroDescription = "Join our team",
                HeroBackgroundImageAltText = "Career banner",
                HeroBackgroundImage_MobileAltText = "Mobile career banner",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Apply Now"
            };

            var expectedHeroDto = new HeroDto
            {
                Layout = "FullWidth",
                Title = "Career Opportunities",
                Description = "Join our team"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.IsAny<HeroData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHeroDto);

            var result = await _service.GetCareerContentDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
        }

        [Test]
        public async Task GetCareerContentDtoAsync_MapsHeroDataCorrectly_WhenNoImages()
        {
            var page = new CareerContent
            {
                HeroLayout = "Centered",
                HeroTitle = "Title",
                HeroDescription = "Desc",
                HeroBackgroundImageAltText = "BgAlt",
                HeroBackgroundImage_MobileAltText = "MobileAlt",
                HeroCallToActionButtonType = "Secondary",
                HeroCallToActionButtonText = "Click"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.Is<HeroData>(h =>
                        h.Layout == "Centered" &&
                        h.Title == "Title" &&
                        h.Description == "Desc" &&
                        h.BackgroundImage == null &&
                        h.BackgroundImageAltText == "BgAlt" &&
                        h.MobileBackgroundImage == null &&
                        h.MobileBackgroundImageAltText == "MobileAlt" &&
                        h.CallToActionButtonType == "Secondary" &&
                        h.CallToActionButtonText == "Click" &&
                        h.CallToActionButtonUrl == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_MapsHeroDataCorrectly_WithBackgroundImages()
        {
            var bgImageAttachment = new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = "media/career-bg.jpg" }
            };

            var mobileImageAttachment = new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = "media/career-mobile.jpg" }
            };

            var page = new CareerContent
            {
                HeroLayout = "Hero",
                HeroTitle = "Careers",
                HeroDescription = "Join Us",
                HeroBackgroundImage = new[] { bgImageAttachment },
                HeroBackgroundImageAltText = "Background",
                HeroBackgroundImage_Mobile = new[] { mobileImageAttachment },
                HeroBackgroundImage_MobileAltText = "Mobile Background",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Learn More"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.Is<HeroData>(h =>
                        h.BackgroundImage != null &&
                        h.BackgroundImage.Path == "media/career-bg.jpg" &&
                        h.MobileBackgroundImage != null &&
                        h.MobileBackgroundImage.Path == "media/career-mobile.jpg"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_MapsHeroDataCorrectly_WithCallToActionUrl()
        {
            var mockWebPage = new Mock<IWebPageFieldsSource>();
            mockWebPage.Setup(x => x.SystemFields).Returns(new WebPageFields
            {
                WebPageUrlPath = "/careers/apply"
            });

            var page = new CareerContent
            {
                HeroLayout = "Banner",
                HeroTitle = "Apply Today",
                HeroDescription = "Start your career",
                HeroBackgroundImageAltText = "Apply",
                HeroBackgroundImage_MobileAltText = "Apply Mobile",
                HeroCallToActionButtonType = "Tertiary",
                HeroCallToActionButtonText = "Apply",
                HeroCallToActionButtonUrl = new[] { mockWebPage.Object }
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.Is<HeroData>(h =>
                        h.CallToActionButtonUrl != null &&
                        h.CallToActionButtonUrl.Path == "careers/apply"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_PassesCancellationToken_ToContentService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync((CareerContent)null!)
                .Verifiable();

            await _service.GetCareerContentDtoAsync(token);

            _contentServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_PassesCancellationToken_ToHeroService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = new CareerContent
            {
                HeroLayout = "Hero",
                HeroTitle = "Test"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.IsAny<HeroData>(),
                    token))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(token);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_SetsLinkedItemsMaxLevel_ToFive()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 5),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((CareerContent)null!)
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _contentServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_HandlesEmptyBackgroundImageCollection()
        {
            var page = new CareerContent
            {
                HeroLayout = "Hero",
                HeroTitle = "Test",
                HeroBackgroundImage = Enumerable.Empty<ContentAttachment>(),
                HeroBackgroundImage_Mobile = Enumerable.Empty<ContentAttachment>()
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.Is<HeroData>(h =>
                        h.BackgroundImage == null &&
                        h.MobileBackgroundImage == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerContentDtoAsync_HandlesEmptyCallToActionUrl()
        {
            var page = new CareerContent
            {
                HeroLayout = "Banner",
                HeroTitle = "Test",
                HeroCallToActionButtonUrl = Enumerable.Empty<IWebPageFieldsSource>()
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.Is<HeroData>(h => h.CallToActionButtonUrl == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerContentDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }
    }
}
