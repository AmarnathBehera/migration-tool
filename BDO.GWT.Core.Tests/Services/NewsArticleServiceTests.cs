using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsArticleServiceTests
{
    private Mock<ISidebarService> _sidebarServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IExternalPeopleHelper> _externalPeopleHelperMock = null!;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock = null!;
    private NewsArticleService _service = null!;

    [SetUp]
    public void Setup()
    {
        _sidebarServiceMock = new Mock<ISidebarService>();
        _contentServiceMock = new Mock<IContentService>();
        _externalPeopleHelperMock = new Mock<IExternalPeopleHelper>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("https://example.com/news/test");

        SetupSectionNews(showCopyUrlButton: true);

        _service = new NewsArticleService(
            _sidebarServiceMock.Object,
            _contentServiceMock.Object,
            _externalPeopleHelperMock.Object,
            _webPageUrlResolverMock.Object);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ReturnsNull_WhenPageIsNull()
    {
        var result = await _service.GetSectionNewsPageDtoAsync(null!, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DoesNotCallSidebarService_WhenPageIsNull()
    {
        await _service.GetSectionNewsPageDtoAsync(null!, CancellationToken.None);

        _sidebarServiceMock.Verify(
            x => x.GetSidebarDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DoesNotCallUrlResolver_WhenPageIsNull()
    {
        await _service.GetSectionNewsPageDtoAsync(null!, CancellationToken.None);

        _webPageUrlResolverMock.Verify(
            x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()),
            Times.Never);
    }
    [Test]
    public async Task GetSectionNewsPageDtoAsync_SetsTitle_WhenShowTitleIsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowTitle = true;
        page.MetadataTitle = "News Title";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Title, Is.EqualTo("News Title"));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_TitleIsNull_WhenShowTitleIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowTitle = false;
        page.MetadataTitle = "Should Not Appear";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Title, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_SetsDescription_WhenShowDescriptionIsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowDescription = true;
        page.MetadataDescription = "Plain description";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Description, Is.EqualTo("Plain description"));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DescriptionIsNull_WhenShowDescriptionIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowDescription = false;
        page.MetadataDescription = "Should Not Appear";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Description, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_RemovesTildeFromDescription()
    {
        var page = CreateNewsArticle();
        page.ShowDescription = true;
        page.MetadataDescription = "<a href=\"~/page\">link</a>";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Description, Does.Not.Contain("~/"));
            Assert.That(result!.Description, Does.Contain("/page"));
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_MapsContent()
    {
        var page = CreateNewsArticle();
        page.Content = "<p>Hello</p>";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Content, Is.EqualTo("<p>Hello</p>"));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_RemovesTildeFromContent()
    {
        var page = CreateNewsArticle();
        page.Content = "<img src=\"~/media/x.png\" />";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Content, Does.Not.Contain("~/"));
            Assert.That(result!.Content, Does.Contain("/media/x.png"));
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ContentIsNull_WhenContentIsNull()
    {
        var page = CreateNewsArticle();
        page.Content = null!;

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Content, Is.Null);
    }
    [Test]
    public async Task GetSectionNewsPageDtoAsync_CallsSidebarService_WhenShowSidebarIsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _sidebarServiceMock.Verify(
            x => x.GetSidebarDtoAsync(page, "https://example.com/news/test", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DoesNotCallSidebarService_WhenShowSidebarIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = false;

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _sidebarServiceMock.Verify(
            x => x.GetSidebarDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_SidebarIsNull_WhenShowSidebarIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = false;

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Sidebar, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_SetsSidebar_WhenShowSidebarIsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        var sidebar = new ContentSidebarDto { RelevantContactPeopleTitle = "Authors" };

        _sidebarServiceMock
            .Setup(x => x.GetSidebarDtoAsync(page, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sidebar);

        _sidebarServiceMock
            .Setup(x => x.GetContentPageSidebarDetails(
                It.IsAny<ContentSidebarDto>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .ReturnsAsync(sidebar);

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Sidebar, Is.Not.Null);
            Assert.That(result.Sidebar!.RelevantContactPeopleTitle, Is.EqualTo("Authors"));
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesAbsoluteUrl_ToSidebarService()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(page))
            .Returns("https://bdo.com/articles/my-article");

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _sidebarServiceMock.Verify(
            x => x.GetSidebarDtoAsync(page, "https://bdo.com/articles/my-article", It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesLastPublishedDate_ToSidebarDetails()
    {
        var lastPublished = new DateTime(2024, 6, 15);
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublished;
        page.SystemFields.ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1);

        DateTime? capturedPublishDate = null;

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, _, pub, _) => capturedPublishDate = pub);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedPublishDate, Is.EqualTo(lastPublished));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_FallsBackToFirstPublished_WhenLastPublishedIsNull()
    {
        var firstPublished = new DateTime(2024, 1, 1);
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.SystemFields.ContentItemCommonDataLastPublishedWhen = null;
        page.SystemFields.ContentItemCommonDataFirstPublishedWhen = firstPublished;

        DateTime? capturedPublishDate = null;

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, _, pub, _) => capturedPublishDate = pub);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedPublishDate, Is.EqualTo(firstPublished));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesUpdatedDate_WhenNotMinValue()
    {
        var updatedDate = new DateTime(2024, 7, 20);
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.UpdatedOn = updatedDate;

        DateTime? capturedUpdatedDate = null;

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, _, _, upd) => capturedUpdatedDate = upd);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedUpdatedDate, Is.EqualTo(updatedDate));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesNullUpdatedDate_WhenUpdatedOnIsMinValue()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.UpdatedOn = DateTime.MinValue;

        DateTime? capturedUpdatedDate = new DateTime(2099, 1, 1);

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, _, _, upd) => capturedUpdatedDate = upd);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedUpdatedDate, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesRelevantPersonGuids_ToSidebarDetails()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.RelevantContactPerson = new[]
        {
            CreateWebPageFieldsSource(guid1),
            CreateWebPageFieldsSource(guid2)
        };

        List<Guid>? capturedGuids = null;

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, guids, _, _) => capturedGuids = guids);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedGuids, Is.Not.Null);
        Assert.That(capturedGuids, Is.EquivalentTo(new[] { guid1, guid2 }));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesEmptyGuidList_WhenRelevantContactPersonIsNull()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.RelevantContactPerson = null!;

        List<Guid>? capturedGuids = null;

        SetupSidebarCallbacks(
            captureContentDetails: (_, _, guids, _, _) => capturedGuids = guids);

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(capturedGuids, Is.Not.Null);
        Assert.That(capturedGuids, Is.Empty);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ShowCopyLinkButton_IsTrue_WhenBothFlagsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        SetupSidebarCallbacks(initialSidebar: new ContentSidebarDto { ShowCopyLinkButton = true });
        SetupSectionNews(showCopyUrlButton: true);

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Sidebar!.ShowCopyLinkButton, Is.True);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ShowCopyLinkButton_IsFalse_WhenSectionPageDisablesIt()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        SetupSidebarCallbacks(initialSidebar: new ContentSidebarDto { ShowCopyLinkButton = true });
        SetupSectionNews(showCopyUrlButton: false);

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Sidebar!.ShowCopyLinkButton, Is.False);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_ShowCopyLinkButton_IsFalse_WhenSidebarDisablesIt()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        SetupSidebarCallbacks(initialSidebar: new ContentSidebarDto { ShowCopyLinkButton = false });
        SetupSectionNews(showCopyUrlButton: true);

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.Sidebar!.ShowCopyLinkButton, Is.False);
    }

    [Test]
    public void GetSectionNewsPageDtoAsync_ThrowsMissingSectionPageException_WhenSectionNewsNotFound()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        SetupSidebarCallbacks();

        _contentServiceMock
            .Setup(x => x.RetrievePagesAsync<SectionNews>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionNews?>());

        Assert.ThrowsAsync<MissingSectionPageException>(
            () => _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None));
    }

    [Test]
    public void GetSectionNewsPageDtoAsync_ThrowsMissingSectionPageException_WhenRetrievePagesReturnsNull()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;

        SetupSidebarCallbacks();

        _contentServiceMock
            .Setup(x => x.RetrievePagesAsync<SectionNews>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<SectionNews?>?)null);

        Assert.ThrowsAsync<MissingSectionPageException>(
            () => _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None));
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PopulatesRelatedExternalPeople_WhenPresent()
    {
        var externalGuid = Guid.NewGuid();
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.RelatedExternalPeople = new[]
        {
            new ExternalPerson { SystemFields = new WebPageFields { ContentItemGUID = externalGuid } }
        };

        var expectedPeople = new[]
        {
            new SidebarExternalPersonDto
            {
                FullName = "Jane Doe",
                JobTitle = "Consultant",
                Url = "https://example.com/jane",
                Image = null
            }
        };

        SetupSidebarCallbacks();

        _externalPeopleHelperMock
            .Setup(x => x.GetSidebarExternalPeopleDtosAsync(
                It.Is<List<Guid>>(g => g.Contains(externalGuid)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPeople);

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Sidebar!.RelatedExternalPeople, Has.Length.EqualTo(1));
            Assert.That(result.Sidebar.RelatedExternalPeople[0].FullName, Is.EqualTo("Jane Doe"));
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DoesNotCallExternalPeopleHelper_WhenRelatedExternalPeopleIsNull()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        page.RelatedExternalPeople = null!;

        SetupSidebarCallbacks();

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _externalPeopleHelperMock.Verify(
            x => x.GetSidebarExternalPeopleDtosAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_DoesNotCallExternalPeopleHelper_WhenSidebarIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = false;

        await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        _externalPeopleHelperMock.Verify(
            x => x.GetSidebarExternalPeopleDtosAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_SetsCallToAction_WhenShowCallToActionIsTrue()
    {
        var page = CreateNewsArticle();
        page.ShowCallToAction = true;
        page.CallToActionDescription = "Read more";
        page.CallToActionButtonText = "Click";
        page.CallToActionButtonUrl = "/somewhere";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.CallToAction, Is.Not.Null);
            Assert.That(result.CallToAction!.Description, Is.EqualTo("Read more"));
            Assert.That(result.CallToAction.ButtonTitle, Is.EqualTo("Click"));
            Assert.That(result.CallToAction.ButtonUrl, Is.EqualTo("/somewhere"));
        });
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_CallToActionIsNull_WhenShowCallToActionIsFalse()
    {
        var page = CreateNewsArticle();
        page.ShowCallToAction = false;
        page.CallToActionButtonText = "Click";
        page.CallToActionButtonUrl = "/somewhere";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.CallToAction, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_CallToActionIsNull_WhenButtonTextIsNull()
    {
        var page = CreateNewsArticle();
        page.ShowCallToAction = true;
        page.CallToActionButtonText = null!;
        page.CallToActionButtonUrl = "/somewhere";

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.CallToAction, Is.Null);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_CallToActionIsNull_WhenButtonUrlIsNull()
    {
        var page = CreateNewsArticle();
        page.ShowCallToAction = true;
        page.CallToActionButtonText = "Click";
        page.CallToActionButtonUrl = null!;

        var result = await _service.GetSectionNewsPageDtoAsync(page, CancellationToken.None);

        Assert.That(result!.CallToAction, Is.Null);
    }
    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesCancellationToken_ToSidebarService()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        SetupSidebarCallbacks();

        await _service.GetSectionNewsPageDtoAsync(page, token);

        _sidebarServiceMock.Verify(
            x => x.GetSidebarDtoAsync(page, It.IsAny<string>(), token),
            Times.Once);
    }

    [Test]
    public async Task GetSectionNewsPageDtoAsync_PassesCancellationToken_ToContentService()
    {
        var page = CreateNewsArticle();
        page.ShowSidebar = true;
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        SetupSidebarCallbacks();

        await _service.GetSectionNewsPageDtoAsync(page, token);

        _contentServiceMock.Verify(
            x => x.RetrievePagesAsync<SectionNews>(token),
            Times.Once);
    }
    private static NewsArticle CreateNewsArticle()
    {
        return new NewsArticle
        {
            SystemFields = new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 6, 1),
                ContentItemCommonDataFirstPublishedWhen = new DateTime(2024, 1, 1),
                WebPageItemID = 1,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "test-article",
                WebPageUrlPath = "/news/test-article"
            },
            MetadataTitle = "Test Article",
            MetadataDescription = "Test description",
            Content = "<p>Test content</p>",
            UpdatedOn = DateTime.MinValue,
            ShowTitle = true,
            ShowDescription = true,
            ShowSidebar = false,
            ShowCallToAction = false,
            CallToActionDescription = string.Empty,
            CallToActionButtonText = string.Empty,
            CallToActionButtonUrl = string.Empty,
            RelevantContactPerson = Array.Empty<IWebPageFieldsSource>(),
            RelatedExternalPeople = Array.Empty<ExternalPerson>()
        };
    }

    private static IWebPageFieldsSource CreateWebPageFieldsSource(Guid guid)
    {
        var mock = new Mock<IWebPageFieldsSource>();
        mock.Setup(x => x.SystemFields).Returns(new WebPageFields
        {
            ContentItemGUID = guid
        });
        return mock.Object;
    }

    private void SetupSidebarCallbacks(
        ContentSidebarDto? initialSidebar = null,
        Action<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>? captureContentDetails = null)
    {
        var sidebar = initialSidebar ?? new ContentSidebarDto();

        _sidebarServiceMock
            .Setup(x => x.GetSidebarDtoAsync(It.IsAny<NewsArticle>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sidebar);

        var setup = _sidebarServiceMock
            .Setup(x => x.GetContentPageSidebarDetails(
                It.IsAny<ContentSidebarDto>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()));

        if (captureContentDetails != null)
        {
            setup.Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(captureContentDetails)
                 .ReturnsAsync(sidebar);
        }
        else
        {
            setup.ReturnsAsync(sidebar);
        }
    }

    private void SetupSectionNews(bool showCopyUrlButton)
    {
        _contentServiceMock
            .Setup(x => x.RetrievePagesAsync<SectionNews>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionNews?>
            {
                new SectionNews { ShowCopyUrlButtonInNewsArticle = showCopyUrlButton }
            });
    }
}
