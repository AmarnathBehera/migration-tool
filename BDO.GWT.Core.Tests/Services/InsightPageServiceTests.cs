using System.Reflection;
using System.Runtime.Serialization;
using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
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

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelName)
            .Returns("BDO Test");

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(default(IEnumerable<PrintorShareInfo>)!);

        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync(default(IEnumerable<OurPeopleInfo>)!);

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
            Assert.That(result.ShowCopyLinkButton, Is.False);
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

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain(nameof(Insight)));
    }

   
    private void SetupUsePersonPageTitleSetting(bool usePageTitle)
    {
        var setting = new TestOurPeopleInfo
        {
            UsePageTitleToDisplayPersonName = usePageTitle
        };

        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync(new[] { setting });
    }

    private sealed class TestOurPeopleInfo : OurPeopleInfo
    {
        public override bool UsePageTitleToDisplayPersonName { get; set; }
    }

    private sealed class TestPrintorShareInfo : PrintorShareInfo
    {
        public override bool DisableDownload { get; set; }
        public override bool DisablePrint { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
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

        SetCollection(insight, nameof(Insight.ContentType), Array.Empty<object>());
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
        var property = typeof(Insight).GetProperty(nameof(Insight.ContentType));
        if (property is null)
        {
            throw new InvalidOperationException("ContentType property was not found.");
        }

        var itemType = GetEnumerableElementType(property.PropertyType);
        var item = CreateObject(itemType);

        SetPropertyValue(item, "Name", name);

        SetCollection(insight, nameof(Insight.ContentType), item);
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
    public async Task GetSidebarDtoAsync_HidesPrintDownloadAndCopy_WhenSettingsDisableFlagsAreTrue()
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

        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[]
            {
                new TestPrintorShareInfo
                {
                    DisablePrint = true,
                    DisableDownload = true,
                    DisableCopyLinkButton = true,
                }
            });

        var result = await _service.GetSidebarDtoAsync(insight, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ShowPrintButton, Is.False);
            Assert.That(result.DownloadButtonAttachment, Is.Null);
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Position, Is.EqualTo("left"));
        Assert.That(result.CallToActionDto.ButtonTitle, Is.EqualTo("Read more"));
        Assert.That(result.CallToActionDto.ButtonUrl, Is.EqualTo("https://example.com"));
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

        Assert.That(result.SocialMediaButtons, Is.Not.Null);
        Assert.That(result.SocialMediaButtons!.ShowCopyLinkButton, Is.Null);
    }
}