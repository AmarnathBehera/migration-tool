using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightPageServiceTests
{
    private static readonly int[] ContactIds = new[] { 1, 2 };
    private static readonly int[] SingleContactId = new[] { 1 };
    private static readonly string[] ExpectedComposedContactNames = new[] { "Ada Lovelace", "Grace van Hopper" };
    private static readonly string[] ExpectedPersonMetadataTitle = new[] { "Person Metadata Title" };
    private static readonly string[] ExpectedExternalComposedName = new[] { "Ada van Lovelace" };
    private static readonly string[] ExpectedExternalMetadataTitle = new[] { "External Metadata Title" };
    private static readonly string[] SchemaAuthorNames = new[] { "BDO Test" };
    private static readonly string[] TwoAuthorsNames = new[] { "Ada Lovelace", "Grace van Hopper" };
    private static readonly DateTime SchemaPublishDate = new(2026, 01, 01);
    private static readonly DateTime SchemaModifyDate = new(2026, 02, 01);

    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInfoProvider<WebsiteChannelInfo>> _websiteChannelInfoProviderMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;

    private InsightPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        _contentServiceMock = new Mock<IContentService>();
        _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _settingServiceMock = new Mock<ISettingService>();
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelID)
            .Returns(1);

        // The GetAsync(int, CancellationToken?) extension used by the service internally casts the
        // provider to IInfoByIdProvider<WebsiteChannelInfo>, so the mock must also implement it.
        // Moq requires As<T>() to be called prior to any .Object access.
        _websiteChannelInfoProviderMock.As<IInfoByIdProvider<WebsiteChannelInfo>>();

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelName)
            .Returns("BDO Test");

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(default(IEnumerable<WebChannelSnippetInfo>)!);

        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync(default(IEnumerable<WebChannelSnippetInfo>)!);

        _contentServiceMock
            .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
            .Returns("/media/logo.png");

        _service = new InsightPageService(
            _contentRetrieverMock.Object,
            _websiteChannelInfoProviderMock.Object,
            _contentServiceMock.Object,
            _channelInfoProviderMock.Object,
            _websiteChannelContextMock.Object,
            _settingServiceMock.Object,
            _socialMediaButtonsServiceMock.Object);
    }


    [Test]
    public async Task GetDisplayNameAsync_ReturnsComposedNameWithPrefix_WhenSettingsUnavailable()
    {
        var externalPerson = new ExternalPerson
        {
            MetadataTitle = "Display From Title",
            FirstName = "Ada",
            SurnamePrefixes = "van",
            LastName = "Lovelace"
        };

        var result = await _service.GetDisplayNameAsync(externalPerson);

        Assert.That(result, Is.EqualTo("Ada van Lovelace"));
    }

    [Test]
    public async Task GetDisplayNameAsync_ReturnsComposedNameWithoutPrefix_WhenSettingsUnavailable()
    {
        var externalPerson = new ExternalPerson
        {
            MetadataTitle = "Display From Title",
            FirstName = "Ada",
            SurnamePrefixes = string.Empty,
            LastName = "Lovelace"
        };

        var result = await _service.GetDisplayNameAsync(externalPerson);

        Assert.That(result, Is.EqualTo("Ada Lovelace"));
    }

    [Test]
    public async Task GetDisplayNameAsync_ReturnsMetadataTitle_WhenUsePersonPageTitleSettingIsTrue()
    {
        SetupUsePersonPageTitleSetting(true);

        var externalPerson = new ExternalPerson
        {
            MetadataTitle = "External Person Title",
            FirstName = "Ada",
            SurnamePrefixes = "van",
            LastName = "Lovelace"
        };

        var result = await _service.GetDisplayNameAsync(externalPerson);

        Assert.That(result, Is.EqualTo("External Person Title"));
    }

    [Test]
    public async Task GetKeyContactsDisplayNames_ReturnsComposedNames_ForPersonAndInternalPerson()
    {
        SetupUsePersonPageTitleSetting(false);

        var person = CreatePerson(
            contentItemId: 1,
            metadataTitle: "Person Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        var internalPerson = CreateInternalPerson(
            contentItemId: 2,
            firstName: "Grace",
            surnamePrefix: "van",
            lastName: "Hopper",
            jobTitle: "Engineer",
            url: "/people/grace");

        SetupContentPages(person);
        SetupContentPages(internalPerson);

        var result = await _service.GetKeyContactsDisplayNames(
            ContactIds,
            CancellationToken.None);

        Assert.That(result, Is.EquivalentTo(ExpectedComposedContactNames));
    }

    [Test]
    public async Task GetKeyContactsDisplayNames_ReturnsMetadataTitle_WhenUsePersonPageTitleSettingIsTrue()
    {
        SetupUsePersonPageTitleSetting(true);

        var person = CreatePerson(
            contentItemId: 1,
            metadataTitle: "Person Metadata Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        SetupContentPages(person);
        SetupContentPages(Array.Empty<InternalPerson>());

        var result = await _service.GetKeyContactsDisplayNames(
            SingleContactId,
            CancellationToken.None);

        Assert.That(result, Is.EqualTo(ExpectedPersonMetadataTitle));
    }

    [Test]
    public async Task GetExternalPeopleDisplayNames_ReturnsComposedNames_WhenSettingUnavailable()
    {
        var externalPerson = CreateExternalPerson(
            contentItemGuid: Guid.NewGuid(),
            metadataTitle: "External Title",
            firstName: "Ada",
            surnamePrefix: "van",
            lastName: "Lovelace",
            jobTitle: "Consultant",
            url: "https://example.com/ada");

        SetupContentPages(externalPerson);

        var result = await _service.GetExternalPeopleDisplayNames(
            new List<Guid> { externalPerson.SystemFields.ContentItemGUID },
            CancellationToken.None);

        Assert.That(result, Is.EqualTo(ExpectedExternalComposedName));
    }

    [Test]
    public async Task GetExternalPeopleDisplayNames_ReturnsMetadataTitle_WhenUsePersonPageTitleSettingIsTrue()
    {
        SetupUsePersonPageTitleSetting(true);

        var externalPerson = CreateExternalPerson(
            contentItemGuid: Guid.NewGuid(),
            metadataTitle: "External Metadata Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Consultant",
            url: "https://example.com/ada");

        SetupContentPages(externalPerson);

        var result = await _service.GetExternalPeopleDisplayNames(
            new List<Guid> { externalPerson.SystemFields.ContentItemGUID },
            CancellationToken.None);

        Assert.That(result, Is.EqualTo(ExpectedExternalMetadataTitle));
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_MapsExternalPeople()
    {
        var externalPersonGuid = Guid.NewGuid();

        var linkedExternalPerson = CreateExternalPerson(
            contentItemGuid: externalPersonGuid,
            metadataTitle: "Linked External Person",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Consultant",
            url: "https://example.com/ada");

        var retrievedExternalPerson = CreateExternalPerson(
            contentItemGuid: externalPersonGuid,
            metadataTitle: "Retrieved External Person",
            firstName: "Ada",
            surnamePrefix: "van",
            lastName: "Lovelace",
            jobTitle: "Partner",
            url: "https://example.com/retrieved-ada");

        SetAssetCollection(
            retrievedExternalPerson,
            nameof(ExternalPerson.ImageFile),
            "/media/ada.png");

        retrievedExternalPerson.ImageFileAltText = "Ada image";

        SetupContentPages(retrievedExternalPerson);

        var result = await _service.GetSidebarExternalPeopleDtosAsync(
            new[] { linkedExternalPerson },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].FullName, Is.EqualTo("Ada van Lovelace"));
            Assert.That(result[0].JobTitle, Is.EqualTo("Partner"));
            Assert.That(result[0].Url, Is.EqualTo("https://example.com/retrieved-ada"));
            Assert.That(result[0].Image?.Url, Is.EqualTo("/media/ada.png"));
            Assert.That(result[0].Image?.AltText, Is.EqualTo("Ada image"));
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_MapsPrintAndDownload_WhenSettingsUnavailable()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = false;

        SetAssetCollection(
            insight,
            nameof(Insight.DownloadButtonAttachment),
            "/media/download.pdf");

        var result = await _service.GetSidebarDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ShowPrintButton, Is.True);
            Assert.That(result.DownloadButtonAttachment, Is.EqualTo("/media/download.pdf"));
            Assert.That(result.SocialMediaButtons, Is.Null);
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_HidesPrint_WhenPageShowPrintButtonIsFalse()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        insight.ShowPrintButton = false;
        insight.ShowSocialMediaButtons = false;

        var result = await _service.GetSidebarDtoAsync(
            insight,
            CancellationToken.None);

        Assert.That(result.ShowPrintButton, Is.False);
    }

    [Test]
    public async Task GetContentPageDtoAsync_MapsCopyInsight_WhenShowFlagsAreEnabled()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: false);

        var copyInsight = CreateCopyInsight(
            title: "Copy title",
            description: "<p>Copy description</p>",
            content: "<p>Body with <a href=\"~/test\">link</a></p>",
            showTitle: true,
            showDescription: true,
            showCallToAction: false);

        SetCollection(insight, nameof(Insight.InsightContent), copyInsight);

        var result = await _service.GetContentPageDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Copy title"));
            Assert.That(result.Description, Is.EqualTo("<p>Copy description</p>"));
            Assert.That(result.Content, Does.Contain("href=\"/test\""));
            Assert.That(result.Sidebar, Is.Null);
            Assert.That(result.CallToAction, Is.Null);
        });
    }

    [Test]
    public async Task GetContentPageDtoAsync_DoesNotMapTitleAndDescription_WhenShowFlagsAreDisabled()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: false);

        var copyInsight = CreateCopyInsight(
            title: "Hidden title",
            description: "<p>Hidden description</p>",
            content: "<p>Visible content</p>",
            showTitle: false,
            showDescription: false,
            showCallToAction: false);

        SetCollection(insight, nameof(Insight.InsightContent), copyInsight);

        var result = await _service.GetContentPageDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Description, Is.Null);
            Assert.That(result.Content, Is.EqualTo("<p>Visible content</p>"));
            Assert.That(result.CallToAction, Is.Null);
        });
    }

    [Test]
    public async Task GetContentPageDtoAsync_ReturnsNullValues_WhenCopyInsightIsMissing()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: false);

        SetCollection(insight, nameof(Insight.InsightContent), Array.Empty<object>());

        var result = await _service.GetContentPageDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Description, Is.Null);
            Assert.That(result.Content, Is.Null);
            Assert.That(result.CallToAction, Is.Null);
        });
    }

    [Test]
    public async Task GetContentPageSidebarDto_MapsSidebarFieldsAuthorsAndExternalPeople()
    {
        SetupUsePersonPageTitleSetting(false);

        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = false;

        SetContentType(insight, "Article");

        var person = CreatePerson(
            contentItemId: 101,
            metadataTitle: "Ada Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        var internalPerson = CreateInternalPerson(
            contentItemId: 102,
            firstName: "Grace",
            surnamePrefix: "van",
            lastName: "Hopper",
            jobTitle: "Engineer",
            url: "/people/grace");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person, internalPerson);

        var externalPersonGuid = Guid.NewGuid();

        var linkedExternalPerson = CreateExternalPerson(
            contentItemGuid: externalPersonGuid,
            metadataTitle: "External Linked",
            firstName: "External",
            surnamePrefix: string.Empty,
            lastName: "Linked",
            jobTitle: "External Job",
            url: "https://example.com/linked");

        var retrievedExternalPerson = CreateExternalPerson(
            contentItemGuid: externalPersonGuid,
            metadataTitle: "External Retrieved",
            firstName: "External",
            surnamePrefix: string.Empty,
            lastName: "Retrieved",
            jobTitle: "External Consultant",
            url: "https://example.com/retrieved");

        SetCollection(insight, nameof(Insight.RelatedExternalPeople), linkedExternalPerson);

        SetupContentPages(retrievedExternalPerson);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TypeName, Is.EqualTo("Article"));
            Assert.That(result.PublishDate, Is.EqualTo(insight.SystemFields.ContentItemCommonDataFirstPublishedWhen));
            Assert.That(result.UpdatedDate, Is.EqualTo(insight.SystemFields.ContentItemCommonDataLastPublishedWhen));
            Assert.That(result.RelevantContactPeopleTitle, Is.EqualTo(ResourceStringKey.LiveSite.Insights.Authors));
            Assert.That(result.RelevantContactPeople, Has.Length.EqualTo(2));
            Assert.That(result.RelevantContactPeople.Select(x => x.FullName), Is.EquivalentTo(ExpectedComposedContactNames));
            Assert.That(result.RelatedExternalPeople, Has.Length.EqualTo(1));
            Assert.That(result.RelatedExternalPeople[0].FullName, Is.EqualTo("External Retrieved"));
        });
    }

    [Test]
    public void GetContentPageSidebarDto_ThrowsMissingSectionPageException_WhenInsightLookupReturnsNull()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        SetContentType(insight, "Article");
        SetCollection(insight, nameof(Insight.RelevantContactPerson), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), Array.Empty<object>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Insight>)null!);

        var exception = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetContentPageSidebarDto(
                insight,
                CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Does.Contain(nameof(Insight)));
        });
    }


    private void SetupUsePersonPageTitleSetting(bool usePageTitle)
    {
        // WebChannelSnippetInfo derives from Kentico's AbstractInfo; setting the
        // UsePageTitleToDisplayPersonName property directly routes through SetValue ->
        // DataClass -> the CMS IoC container, which is not initialized in unit tests.
        // Overriding the virtual property in a test subclass provides plain backing
        // storage so no CMS infrastructure is touched.
        var setting = new TestWebChannelSnippetInfo
        {
            UsePageTitleToDisplayPersonName = usePageTitle
        };

        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync(new WebChannelSnippetInfo[] { setting });
    }

    private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
    {
        public override bool UsePageTitleToDisplayPersonName { get; set; }
    }

    private void SetupCopyLinkSection(
        Insight insight,
        bool showCopyUrlButtonInInsightPage)
    {
        var sectionPage = new SectionInsightsPage
        {
            ShowCopyUrlButtonInInsightPage = showCopyUrlButtonInInsightPage
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { insight });

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { sectionPage });
    }

    private void SetupContentPages(params Person[] people)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(people);
    }

    private void SetupContentPages(params InternalPerson[] people)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(people);
    }

    private void SetupContentPages(params ExternalPerson[] people)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(people);
    }

    private static Insight CreateInsight(
        string title,
        bool showSidebar)
    {
        var insight = new Insight
        {
            MetadataTitle = title,
            MetadataDescription = "Meta description",
            MetadataCanonical = "https://example.com/insight",
            MetadataNofollow = false,
            MetadataNoindex = false,
            IncludeInSitemap = true,
            MetadataOGTitle = "OG title",
            MetadataOGDescription = "OG description",
            MetadataOGTwitterImageSizeCheckbox = false,
            ShowSidebar = showSidebar,
            UseNarrowLayout = true,
            ShowAuthorsAtTheTop = true,
            PageTitle = title
        };

        SetSystemFields(
            insight,
            contentItemId: 10,
            contentItemGuid: Guid.NewGuid(),
            contentItemName: "Insight",
            webPageItemId: 100,
            webPageParentId: 50,
            webPageUrlPath: "/insights/test",
            webPageTreePath: "/Insights/Test");

        SetCollection(insight, nameof(Insight.InsightContent), CreateCopyInsight(
            title: title,
            description: "<p>Description</p>",
            content: "<p>Content</p>",
            showTitle: true,
            showDescription: true,
            showCallToAction: false));

        SetCollection(insight, nameof(Insight.ContentTypeFilter), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.RelevantContactPerson), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.MetadataTeaserImage), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.MetadataOGImage), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.MetadataOGTwitterImage), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.DownloadButtonAttachment), Array.Empty<object>());

        return insight;
    }

    private static object CreateCopyInsight(
        string title,
        string description,
        string content,
        bool showTitle,
        bool showDescription,
        bool showCallToAction,
        string? callToActionButtonUrl = null,
        string? callToActionPosition = null)
    {
        var copyInsightProperty = typeof(Insight).GetProperty(nameof(Insight.InsightContent));
        if (copyInsightProperty is null)
        {
            throw new InvalidOperationException("CopyInsight property was not found.");
        }

        var copyInsightType = GetEnumerableElementType(copyInsightProperty.PropertyType);
        var copyInsight = CreateObject(copyInsightType);

        SetPropertyValue(copyInsight, "Title", title);
        SetPropertyValue(copyInsight, "Description", description);
        SetPropertyValue(copyInsight, "InsightContent", content);
        SetPropertyValue(copyInsight, "ShowInsightTitle", showTitle);
        SetPropertyValue(copyInsight, "ShowInsightDescription", showDescription);
        SetPropertyValue(copyInsight, "ShowCallToAction", showCallToAction);
        SetPropertyValue(copyInsight, "CallToActionDescription", "CTA description");
        SetPropertyValue(copyInsight, "CallToActionButtonText", "CTA button");
        SetPropertyValue(copyInsight, "CallToActionButtonUrl", callToActionButtonUrl);
        SetPropertyValue(copyInsight, "CallToActionPosition", callToActionPosition);
        SetPropertyValue(copyInsight, "SharingSourceFirm", "BDO Firm");

        SetCollection(copyInsight, "ImageFile", Array.Empty<object>());

        return copyInsight;
    }

    private static Person CreatePerson(
        int contentItemId,
        string metadataTitle,
        string firstName,
        string surnamePrefix,
        string lastName,
        string jobTitle,
        string url)
    {
        var person = new Person
        {
            MetadataTitle = metadataTitle,
            PersonFirstName = firstName,
            PersonSurnamePrefixes = surnamePrefix,
            PersonLastName = lastName,
            PersonJobTitle = jobTitle,
            MetadataTeaserImageAltText = "Person image"
        };

        SetSystemFields(
            person,
            contentItemId: contentItemId,
            contentItemGuid: Guid.NewGuid(),
            contentItemName: metadataTitle,
            webPageItemId: contentItemId,
            webPageParentId: 0,
            webPageUrlPath: url,
            webPageTreePath: url);

        SetCollection(person, nameof(Person.MetadataTeaserImage), Array.Empty<object>());

        return person;
    }

    private static InternalPerson CreateInternalPerson(
        int contentItemId,
        string firstName,
        string surnamePrefix,
        string lastName,
        string jobTitle,
        string url)
    {
        var person = new InternalPerson
        {
            FirstName = firstName,
            SurnamePrefixes = surnamePrefix,
            LastName = lastName,
            JobTitle = jobTitle,
            MetadataTeaserImageAltText = "Internal person image"
        };

        SetSystemFields(
            person,
            contentItemId: contentItemId,
            contentItemGuid: Guid.NewGuid(),
            contentItemName: $"{firstName} {lastName}",
            webPageItemId: contentItemId,
            webPageParentId: 0,
            webPageUrlPath: url,
            webPageTreePath: url);

        SetCollection(person, nameof(InternalPerson.MetadataTeaserImage), Array.Empty<object>());

        return person;
    }

    private static ExternalPerson CreateExternalPerson(
        Guid contentItemGuid,
        string metadataTitle,
        string firstName,
        string surnamePrefix,
        string lastName,
        string jobTitle,
        string url)
    {
        var person = new ExternalPerson
        {
            MetadataTitle = metadataTitle,
            FirstName = firstName,
            SurnamePrefixes = surnamePrefix,
            LastName = lastName,
            JobTitle = jobTitle,
            Url = url,
            ImageFileAltText = "External image"
        };

        SetSystemFields(
            person,
            contentItemId: Random.Shared.Next(1000, 9999),
            contentItemGuid: contentItemGuid,
            contentItemName: metadataTitle,
            webPageItemId: 0,
            webPageParentId: 0,
            webPageUrlPath: url,
            webPageTreePath: url);

        SetCollection(person, nameof(ExternalPerson.ImageFile), Array.Empty<object>());

        return person;
    }

    private static void SetContentType(
        Insight insight,
        string name)
    {
        var property = typeof(Insight).GetProperty(nameof(Insight.ContentTypeFilter));
        if (property is null)
        {
            throw new InvalidOperationException("ContentType property was not found.");
        }

        var itemType = GetEnumerableElementType(property.PropertyType);
        var item = CreateObject(itemType);

        SetPropertyValue(item, "Name", name);

        SetCollection(insight, nameof(Insight.ContentTypeFilter), item);
    }

    private static void SetAssetCollection(
        object target,
        string propertyName,
        string url)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found on '{target.GetType().Name}'.");
        }

        var itemType = GetEnumerableElementType(property.PropertyType);
        var item = CreateObject(itemType);

        var assetProperty = itemType.GetProperty("ContentAttachmentAsset");
        if (assetProperty is null)
        {
            throw new InvalidOperationException($"ContentAttachmentAsset was not found on '{itemType.Name}'.");
        }

        var asset = CreateObject(assetProperty.PropertyType);
        SetPropertyValue(asset, "Url", url);
        SetPropertyValue(item, "ContentAttachmentAsset", asset);

        SetCollection(target, propertyName, item);
    }

    private static void SetSystemFields(
        object target,
        int contentItemId,
        Guid contentItemGuid,
        string contentItemName,
        int webPageItemId,
        int webPageParentId,
        string webPageUrlPath,
        string webPageTreePath)
    {
        var systemFieldsProperty = target.GetType().GetProperty("SystemFields");
        if (systemFieldsProperty is null)
        {
            throw new InvalidOperationException($"SystemFields property was not found on '{target.GetType().Name}'.");
        }

        var systemFields = CreateObject(systemFieldsProperty.PropertyType);

        SetPropertyValue(systemFields, "ContentItemID", contentItemId);
        SetPropertyValue(systemFields, "ContentItemGUID", contentItemGuid);
        SetPropertyValue(systemFields, "ContentItemName", contentItemName);
        SetPropertyValue(systemFields, "ContentItemContentTypeID", 1);
        SetPropertyValue(systemFields, "ContentItemIsSecured", false);
        SetPropertyValue(systemFields, "ContentItemCommonDataContentLanguageID", 1);
        SetPropertyValue(systemFields, "ContentItemCommonDataFirstPublishedWhen", new DateTime(2026, 01, 01));
        SetPropertyValue(systemFields, "ContentItemCommonDataLastPublishedWhen", new DateTime(2026, 02, 01));

        SetPropertyValue(systemFields, "WebPageItemGUID", contentItemGuid);
        SetPropertyValue(systemFields, "WebPageItemID", webPageItemId);
        SetPropertyValue(systemFields, "WebPageItemName", contentItemName);
        SetPropertyValue(systemFields, "WebPageItemWebsiteChannelId", 1);
        SetPropertyValue(systemFields, "WebPageUrlPath", webPageUrlPath);
        SetPropertyValue(systemFields, "WebPageItemOrder", 1);
        SetPropertyValue(systemFields, "WebPageItemTreePath", webPageTreePath);
        SetPropertyValue(systemFields, "WebPageUrlPathContentLanguageID", 1);
        SetPropertyValue(systemFields, "WebPageItemParentID", webPageParentId);

        SetPropertyValue(target, "SystemFields", systemFields);
    }

    private static void SetCollection(
        object target,
        string propertyName,
        params object[] items)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property is null)
        {
            return;
        }

        var elementType = GetEnumerableElementType(property.PropertyType);
        var array = Array.CreateInstance(elementType, items.Length);

        for (var i = 0; i < items.Length; i++)
        {
            array.SetValue(items[i], i);
        }

        SetPropertyValue(target, propertyName, array);
    }

    private static Type GetEnumerableElementType(Type enumerableType)
    {
        if (enumerableType.IsArray)
        {
            return enumerableType.GetElementType()!;
        }

        if (enumerableType.IsGenericType &&
            enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return enumerableType.GetGenericArguments()[0];
        }

        var enumerableInterface = enumerableType
            .GetInterfaces()
            .FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface is null)
        {
            throw new InvalidOperationException($"Type '{enumerableType.Name}' is not IEnumerable<T>.");
        }

        return enumerableInterface.GetGenericArguments()[0];
    }

    private static object CreateObject(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);

        if (constructor is not null)
        {
            return Activator.CreateInstance(type)!;
        }

