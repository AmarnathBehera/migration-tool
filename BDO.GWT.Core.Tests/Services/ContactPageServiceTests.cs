using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Services
{
    public class ContactPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IWebsiteChannelContext> _channelMock = null!;
        private ContactPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _heroServiceMock = new Mock<IHeroService>();
            _channelMock = new Mock<IWebsiteChannelContext>();

            _channelMock.Setup(x => x.WebsiteChannelID).Returns(1);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description
                });

            _service = new ContactPageService(
                _contentServiceMock.Object,
                _heroServiceMock.Object);
        }

        private void SetupContent(Contact? page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Contact>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        private static Contact CreatePage(
            string? title = null,
            string? description = null,
            string? formType = null,
            string? form = null,
            string? embedCode = null)
        {
            return new Contact
            {
                DescriptionTitle = title!,
                DescriptionBody = description!,
                FormType = formType!,
                Form = form!,
                EmbedCode = embedCode!
            };
        }

        private static WebPageFields CreateSystemFields()
        {
            var sys = new WebPageFields();

            SetProperty(sys, nameof(WebPageFields.ContentItemID), 17);
            SetProperty(sys, nameof(WebPageFields.ContentItemGUID), Guid.Parse("11111111-1111-1111-1111-111111111111"));
            SetProperty(sys, nameof(WebPageFields.ContentItemName), "Contact");
            SetProperty(sys, nameof(WebPageFields.ContentItemContentTypeID), 22);
            SetProperty(sys, nameof(WebPageFields.ContentItemIsSecured), true);
            SetProperty(sys, nameof(WebPageFields.ContentItemCommonDataContentLanguageID), 5);
            SetProperty(sys, nameof(WebPageFields.ContentItemCommonDataFirstPublishedWhen), new DateTime(2026, 1, 1));
            SetProperty(sys, nameof(WebPageFields.ContentItemCommonDataLastPublishedWhen), new DateTime(2026, 2, 1));

            SetProperty(sys, nameof(WebPageFields.WebPageItemGUID), Guid.Parse("22222222-2222-2222-2222-222222222222"));
            SetProperty(sys, nameof(WebPageFields.WebPageItemID), 7);
            SetProperty(sys, nameof(WebPageFields.WebPageItemName), "Contact Us");
            SetProperty(sys, nameof(WebPageFields.WebPageItemWebsiteChannelId), 1);
            SetProperty(sys, nameof(WebPageFields.WebPageUrlPath), "/contact-us");
            SetProperty(sys, nameof(WebPageFields.WebPageItemOrder), 3);
            SetProperty(sys, nameof(WebPageFields.WebPageItemTreePath), "/Contact");
            SetProperty(sys, nameof(WebPageFields.WebPageUrlPathContentLanguageID), 5);
            SetProperty(sys, nameof(WebPageFields.WebPageItemParentID), 10);

            return sys;
        }

        private static void SetProperty<T>(object target, string propertyName, T value)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            property?.SetValue(target, value);
        }

        [Test]
        public async Task GetContactPageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            SetupContent(null);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.Null);
                Assert.That(result.Description, Is.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.BaseContent, Is.Null);
                Assert.That(result.BaseWebPage, Is.Null);
            });

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsScalarFields()
        {
            var page = CreatePage(
                title: "Contact Us",
                description: "We would love to hear from you.",
                formType: "Embedded",
                form: "ContactUsForm",
                embedCode: "<div id=\"hs-form\"></div>");

            SetupContent(page);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Contact Us"));
                Assert.That(result.Description, Is.EqualTo("We would love to hear from you."));
                Assert.That(result.FormType, Is.EqualTo("Embedded"));
                Assert.That(result.Form, Is.EqualTo("ContactUsForm"));
                Assert.That(result.EmbedCode, Is.EqualTo("<div id=\"hs-form\"></div>"));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsBaseContent_FromSystemFields()
        {
            var page = CreatePage();
            page.SystemFields = CreateSystemFields();

            SetupContent(page);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result.BaseContent, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(17));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
                Assert.That(result.BaseContent.ContentItemName, Is.EqualTo("Contact"));
                Assert.That(result.BaseContent.ContentItemContentTypeID, Is.EqualTo(22));
                Assert.That(result.BaseContent.ContentItemIsSecured, Is.True);
                Assert.That(result.BaseContent.ContentItemCommonDataContentLanguageID, Is.EqualTo(5));
                Assert.That(result.BaseContent.ContentItemCommonDataFirstPublishedWhen, Is.EqualTo(new DateTime(2026, 1, 1)));
                Assert.That(result.BaseContent.ContentItemCommonDataLastPublishedWhen, Is.EqualTo(new DateTime(2026, 2, 1)));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsBaseWebPage_FromSystemFields()
        {
            var page = CreatePage();
            page.SystemFields = CreateSystemFields();

            SetupContent(page);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result.BaseWebPage, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseWebPage!.WebPageItemGUID, Is.EqualTo(Guid.Parse("22222222-2222-2222-2222-222222222222")));
                Assert.That(result.BaseWebPage.WebPageItemID, Is.EqualTo(7));
                Assert.That(result.BaseWebPage.WebPageItemName, Is.EqualTo("Contact Us"));
                Assert.That(result.BaseWebPage.WebPageItemWebsiteChannelID, Is.EqualTo(1));
                Assert.That(result.BaseWebPage.WebPageUrlPath, Is.EqualTo("/contact-us"));
                Assert.That(result.BaseWebPage.WebPageItemOrder, Is.EqualTo(3));
                Assert.That(result.BaseWebPage.WebPageItemTreePath, Is.EqualTo("/Contact"));
                Assert.That(result.BaseWebPage.WebPageUrlPathContentLanguageID, Is.EqualTo(5));
                Assert.That(result.BaseWebPage.WebPageItemParentID, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_MapsDefaultBaseValues_WhenSystemFieldsIsNull()
        {
            var page = CreatePage();

            SetupContent(page);

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent, Is.Not.Null);
                Assert.That(result.BaseWebPage, Is.Not.Null);
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.BaseContent!.ContentItemID, Is.EqualTo(0));
                Assert.That(result.BaseContent.ContentItemGUID, Is.EqualTo(Guid.Empty));
                Assert.That(result.BaseWebPage!.WebPageItemID, Is.EqualTo(0));
                Assert.That(result.BaseWebPage.WebPageItemGUID, Is.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_PassesHeroData_ToHeroService()
        {
            var ctaPage = new Contact
            {
                SystemFields = CreateSystemFields()
            };

            var page = new Contact
            {
                HeroLayout = "Default",
                HeroTitle = "Hero",
                HeroDescription = "Desc",
                HeroCallToActionButtonType = "Primary",
                HeroCallToActionButtonText = "Go",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "MAlt",
                HeroCallToActionButtonUrl = new List<IWebPageFieldsSource> { ctaPage }
            };

            SetupContent(page);

            HeroData? captured = null;

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => captured = data)
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(captured!.Layout, Is.EqualTo("Default"));
                Assert.That(captured.Title, Is.EqualTo("Hero"));
                Assert.That(captured.Description, Is.EqualTo("Desc"));
                Assert.That(captured.CallToActionButtonType, Is.EqualTo("Primary"));
                Assert.That(captured.CallToActionButtonText, Is.EqualTo("Go"));
                Assert.That(captured.BackgroundImageAltText, Is.EqualTo("Alt"));
                Assert.That(captured.MobileBackgroundImageAltText, Is.EqualTo("MAlt"));
                Assert.That(captured.CallToActionButtonUrl, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_SetsBackgroundImageNull_WhenNoAttachment()
        {
            var page = new Contact
            {
                HeroBackgroundImage = new List<ContentAttachment>(),
                HeroBackgroundImage_Mobile = new List<ContentAttachment>()
            };

            SetupContent(page);

            HeroData? captured = null;

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => captured = data)
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(captured!.BackgroundImage, Is.Null);
                Assert.That(captured.MobileBackgroundImage, Is.Null);
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_SetsCallToActionButtonUrlNull_WhenNoLinkedPage()
        {
            var page = new Contact
            {
                HeroCallToActionButtonUrl = new List<IWebPageFieldsSource>()
            };

            SetupContent(page);

            HeroData? captured = null;

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .Callback<HeroData, CancellationToken>((data, _) => captured = data)
                .ReturnsAsync(new HeroDto());

            await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.CallToActionButtonUrl, Is.Null);
        }

        [Test]
        public async Task GetContactPageDtoAsync_ReturnsHero_FromHeroService()
        {
            var page = new Contact
            {
                HeroLayout = "Layout",
                HeroTitle = "Hero title",
                HeroDescription = "Hero desc"
            };

            SetupContent(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto
                {
                    Layout = "Mapped layout",
                    Title = "Mapped title",
                    Description = "Mapped desc"
                });

            var result = await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            Assert.That(result.Hero, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(result.Hero!.Layout, Is.EqualTo("Mapped layout"));
                Assert.That(result.Hero.Title, Is.EqualTo("Mapped title"));
                Assert.That(result.Hero.Description, Is.EqualTo("Mapped desc"));
            });
        }

        [Test]
        public async Task GetContactPageDtoAsync_UsesLinkedItemsMaxLevelFour()
        {
            var page = CreatePage();

            SetupContent(page);

            await _service.GetContactPageDtoAsync(null, CancellationToken.None);

            _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<Contact>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetContactPageDtoAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = CreatePage();

            SetupContent(page);

            await _service.GetContactPageDtoAsync(null, token);

            _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<Contact>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                token), Times.Once);

            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                token), Times.Once);
        }

        [Test]
        public async Task GetContactPageDtoAsync_UsesSuppliedPageArgument_WhenPageIsProvided()
        {
            var page = CreatePage(title: "Supplied Page");

            var result = await _service.GetContactPageDtoAsync(page, CancellationToken.None);

            Assert.That(result.Title, Is.EqualTo("Supplied Page"));

            _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<Contact>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetContactPageDtoAsync_DoesNotCallContentService_WhenPageIsProvided()
        {
            var page = CreatePage(title: "Direct Page");

            await _service.GetContactPageDtoAsync(page, CancellationToken.None);

            _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<Contact>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void GetContactPageDtoAsync_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Contact>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB fail"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetContactPageDtoAsync(null, CancellationToken.None));
        }

        [Test]
        public void GetContactPageDtoAsync_PropagatesException_FromHeroService()
        {
            var page = CreatePage();

            SetupContent(page);

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Hero fail"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetContactPageDtoAsync(null, CancellationToken.None));
        }
    }
}