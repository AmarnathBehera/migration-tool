using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class SectionAboutPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private SectionAboutPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _service = new SectionAboutPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object);
        }

        [Test]
        public async Task GetAboutDtoAsync_ReturnsEmptyAboutDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AboutPage)null!);

            var result = await _service.GetAboutDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetAboutDtoAsync_ReturnsAboutDtoWithHero_WhenPageExists()
        {
            var page = new AboutPage
            {
                HeroLayout = "FullWidth",
                HeroTitle = "About Us",
                HeroDescription = "Description",
                HeroBackgroundImageAltText = "Alt text",
                HeroBackgroundImage_MobileAltText = "Mobile alt",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Learn More"
            };

            var expectedHeroDto = new HeroDto
            {
                Layout = "FullWidth",
                Title = "About Us",
                Description = "Description"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(
                    It.IsAny<HeroData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHeroDto);

            var result = await _service.GetAboutDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.EqualTo(expectedHeroDto));
        }

        [Test]
        public async Task GetAboutDtoAsync_MapsHeroDataCorrectly_WhenNoImages()
        {
            var page = new AboutPage
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
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
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

            await _service.GetAboutDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetAboutDtoAsync_PassesCancellationToken_ToContentService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    token))
                .ReturnsAsync((AboutPage)null!)
                .Verifiable();

            await _service.GetAboutDtoAsync(token);

            _contentServiceMock.Verify();
        }

        [Test]
        public async Task GetAboutDtoAsync_PassesCancellationToken_ToHeroService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = new AboutPage
            {
                HeroLayout = "Default",
                HeroTitle = "Test"
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), token))
                .ReturnsAsync(new HeroDto())
                .Verifiable();

            await _service.GetAboutDtoAsync(token);

            _heroServiceMock.Verify();
        }

        [Test]
        public async Task GetAboutDtoAsync_SetsLinkedItemsMaxLevel_To4()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<AboutPage>(
                    It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AboutPage)null!)
                .Verifiable();

            await _service.GetAboutDtoAsync(CancellationToken.None);

            _contentServiceMock.Verify();
        }
    }
}