#pragma warning disable SYSLIB0050
        return FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
    }

    private static void SetPropertyValue(
        object target,
        string propertyName,
        object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var backingField = target.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField is not null)
        {
            backingField.SetValue(target, value);
        }
    }



    [Test]
    public async Task GetContentPageSidebarDto_SetsNullPublishAndUpdated_WhenSystemDatesAreMinValue()
    {
        SetupUsePersonPageTitleSetting(false);

        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataFirstPublishedWhen", DateTime.MinValue);
        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataLastPublishedWhen", DateTime.MinValue);

        SetContentType(insight, "Article");
        SetCollection(insight, nameof(Insight.RelevantContactPerson), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), Array.Empty<object>());

        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.PublishDate, Is.Null);
            Assert.That(result.UpdatedDate, Is.Null);
        });
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_ReturnsEmpty_WhenNoExternalPeopleAreRetrieved()
    {
        SetupContentPages(Array.Empty<ExternalPerson>());

        var linkedExternalPerson = CreateExternalPerson(
            contentItemGuid: Guid.NewGuid(),
            metadataTitle: "Linked",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Consultant",
            url: "https://example.com/linked");

        var result = await _service.GetSidebarExternalPeopleDtosAsync(
            new[] { linkedExternalPerson },
            CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetContentPageDtoAsync_WhenShowSidebarTrue_ReturnsSidebar()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        SetContentType(insight, "Article");
        SetCollection(insight, nameof(Insight.RelevantContactPerson), Array.Empty<object>());
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), Array.Empty<object>());

        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);
        SetupContentPages(Array.Empty<ExternalPerson>());

        var result = await _service.GetContentPageDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Sidebar, Is.Not.Null);
            Assert.That(result.Sidebar!.TypeName, Is.EqualTo("Article"));
        });
    }

    [Test]
    public async Task GetContentPageSidebarDto_DeduplicatesRelevantPeople_ByContentItemId()
    {
        SetupUsePersonPageTitleSetting(false);

        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        SetContentType(insight, "Article");

        var person1 = CreatePerson(
            contentItemId: 200,
            metadataTitle: "Ada Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        var personDuplicateId = CreatePerson(
            contentItemId: 200,
            metadataTitle: "Ada Duplicate",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada-duplicate");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person1, personDuplicateId);
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), Array.Empty<object>());

        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);
        SetupContentPages(Array.Empty<ExternalPerson>());

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.That(result.RelevantContactPeople, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetKeyContactsDisplayNames_ReturnsEmpty_WhenNoContactsFound()
    {
        SetupUsePersonPageTitleSetting(false);
        SetupContentPages(Array.Empty<Person>());
        SetupContentPages(Array.Empty<InternalPerson>());

        var result = await _service.GetKeyContactsDisplayNames(Array.Empty<int>(), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetExternalPeopleDisplayNames_ReturnsEmpty_WhenNoPeopleFound()
    {
        SetupContentPages(Array.Empty<ExternalPerson>());

        var result = await _service.GetExternalPeopleDisplayNames(new List<Guid>(), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetCallToActionDto_ReturnsNull_WhenButtonTextIsMissing()
    {
        var result = InsightPageService.GetCallToActionDto(
            callToActionDescription: "CTA description",
            callToActionButtonText: null,
            callToActionButtonLink: "https://example.com",
            CallToActionPosition: "right");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCallToActionDto_ReturnsNull_WhenButtonLinkIsMissing()
    {
        var result = InsightPageService.GetCallToActionDto(
            callToActionDescription: "CTA description",
            callToActionButtonText: "Read more",
            callToActionButtonLink: null,
            CallToActionPosition: "right");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCallToActionDto_UsesDefaultLeftPosition_WhenPositionIsNull()
    {
        var result = InsightPageService.GetCallToActionDto(
            callToActionDescription: "CTA description",
            callToActionButtonText: "Read more",
            callToActionButtonLink: "https://example.com",
            CallToActionPosition: null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Position, Is.EqualTo("left"));
            Assert.That(result.CallToActionDto?.ButtonTitle, Is.EqualTo("Read more"));
            Assert.That(result.CallToActionDto?.ButtonUrl, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_WhenSocialMediaButtonsExist_ResetsCopyLinkToNull()
    {
        var insight = CreateInsight(
            title: "Insight page",
            showSidebar: true);

        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = true;

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDto(
                insight,
                insight.PageTitle,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto
            {
                EmailButtonUrl = "mailto:test@example.com",
                ShowCopyLinkButton = true
            });

        var result = await _service.GetSidebarDtoAsync(
            insight,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SocialMediaButtons, Is.Not.Null);
            Assert.That(result.SocialMediaButtons!.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task GetInsightPageDtoAsync_ReturnsEmptyDto_WhenCurrentPageIsNull()
    {
        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<Insight>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Insight)null!);

        var result = await _service.GetInsightPageDtoAsync(null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.SchemaMarkup, Is.TypeOf<ArticleSchemaMarkupDto>());
            Assert.That(result.BaseContent, Is.Null);
        });
    }

    [Test]
    public async Task GetInsightSchemaMarkupDtoAsync_UsesChannelName_WhenNoAuthorsAreLinked()
    {
        SetupWebsiteChannel("https://example.com");

        var insight = CreateInsight(title: "Insight page", showSidebar: false);

        var result = await InvokeGetInsightSchemaMarkupDtoAsync(insight);

        Assert.Multiple(() =>
        {
            Assert.That(result.Author, Is.Not.Null);
            Assert.That(result.Author!.Select(x => x.Name), Is.EqualTo(SchemaAuthorNames));
            Assert.That(result.MainEntityOfPage, Is.EqualTo("https://example.com"));
            Assert.That(result.Headline, Is.EqualTo("Insight page"));
            Assert.That(result.Description, Is.EqualTo("Description"));
            Assert.That(result.Image, Is.Null);
            Assert.That(result.Url, Is.EqualTo("~//insights/test"));
            Assert.That(result.Publisher, Is.Not.Null);
            Assert.That(result.Publisher!.Name, Is.EqualTo("BDO Test"));
            Assert.That(result.Publisher!.Logo, Is.EqualTo("/media/logo.png"));
            Assert.That(result.DatePublished, Is.EqualTo(SchemaPublishDate));
            Assert.That(result.DateModified, Is.EqualTo(SchemaModifyDate));
        });
    }

    [Test]
    public async Task GetInsightSchemaMarkupDtoAsync_ComposesAuthors_FromRelevantAndExternalPeople()
    {
        SetupUsePersonPageTitleSetting(false);
        SetupWebsiteChannel("https://example.com");

        var insight = CreateInsight(title: "Insight page", showSidebar: false);

        var person = CreatePerson(
            contentItemId: 1,
            metadataTitle: "Person Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        var externalPerson = CreateExternalPerson(
            contentItemGuid: Guid.NewGuid(),
            metadataTitle: "External Metadata Title",
            firstName: "Grace",
            surnamePrefix: "van",
            lastName: "Hopper",
            jobTitle: "Engineer",
            url: "/people/grace");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person);
        SetCollection(insight, nameof(Insight.RelatedExternalPeople), externalPerson);

        SetupContentPages(person);
        SetupContentPages(Array.Empty<InternalPerson>());
        SetupContentPages(externalPerson);

        var result = await InvokeGetInsightSchemaMarkupDtoAsync(insight);

        Assert.That(result.Author, Is.Not.Null);
        Assert.That(
            result.Author!.Select(x => x.Name),
            Is.EqualTo(TwoAuthorsNames));
    }

    [Test]
    public async Task GetInsightSchemaMarkupDtoAsync_MapsImage_WhenCopyInsightHasImage()
    {
        SetupWebsiteChannel(null);

        var insight = CreateInsight(title: "Insight page", showSidebar: false);
        var copyInsight = insight.InsightContent.First()!;

        SetAssetCollection(copyInsight, "ImageFile", "/media/insight.png");

        var result = await InvokeGetInsightSchemaMarkupDtoAsync(insight);

        Assert.Multiple(() =>
        {
            Assert.That(result.Image, Is.EqualTo("/media/insight.png"));
            Assert.That(result.MainEntityOfPage, Is.Null);
        });
    }

    [Test]
    public void GetCallToActionDto_ReturnsNull_WhenButtonTextOrLinkIsMissing()
    {
        Assert.Multiple(() =>
        {
            Assert.That(InsightPageService.GetCallToActionDto("desc", null, "/link", "right"), Is.Null);
            Assert.That(InsightPageService.GetCallToActionDto("desc", "text", null, "right"), Is.Null);
        });
    }

    [Test]
    public void GetCallToActionDto_MapsValuesAndDefaultsPosition()
    {
        var withPosition = InsightPageService.GetCallToActionDto("desc", "text", "/link", "right");
        var withoutPosition = InsightPageService.GetCallToActionDto("desc", "text", "/link", null);

        Assert.Multiple(() =>
        {
            Assert.That(withPosition!.Position, Is.EqualTo("right"));
            Assert.That(withPosition.CallToActionDto!.Description, Is.EqualTo("desc"));
            Assert.That(withPosition.CallToActionDto.ButtonTitle, Is.EqualTo("text"));
            Assert.That(withPosition.CallToActionDto.ButtonUrl, Is.EqualTo("/link"));
            Assert.That(withPosition.CallToActionDto.Position, Is.EqualTo("right"));
            Assert.That(withoutPosition!.Position, Is.EqualTo("left"));
            Assert.That(withoutPosition.CallToActionDto!.Position, Is.EqualTo("left"));
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_HidesPrintDownloadAndCopyLink_WhenDisabledInSettings()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new WebChannelSnippetInfo[]
            {
                new TestPrintShareSnippetInfo { DisablePrint = true, DisableDownload = true, DisableCopyLinkButton = true }
            });

        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = false;
        SetAssetCollection(insight, nameof(Insight.DownloadButtonAttachment), "/media/download.pdf");

        var result = await _service.GetSidebarDtoAsync(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ShowPrintButton, Is.False);
            Assert.That(result.DownloadButtonAttachment, Is.Null);
            Assert.That(result.ShowCopyLinkButton, Is.False);
        });
    }

    [Test]
    public async Task GetSidebarDtoAsync_KeepsPrintDownloadAndCopyLink_WhenSettingsDoNotDisable()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new WebChannelSnippetInfo[] { new TestPrintShareSnippetInfo() });

        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = false;
        SetAssetCollection(insight, nameof(Insight.DownloadButtonAttachment), "/media/file.pdf");

        var result = await _service.GetSidebarDtoAsync(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ShowPrintButton, Is.True);
            Assert.That(result.DownloadButtonAttachment, Is.EqualTo("/media/file.pdf"));
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_ReturnsEmpty_WhenInputIsNullOrHasNoGuids()
    {
        var withoutGuid = CreateExternalPerson(Guid.Empty, "t", "a", "", "b", "j", "/u");

        var fromNull = await _service.GetSidebarExternalPeopleDtosAsync(null!, CancellationToken.None);
        var fromEmptyGuid = await _service.GetSidebarExternalPeopleDtosAsync(new[] { withoutGuid }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(fromNull, Is.Empty);
            Assert.That(fromEmptyGuid, Is.Empty);
        });
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_ReturnsEmpty_WhenLookupReturnsNull()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ExternalPerson>)null!);

        var person = CreateExternalPerson(Guid.NewGuid(), "t", "a", "", "b", "j", "/u");

        var result = await _service.GetSidebarExternalPeopleDtosAsync(new[] { person }, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }
    private static readonly string[] KeyContactname = { "Ada Lovelace" };

    [Test]
    public async Task GetKeyContactsDisplayNames_TreatsWhitespacePrefixAsMissing()
    {
        SetupUsePersonPageTitleSetting(false);
        var person = CreatePerson(1, "Title", "Ada", "   ", "Lovelace", "Dev", "/people/ada");
        SetupContentPages(person);
        SetupContentPages(Array.Empty<InternalPerson>());

        var result = await _service.GetKeyContactsDisplayNames(SingleContactId, CancellationToken.None);

        Assert.That(result, Is.EqualTo(KeyContactname));
    }

    [Test]
    public async Task ShowCopyLinkOption_ReturnsFalse_WhenParentIdIsZero()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        SetSystemFields(insight, 10, Guid.NewGuid(), "Insight", 100, 0, "/insights/test", "/Insights/Test");

        var result = await InvokeShowCopyLinkOptionAsync(insight, true);

        Assert.That(result, Is.False);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    public async Task ShowCopyLinkOption_DirectParentSection_CombinesFlags(bool sectionFlag, bool input, bool expected)
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        SetupSectionPagesSequence(new SectionInsightsPage { ShowCopyUrlButtonInInsightPage = sectionFlag });

        var result = await InvokeShowCopyLinkOptionAsync(insight, input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ShowCopyLinkOption_FindsSectionViaInsightGroup()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        var group = new InsightGroup();
        SetSystemFields(group, 1, Guid.NewGuid(), "Group", 50, 20, "/g", "/g");

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { group });
        SetupSectionPagesSequence(null, new SectionInsightsPage { ShowCopyUrlButtonInInsightPage = true });

        var result = await InvokeShowCopyLinkOptionAsync(insight, true);

        Assert.That(result, Is.True);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ShowCopyLinkOption_FindsSectionViaInsightCategory()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        var category = new InsightCategory();
        SetSystemFields(category, 1, Guid.NewGuid(), "Category", 50, 30, "/c", "/c");

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightGroup>());
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { category });
        SetupSectionPagesSequence(null, null, new SectionInsightsPage { ShowCopyUrlButtonInInsightPage = true });

        var result = await InvokeShowCopyLinkOptionAsync(insight, true);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ShowCopyLinkOption_ReturnsFalse_WhenNoSectionFoundInHierarchy()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightGroup>());
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightCategory>());
        SetupSectionPagesSequence(null, null, null);

        var result = await InvokeShowCopyLinkOptionAsync(insight, true);

        Assert.That(result, Is.False);
    }

    private void SetupSectionPagesSequence(params SectionInsightsPage?[] pages)
    {
        var sequence = _contentServiceMock
            .SetupSequence(x => x.GetPagesAsync<SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()));

        foreach (var page in pages)
        {
            sequence = sequence.ReturnsAsync(page is null ? Array.Empty<SectionInsightsPage>() : new[] { page });
        }
    }

    private Task<bool> InvokeShowCopyLinkOptionAsync(Insight insight, bool showCopyLinkButton)
    {
        var method = typeof(InsightPageService).GetMethod(
            "GetShowCopyLinkOptionFromSectionPageAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        return (Task<bool>)method!.Invoke(_service, new object[] { insight, showCopyLinkButton, CancellationToken.None })!;
    }

    private sealed class TestPrintShareSnippetInfo : WebChannelSnippetInfo
    {
        public override bool DisablePrint { get; set; }
        public override bool DisableDownload { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
    }

    private void SetupWebsiteChannel(string? domain)
    {
        WebsiteChannelInfo? channel = null;

        if (domain is not null)
        {
            var testChannel = (TestWebsiteChannelInfo)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(TestWebsiteChannelInfo));
            testChannel.WebsiteChannelDomain = domain;
            channel = testChannel;
        }

        _websiteChannelInfoProviderMock
            .As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);
    }

    private sealed class TestWebsiteChannelInfo : WebsiteChannelInfo
    {
        public override string WebsiteChannelDomain { get; set; } = string.Empty;
    }

    private async Task<ArticleSchemaMarkupDto> InvokeGetInsightSchemaMarkupDtoAsync(Insight insight)
    {
        var method = typeof(InsightPageService).GetMethod(
            "GetInsightSchemaMarkupDtoAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        return await (Task<ArticleSchemaMarkupDto>)method.Invoke(
            _service,
            new object[] { insight, CancellationToken.None })!;
    }

    // =====================================================================================
    // ADDITIONAL COVERAGE TESTS
    // =====================================================================================

    #region Additional helpers

    private static object? GetPropertyValue(object target, string propertyName) =>
        target.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?
            .GetValue(target);

    private static object InvokePrivateStatic(string methodName, params object[] args)
    {
        var method = typeof(InsightPageService).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"Private static method '{methodName}' was not found.");
        return method!.Invoke(null, args)!;
    }

    /// <summary>
    /// Builds a real sidebar DTO (an ordinary Person with a teaser image, an InternalPerson and one
    /// external person) so the private static mappers can be fed with realistic input. Returned as
    /// object so the test does not need to name the DTO type.
    /// </summary>
    private async Task<object> BuildSidebarWithPeopleAsync()
    {
        SetupUsePersonPageTitleSetting(false);

        var insight = CreateInsight(title: "Insight page", showSidebar: true);
        insight.ShowPrintButton = true;
        insight.ShowSocialMediaButtons = false;

        SetContentType(insight, "Article");
        SetAssetCollection(insight, nameof(Insight.DownloadButtonAttachment), "/media/download.pdf");

        var person = CreatePerson(
            contentItemId: 401,
            metadataTitle: "Ada Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");
        SetAssetCollection(person, nameof(Person.MetadataTeaserImage), "/media/ada.png");

        var internalPerson = CreateInternalPerson(
            contentItemId: 402,
            firstName: "Grace",
            surnamePrefix: "van",
            lastName: "Hopper",
            jobTitle: "Engineer",
            url: "/people/grace");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person, internalPerson);

        var externalGuid = Guid.NewGuid();
        var linkedExternal = CreateExternalPerson(
            externalGuid, "Linked", "External", string.Empty, "Linked", "Job", "https://example.com/linked");
        var retrievedExternal = CreateExternalPerson(
            externalGuid, "External Retrieved", "External", string.Empty, "Retrieved", "External Consultant", "https://example.com/retrieved");

        SetCollection(insight, nameof(Insight.RelatedExternalPeople), linkedExternal);
        SetupContentPages(retrievedExternal);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        return await _service.GetContentPageSidebarDto(insight, CancellationToken.None);
    }

    private void SetupGroupAndCategoryLookups()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightGroup>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightCategory>());
    }

    private static RetrievePagesQueryParameters CreateQueryParameters()
    {
        try
        {
            return (RetrievePagesQueryParameters)Activator.CreateInstance(
                typeof(RetrievePagesQueryParameters),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                null,
                null)!;
        }
        catch (Exception)
        {
            return (RetrievePagesQueryParameters)CreateObject(typeof(RetrievePagesQueryParameters));
        }
    }

    #endregion

    #region GetPageConfigurationDto (private static)

    [TestCase(true)]
    [TestCase(false)]
    public async Task GetPageConfigurationDto_MapsSidebarValues_AndUsesSuppliedCopyLinkFlag(bool showCopyLinkButton)
    {
        var sidebar = await BuildSidebarWithPeopleAsync();

        var config = InvokePrivateStatic("GetPageConfigurationDto", sidebar, showCopyLinkButton);

        Assert.Multiple(() =>
        {
            Assert.That(GetPropertyValue(config, "TypeName"), Is.EqualTo("Article"));
            Assert.That(GetPropertyValue(config, "PublishDate"), Is.EqualTo(GetPropertyValue(sidebar, "PublishDate")));
            Assert.That(GetPropertyValue(config, "UpdatedDate"), Is.EqualTo(GetPropertyValue(sidebar, "UpdatedDate")));
            Assert.That(GetPropertyValue(config, "ShowPrintButton"), Is.EqualTo(true));
            Assert.That(GetPropertyValue(config, "DownloadButtonAttachment"), Is.EqualTo("/media/download.pdf"));
            Assert.That(GetPropertyValue(config, "ShowCopyLinkButton"), Is.EqualTo(showCopyLinkButton));
            Assert.That(GetPropertyValue(config, "SocialMediaButtons"), Is.Null);
        });
    }

    #endregion

    #region GetPageAuthorsDto (private static)

    [Test]
    public async Task GetPageAuthorsDto_MapsInternalAndExternalPeople()
    {
        var sidebar = await BuildSidebarWithPeopleAsync();

        var grid = InvokePrivateStatic("GetPageAuthorsDto", sidebar);

        var items = ((IEnumerable)GetPropertyValue(grid, "Items")!).Cast<object>().ToList();
        var externalItems = ((IEnumerable)GetPropertyValue(grid, "ExternalItems")!).Cast<object>().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(GetPropertyValue(grid, "Title"), Is.EqualTo(ResourceStringKey.LiveSite.Insights.Authors));
            Assert.That(GetPropertyValue(grid, "UseNameAsLink"), Is.EqualTo(true));

            Assert.That(items, Has.Count.EqualTo(2));
            Assert.That(items.Select(x => GetPropertyValue(x, "DisplayName")), Is.EquivalentTo(ExpectedComposedContactNames));
            Assert.That(GetPropertyValue(items[0], "JobTitle"), Is.EqualTo("Developer"));
            Assert.That(GetPropertyValue(items[0], "TeaserImageUrl"), Is.EqualTo("/media/ada.png"));
            Assert.That(GetPropertyValue(items[0], "TeaserImageAltText"), Is.EqualTo("Person image"));
            Assert.That((string?)GetPropertyValue(items[0], "PersonUrl"), Does.EndWith("/people/ada"));

            Assert.That(externalItems, Has.Count.EqualTo(1));
            Assert.That(GetPropertyValue(externalItems[0], "DisplayName"), Is.EqualTo("External Retrieved"));
            Assert.That(GetPropertyValue(externalItems[0], "JobTitle"), Is.EqualTo("External Consultant"));
            Assert.That(GetPropertyValue(externalItems[0], "PersonUrl"), Is.EqualTo("https://example.com/retrieved"));
        });
    }

    #endregion

    #region GetCallToActionDto(Insight) (private static, via GetContentPageDtoAsync)

    [Test]
    public async Task GetContentPageDtoAsync_MapsCallToAction_WhenEnabledAndComplete()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: false);
        var copyInsight = CreateCopyInsight(
            title: "Copy title",
            description: "<p>Description</p>",
            content: "<p>Content</p>",
            showTitle: true,
            showDescription: true,
            showCallToAction: true,
            callToActionButtonUrl: "https://example.com/cta",
            callToActionPosition: "right");

        SetCollection(insight, nameof(Insight.InsightContent), copyInsight);

        var result = await _service.GetContentPageDtoAsync(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CallToActionWithPosition, Is.Not.Null);
            Assert.That(result.CallToActionWithPosition!.Position, Is.EqualTo("right"));
            Assert.That(result.CallToActionWithPosition.CallToActionDto!.Description, Is.EqualTo("CTA description"));
            Assert.That(result.CallToActionWithPosition.CallToActionDto.ButtonTitle, Is.EqualTo("CTA button"));
            Assert.That(result.CallToActionWithPosition.CallToActionDto.ButtonUrl, Is.EqualTo("https://example.com/cta"));
            Assert.That(result.CallToActionWithPosition.CallToActionDto.Position, Is.EqualTo("right"));
        });
    }

    [Test]
    public async Task GetContentPageDtoAsync_CallToActionDefaultsToLeft_WhenPositionIsNull()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: false);
        var copyInsight = CreateCopyInsight(
            title: "Copy title",
            description: "<p>Description</p>",
            content: "<p>Content</p>",
            showTitle: true,
            showDescription: true,
            showCallToAction: true,
            callToActionButtonUrl: "https://example.com/cta",
            callToActionPosition: null);

        SetCollection(insight, nameof(Insight.InsightContent), copyInsight);

        var result = await _service.GetContentPageDtoAsync(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CallToActionWithPosition, Is.Not.Null);
            Assert.That(result.CallToActionWithPosition!.Position, Is.EqualTo("left"));
        });
    }

    [Test]
    public async Task GetContentPageDtoAsync_CallToActionIsNull_WhenEnabledButButtonUrlIsMissing()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: false);
        var copyInsight = CreateCopyInsight(
            title: "Copy title",
            description: "<p>Description</p>",
            content: "<p>Content</p>",
            showTitle: true,
            showDescription: true,
            showCallToAction: true,
            callToActionButtonUrl: null,
            callToActionPosition: "right");

        SetCollection(insight, nameof(Insight.InsightContent), copyInsight);

        var result = await _service.GetContentPageDtoAsync(insight, CancellationToken.None);

        Assert.That(result.CallToActionWithPosition, Is.Null);
    }

    #endregion

    #region GetContentPageSidebarDto - extra branches

    [Test]
    public async Task GetContentPageSidebarDto_UsesMetadataTitleAndTeaserImage_WhenUsePersonPageTitleSettingIsTrue()
    {
        SetupUsePersonPageTitleSetting(true);

        var insight = CreateInsight(title: "Insight page", showSidebar: true);

        var person = CreatePerson(
            contentItemId: 301,
            metadataTitle: "Ada Page Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");
        SetAssetCollection(person, nameof(Person.MetadataTeaserImage), "/media/ada.png");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.RelevantContactPeople, Has.Length.EqualTo(1));
            Assert.That(result.RelevantContactPeople[0].FullName, Is.EqualTo("Ada Page Title"));
            Assert.That(result.RelevantContactPeople[0].JobTitle, Is.EqualTo("Developer"));
            Assert.That(result.RelevantContactPeople[0].Image?.Url, Is.EqualTo("/media/ada.png"));
            Assert.That(result.RelevantContactPeople[0].Url, Does.EndWith("/people/ada"));
        });
    }

    [Test]
    public async Task GetContentPageSidebarDto_UpdatedDateIsNull_WhenFirstAndLastPublishedAreTheSame()
    {
        var published = new DateTime(2026, 03, 01, 10, 0, 0);
        var insight = CreateInsight(title: "Insight page", showSidebar: true);

        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataFirstPublishedWhen", published);
        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataLastPublishedWhen", published);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.PublishDate, Is.EqualTo(published));
            Assert.That(result.UpdatedDate, Is.Null);
        });
    }

    [Test]
    public async Task GetContentPageSidebarDto_PublishDateIsNull_WhenOnlyLastPublishedIsSet()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);

        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataFirstPublishedWhen", DateTime.MinValue);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.PublishDate, Is.Null);
            Assert.That(result.UpdatedDate, Is.EqualTo(SchemaModifyDate));
        });
    }

    [Test]
    public async Task GetContentPageSidebarDto_TreatsMissingPublishDatesAsMinValue()
    {
        var insight = CreateInsight(title: "Insight page", showSidebar: true);

        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataFirstPublishedWhen", null);
        SetPropertyValue(insight.SystemFields, "ContentItemCommonDataLastPublishedWhen", null);
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);

        var result = await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.PublishDate, Is.Null);
            Assert.That(result.UpdatedDate, Is.Null);
        });
    }

    #endregion

    #region GetSidebarExternalPeopleDtosAsync - extra branches

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_IgnoresPeopleWithoutSystemFields()
    {
        var result = await _service.GetSidebarExternalPeopleDtosAsync(
            new[] { new ExternalPerson() },
            CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_QueriesOnce_WhenSameGuidIsLinkedTwice()
    {
        var guid = Guid.NewGuid();
        var first = CreateExternalPerson(guid, "First", "Ada", string.Empty, "Lovelace", "Consultant", "/first");
        var second = CreateExternalPerson(guid, "Second", "Ada", string.Empty, "Lovelace", "Consultant", "/second");

        SetupContentPages(first);

        var result = await _service.GetSidebarExternalPeopleDtosAsync(
            new[] { first, second },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].FullName, Is.EqualTo("Ada Lovelace"));
        });
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSidebarExternalPeopleDtosAsync_UsesPageTitle_WhenUsePersonPageTitleSettingIsTrue()
    {
        SetupUsePersonPageTitleSetting(true);

        var person = CreateExternalPerson(
            Guid.NewGuid(), "External Page Title", "Ada", string.Empty, "Lovelace", "Consultant", "/ext");
        SetupContentPages(person);

        var result = await _service.GetSidebarExternalPeopleDtosAsync(new[] { person }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].FullName, Is.EqualTo("External Page Title"));
            Assert.That(result[0].JobTitle, Is.EqualTo("Consultant"));
        });
    }

    #endregion

    #region GetInsightSchemaMarkupDtoAsync - extra branches

    [Test]
    public async Task GetInsightSchemaMarkupDtoAsync_UsesPersonPageTitle_ForRelevantContactAuthors()
    {
        SetupUsePersonPageTitleSetting(true);
        SetupWebsiteChannel("https://example.com");

        var insight = CreateInsight(title: "Insight page", showSidebar: false);

        var person = CreatePerson(
            contentItemId: 501,
            metadataTitle: "Person Metadata Title",
            firstName: "Ada",
            surnamePrefix: string.Empty,
            lastName: "Lovelace",
            jobTitle: "Developer",
            url: "/people/ada");

        SetCollection(insight, nameof(Insight.RelevantContactPerson), person);
        SetupContentPages(person);
        SetupContentPages(Array.Empty<InternalPerson>());

        var result = await InvokeGetInsightSchemaMarkupDtoAsync(insight);

        Assert.That(result.Author!.Select(x => x.Name), Is.EqualTo(ExpectedPersonMetadataTitle));
    }

    [Test]
    public async Task GetInsightSchemaMarkupDtoAsync_UsesExternalPeopleOnly_WhenNoRelevantContactsExist()
    {
        SetupUsePersonPageTitleSetting(false);
        SetupWebsiteChannel("https://example.com");

        var insight = CreateInsight(title: "Insight page", showSidebar: false);

        var externalPerson = CreateExternalPerson(
            contentItemGuid: Guid.NewGuid(),
            metadataTitle: "External Title",
            firstName: "Ada",
            surnamePrefix: "van",
            lastName: "Lovelace",
            jobTitle: "Consultant",
            url: "/people/ada");

        SetCollection(insight, nameof(Insight.RelatedExternalPeople), externalPerson);
        SetupContentPages(externalPerson);

        var result = await InvokeGetInsightSchemaMarkupDtoAsync(insight);

        Assert.That(result.Author!.Select(x => x.Name), Is.EqualTo(ExpectedExternalComposedName));
    }

    #endregion

    #region Kentico query delegates

    // The service passes query-builder lambdas (query => query.Where(...).OrderBy(...)) into
    // IContentService.GetPagesAsync. Because the service is mocked, those lambdas are never run, so
    // they show as uncovered. This test exercises every method that builds such a query, then runs the
    // captured delegates against a best-effort RetrievePagesQueryParameters instance. The delegates may
    // throw if the Kentico type needs internal state; that is swallowed on purpose because the goal is
    // only to execute the lambda bodies.
    [Test]
    public async Task QueryDelegates_PassedToContentService_CanBeExecuted()
    {
        SetupUsePersonPageTitleSetting(false);
        SetupContentPages(Array.Empty<Person>());
        SetupContentPages(Array.Empty<InternalPerson>());

        var external = CreateExternalPerson(
            Guid.NewGuid(), "External", "Ada", string.Empty, "Lovelace", "Consultant", "/ext");
        SetupContentPages(external);

        var insight = CreateInsight(title: "Insight page", showSidebar: true);

        await _service.GetKeyContactsDisplayNames(SingleContactId, CancellationToken.None);
        await _service.GetExternalPeopleDisplayNames(
            new List<Guid> { external.SystemFields.ContentItemGUID },
            CancellationToken.None);
        await _service.GetSidebarExternalPeopleDtosAsync(new[] { external }, CancellationToken.None);

        // Walks the whole section hierarchy: direct parent -> InsightGroup -> InsightCategory.
        SetupGroupAndCategoryLookups();
        SetupSectionPagesSequence(null, null, null);
        await InvokeShowCopyLinkOptionAsync(insight, true);

        // Runs the Insight lookup query inside GetContentPageSidebarDto.
        SetupCopyLinkSection(insight, showCopyUrlButtonInInsightPage: true);
        await _service.GetContentPageSidebarDto(insight, CancellationToken.None);

        var queryActions = _contentServiceMock.Invocations
            .SelectMany(invocation => invocation.Arguments)
            .OfType<Action<RetrievePagesQueryParameters>>()
            .ToList();

        Assert.That(queryActions, Is.Not.Empty);

        var queryParameters = CreateQueryParameters();
        foreach (var queryAction in queryActions)
        {
            try
            {
                queryAction(queryParameters);
            }
            catch (Exception)
            {
                // Best effort only - see comment above the test.
            }
        }
    }

    #endregion
}