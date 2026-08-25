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
    public class CareerCategoryPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private CareerCategoryPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _service = new CareerCategoryPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object);
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_ReturnsEmptyCareerCategoryDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((CareerContent)null!);

            var result = await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_ReturnsCareerCategoryDtoWithHero_WhenPageExists()
        {
            var page = new CareerContent
            {
                HeroLayout = "FullWidth",
                HeroTitle = "Career Categories",
                HeroDescription = "Explore career paths",
                HeroBackgroundImageAltText = "Category banner",
                HeroBackgroundImage_MobileAltText = "Mobile category banner",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Browse Categories"
            };

            var expectedHeroDto = new HeroDto
            {
                Layout = "FullWidth",
                Title = "Career Categories",
                Description = "Explore career paths"
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

            var result = await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_MapsHeroDataCorrectly_WhenNoImages()
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

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_MapsHeroDataCorrectly_WithBackgroundImages()
        {
            var bgImageAttachment = new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = "media/category-bg.jpg" }
            };

            var mobileImageAttachment = new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = "media/category-mobile.jpg" }
            };

            var page = new CareerContent
            {
                HeroLayout = "Hero",
                HeroTitle = "Categories",
                HeroDescription = "Find Your Path",
                HeroBackgroundImage = new[] { bgImageAttachment },
                HeroBackgroundImageAltText = "Background",
                HeroBackgroundImage_Mobile = new[] { mobileImageAttachment },
                HeroBackgroundImage_MobileAltText = "Mobile Background",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Explore"
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
                        h.BackgroundImage.Path == "media/category-bg.jpg" &&
                        h.MobileBackgroundImage != null &&
                        h.MobileBackgroundImage.Path == "media/category-mobile.jpg"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_MapsHeroDataCorrectly_WithCallToActionUrl()
        {
            var mockWebPage = new Mock<IWebPageFieldsSource>();
            mockWebPage.Setup(x => x.SystemFields).Returns(new WebPageFields
            {
                WebPageUrlPath = "/careers/categories"
            });

            var page = new CareerContent
            {
                HeroLayout = "Banner",
                HeroTitle = "View All",
                HeroDescription = "Explore all categories",
                HeroBackgroundImageAltText = "Categories",
                HeroBackgroundImage_MobileAltText = "Categories Mobile",
                HeroCallToActionButtonType = "Tertiary",
                HeroCallToActionButtonText = "View",
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
                        h.CallToActionButtonUrl.Path == "/careers/categories"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_PassesCancellationToken_ToContentService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync((CareerContent)null!)
                .Verifiable();

            await _service.GetCareerCategoryDtoAsync(token);

            _contentServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_PassesCancellationToken_ToHeroService()
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

            await _service.GetCareerCategoryDtoAsync(token);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_SetsLinkedItemsMaxLevel_ToThree()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<CareerContent>(
                    It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 3),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((CareerContent)null!)
                .Verifiable();

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _contentServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_HandlesEmptyBackgroundImageCollection()
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

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetCareerCategoryDtoAsync_HandlesEmptyCallToActionUrl()
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

            await _service.GetCareerCategoryDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }
    }
}
