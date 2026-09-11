using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class CareerDetailPageServiceTests
{

    private sealed class TestPrintorShareInfo : WebChannelSnippetInfo
    {
        public override bool DisablePrint { get; set; }
        public override bool DisableDownload { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
    }

    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ISidebarService> _sidebarServiceMock = null!;
    private CareerDetailPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
        _settingServiceMock = new Mock<ISettingService>();
        _contentServiceMock = new Mock<IContentService>();
        _sidebarServiceMock = new Mock<ISidebarService>();

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);

        _sidebarServiceMock
            .Setup(x => x.GetContentPageAuthorDtosGen(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SidebarPersonDto>());

        _service = new CareerDetailPageService(
            _heroServiceMock.Object,
            _cultureContextMock.Object,
            _socialMediaButtonsServiceMock.Object,
            _settingServiceMock.Object,
            _contentServiceMock.Object,
            _sidebarServiceMock.Object);
    }

    private static Career CreateCareer(
        bool showSidebar = false,
        bool showPrintButton = true,
        bool showSocialButtons = false,
        int parentId = 10)
        => new()
        {
            MetadataTitle = "Career title",
            MetadataDescription = "Career description",
            CareerReference = "REF-001",
            CareerApplyForm = "apply-form",
            ShowSidebar = showSidebar,
            ShowPrintButton = showPrintButton,
            ShowSocialMediaButtons = showSocialButtons,
            PageTitle = "Career page",
            MetadataOfficeLocation = [],
            CareerContactPeopleID = [],
            MetadataRelevantContactPerson = [],
            CareerJobTitle = [],
            CareerEmploymentType = [],
            CareerLevel = [],
            HeroBackgroundImage = [],
            HeroBackgroundImage_Mobile = [],
            HeroCallToActionButtonUrl = [],
            DownloadButtonAttachment = [],
            SystemFields = new WebPageFields
            {
                WebPageItemID = 100,
                WebPageItemParentID = parentId,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "career",
                WebPageUrlPath = "/career"
            }
        };

    private static Careers CreateCareers(int id, int parentId)
        => new()
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = id,
                WebPageItemParentID = parentId,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "careers",
                WebPageUrlPath = "/careers"
            }
        };

    private static SectionCareers CreateSectionCareers(bool showCopy)
        => new()
        {
            ShowCopyUrlButtonInJobOpportunity = showCopy,
            SystemFields = new WebPageFields
            {
                WebPageItemID = 5,
                WebPageItemParentID = 1,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "section-careers",
                WebPageUrlPath = "/section-careers"
            }
        };

    private void SetupCurrentCareer(Career? career)
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Career>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(career);
    }

    private void SetupCareersLookup(IEnumerable<Careers>? careers)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Careers>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(careers!);
    }

    private void SetupSectionCareersLookup(IEnumerable<SectionCareers> sectionCareers)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionCareers>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionCareers);
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
    {
        SetupCurrentCareer(null);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            Assert.That(result.Content, Is.Null);
            Assert.That(result.Form, Is.Null);
            Assert.That(result.Sidebar, Is.Null);
        });
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenPageExists_MapsContentAndForm()
    {
        var career = CreateCareer();
        SetupCurrentCareer(career);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Content.Title, Is.EqualTo("Career title"));
            Assert.That(result.Content.Content, Is.EqualTo("Career description"));
            Assert.That(result.Content.Reference, Is.EqualTo("REF-001"));
            Assert.That(result.Form, Is.EqualTo("apply-form"));
        });
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenPageExists_CallsHeroServiceOnce()
    {
        SetupCurrentCareer(CreateCareer());

        await _service.GetCareerPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenSidebarDisabled_SidebarIsNull()
    {
        SetupCurrentCareer(CreateCareer(showSidebar: false));

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar, Is.Null);
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenSidebarDisabled_DoesNotQuerySectionPages()
    {
        SetupCurrentCareer(CreateCareer(showSidebar: false));

        await _service.GetCareerPageDtoAsync(CancellationToken.None);


        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Careers>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void GetCareerPageDtoAsync_WhenCareersPageLookupReturnsNull_ThrowsMissingSectionPageException()
    {
        SetupCurrentCareer(CreateCareer(showSidebar: true));
        SetupCareersLookup(null);

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetCareerPageDtoAsync(CancellationToken.None));
    }

    [Test]
    public void GetCareerPageDtoAsync_WhenSectionCareersPageMissing_ThrowsMissingSectionPageException()
    {
        SetupCurrentCareer(CreateCareer(showSidebar: true));
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([]);

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetCareerPageDtoAsync(CancellationToken.None));
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenSidebarEnabled_MapsPublishDate()
    {
        var career = CreateCareer(showSidebar: true);
        SetupCurrentCareer(career);
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([CreateSectionCareers(showCopy: true)]);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar!.PublishDate, Is.EqualTo(career.SystemFields.ContentItemCommonDataFirstPublishedWhen));
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenSidebarEnabled_SetsRelevantContactPeopleTitle()
    {
        SetupCurrentCareer(CreateCareer(showSidebar: true));
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([CreateSectionCareers(showCopy: true)]);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar!.RelevantContactPeopleTitle, Is.EqualTo(ResourceStringKey.LiveSite.Careers.RelevantContactPeople));
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenRelevantContactsNotProvided_ReturnsEmptyArray()
    {
        var career = CreateCareer(showSidebar: true);
        career.MetadataRelevantContactPerson = null!;
        SetupCurrentCareer(career);
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([CreateSectionCareers(showCopy: true)]);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar!.RelevantContactPeople, Is.Empty);
    }



    [Test]
    public async Task GetCareerPageDtoAsync_WhenCopyLinkAllowedBySection_EnablesCopyLink()
    {
        var setting = new TestPrintorShareInfo { DisableCopyLinkButton = false };

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync([setting]);

        SetupCurrentCareer(CreateCareer(showSidebar: true));
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([CreateSectionCareers(showCopy: true)]);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar!.ShowCopyLinkButton, Is.True);
    }

    [Test]
    public async Task GetCareerPageDtoAsync_WhenSectionDisablesCopyLink_EndsAsFalse()
    {
        var setting = new TestPrintorShareInfo { DisableCopyLinkButton = true };

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync([setting]);

        SetupCurrentCareer(CreateCareer(showSidebar: true));
        SetupCareersLookup([CreateCareers(10, 20)]);
        SetupSectionCareersLookup([CreateSectionCareers(showCopy: false)]);

        var result = await _service.GetCareerPageDtoAsync(CancellationToken.None);

        Assert.That(result.Sidebar!.ShowCopyLinkButton, Is.False);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenNoSettings_ShowPrintButtonUsesPageFlag()
    {
        var result = await _service.GetSidebarDtoAsync(CreateCareer(showPrintButton: true), CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.True);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenSettingsDisablePrint_ShowPrintButtonIsFalse()
    {
        var setting = new TestPrintorShareInfo { DisablePrint = true };

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync([setting]);

        var result = await _service.GetSidebarDtoAsync(CreateCareer(showPrintButton: true), CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.False);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenSocialButtonsDisabled_DoesNotCallSocialMediaService()
    {
        await _service.GetSidebarDtoAsync(CreateCareer(showSocialButtons: false), CancellationToken.None);

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDto(It.IsAny<Career>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenSocialButtonsEnabled_ClearsNestedCopyLinkValue()
    {
        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(
                It.IsAny<Career>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto { ShowCopyLinkButton = true });

        var result = await _service.GetSidebarDtoAsync(CreateCareer(showSocialButtons: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SocialMediaButtons, Is.Not.Null);
            Assert.That(result.SocialMediaButtons!.ShowCopyLinkButton, Is.Null);
        });
    }
}
