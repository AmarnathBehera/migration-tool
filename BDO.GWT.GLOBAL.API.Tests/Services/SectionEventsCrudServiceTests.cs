using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class SectionEventsCrudServiceTests
{
    private static readonly string[] ExpectedTaxAuditFilters = ["Tax", "Audit"];
    private static readonly string[] ExpectedTaxAuditAdvisoryFilters = ["Tax", "Audit", "Advisory"];
    private static readonly LanguageTag[] GbAndUsCultures = [LanguageTag.GB, LanguageTag.US];
    private static SectionEventsCrudService CreateServiceWithoutConstructor() =>
        (SectionEventsCrudService)RuntimeHelpers.GetUninitializedObject(typeof(SectionEventsCrudService));

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void SectionEventsCrudService_ImplementsISectionEventsCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<ISectionEventsCrudService>());
    }

    [Test]
    public void SectionEventsPageContent_DefaultValues_AreInitialized()
    {
        var content = new SectionEventsPageContent();

        Assert.Multiple(() =>
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.EventsWebinarsListFilters, Is.Empty);
            Assert.That(content.EventsWebinarsListFeaturedPages, Is.Empty);
        });
    }

    [Test]
    public void SectionEventsPageContent_StringProperties_CanBeSet()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "Events Title",
            MetadataTeaserImageAltText = "Alt",
            HeroLayout = "default",
            MetadataDescription = "Desc",
            MetadataTeaserImage = "img.jpg",
            MetadataCanonical = "https://example.com",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Desc",
            HeroBackgroundImage = "hero.jpg",
            HeroBackgroundImageAltText = "hero alt",
            HeroBackgroundImage_Mobile = "hero-m.jpg",
            HeroBackgroundImage_MobileAltText = "hero mobile alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Click me",
            HeroCallToActionButtonUrl = "https://cta.com",
            EventsWebinarsListTitle = "List Title",
            EventsWebinarsListDescription = "List Desc",
            EventsWebinarsListAdvancedSearchDescription = "Search Desc",
            EventsWebinarsListAdvancedSearchLinkText = "Search Link",
            EventsWebinarsListAdvancedSearchLinkUrl = "https://search.com",
            MetadataOGTitle = "OG Title",
            MetadataOGDescription = "OG Desc",
            MetadataOGImage = "og.jpg",
            MetadataOGTwitterImage = "tw.jpg",
            RedirectOnUnpublish = "/redirect"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Events Title"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("Alt"));
            Assert.That(content.HeroLayout, Is.EqualTo("default"));
            Assert.That(content.MetadataDescription, Is.EqualTo("Desc"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("img.jpg"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com"));
            Assert.That(content.HeroTitle, Is.EqualTo("Hero Title"));
            Assert.That(content.HeroDescription, Is.EqualTo("Hero Desc"));
            Assert.That(content.HeroBackgroundImage, Is.EqualTo("hero.jpg"));
            Assert.That(content.HeroBackgroundImageAltText, Is.EqualTo("hero alt"));
            Assert.That(content.HeroBackgroundImage_Mobile, Is.EqualTo("hero-m.jpg"));
            Assert.That(content.HeroBackgroundImage_MobileAltText, Is.EqualTo("hero mobile alt"));
            Assert.That(content.HeroCallToActionButtonType, Is.EqualTo("primary"));
            Assert.That(content.HeroCallToActionButtonText, Is.EqualTo("Click me"));
            Assert.That(content.HeroCallToActionButtonUrl, Is.EqualTo("https://cta.com"));
            Assert.That(content.EventsWebinarsListTitle, Is.EqualTo("List Title"));
            Assert.That(content.EventsWebinarsListDescription, Is.EqualTo("List Desc"));
            Assert.That(content.EventsWebinarsListAdvancedSearchDescription, Is.EqualTo("Search Desc"));
            Assert.That(content.EventsWebinarsListAdvancedSearchLinkText, Is.EqualTo("Search Link"));
            Assert.That(content.EventsWebinarsListAdvancedSearchLinkUrl, Is.EqualTo("https://search.com"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("OG Title"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("OG Desc"));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.jpg"));
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("tw.jpg"));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/redirect"));
        });
    }

    [Test]
    public void SectionEventsPageContent_BoolProperties_CanBeSet()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            EventsWebinarsListFilteringEnabled = true,
            ShowPastEvents = true,
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataNofollow = true,
            MetadataNoindex = true,
            IncludeInSitemap = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EventsWebinarsListFilteringEnabled, Is.True);
            Assert.That(content.ShowPastEvents, Is.True);
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataNofollow, Is.True);
            Assert.That(content.MetadataNoindex, Is.True);
            Assert.That(content.IncludeInSitemap, Is.True);
        });
    }

    [Test]
    public void SectionEventsPageContent_CollectionProperties_CanBePopulated()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            EventsWebinarsListFilters = ["filter1", "filter2"],
            EventsWebinarsListFeaturedPages = ["page1"]
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EventsWebinarsListFilters, Has.Count.EqualTo(2));
            Assert.That(content.EventsWebinarsListFeaturedPages, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void SectionEventsPageContent_DateProperties_CanBeSet()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            PublishFrom = from,
            PublishTo = to
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PublishFrom, Is.EqualTo(from));
            Assert.That(content.PublishTo, Is.EqualTo(to));
        });
    }

    [Test]
    public void PageOutput_SectionEventsPageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new SectionEventsPageContent();
        var output = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = guid },
            Content = content
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.General.PageGuid, Is.EqualTo(guid));
            Assert.That(output.Content, Is.SameAs(content));
        });
    }

    [Test]
    public async Task GetPagesAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public void DeletePageAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public void DeletePageAsync_WithDeleteAllCultures_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None));
    }

    [Test]
    public void SectionEventsCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(ISectionEventsCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
        });
    }
}

[TestFixture]
public class SectionEventsCrudServiceBranchTests
{
    private static readonly string[] ExpectedTaxAuditFilters = ["Tax", "Audit"];
    private static readonly string[] ExpectedTaxAuditAdvisoryFilters = ["Tax", "Audit", "Advisory"];
    private static readonly LanguageTag[] GbAndUsCultures = [LanguageTag.GB, LanguageTag.US];

    private Mock<IContentService> _contentRetrieverMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IInfoByIdProvider<ChannelInfo>> _channelByIdProviderMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;

    private static LanguageTag EmptyLocaleCulture => new("en", null, "GB", "");
    private static LanguageTag EmptyLanguageCulture => new("", null, "GB", "en-GB");

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentService>();
        _validationStateMock = new Mock<IValidationState>();
        _cultureServiceMock = new Mock<ICultureService>();
        _pageContentValidatorMock = new Mock<IPageContentValidator>();
        _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _channelByIdProviderMock = _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>();
        _webPageManagerMock = new Mock<IWebPageManager>();
        _mappingHelperMock = new Mock<IMappingHelper>();
    }

    private SectionEventsCrudService CreateService()
    {
        var service = (SectionEventsCrudService)RuntimeHelpers.GetUninitializedObject(typeof(SectionEventsCrudService));

        SetPrivateField(service, "_contentRetriever", _contentRetrieverMock.Object);
        SetPrivateField(service, "_validationState", _validationStateMock.Object);
        SetPrivateField(service, "_cultureService", _cultureServiceMock.Object);
        SetPrivateField(service, "_pageContentValidator", _pageContentValidatorMock.Object);
        SetPrivateField(service, "_channelInfoProvider", _channelInfoProviderMock.Object);
        SetPrivateField(service, "_webPageManager", _webPageManagerMock.Object);
        SetPrivateField(service, "_mappingHelper", _mappingHelperMock.Object);

        return service;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            Assert.Fail($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        }

        field!.SetValue(target, value);
    }

    private void SetupCulture(LanguageTag? culture) =>
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

    private void SetupCultureSequence(LanguageTag first, LanguageTag second) =>
        _mappingHelperMock
            .SetupSequence(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(first)
            .ReturnsAsync(second);

    private void SetupChannel(ChannelInfo? channel) =>
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);

    private void SetupChannelThrows(Exception exception) =>
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ThrowsAsync(exception);

    private static SectionEventsPageContent CreateRequest() => new()
    {
        MetadataTitle = "Events",
        MetadataTeaserImageAltText = "alt",
    };

    private static FakeChannelInfo CreateChannelInfo(string channelName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelName = channelName;
        return channel;
    }

    private static SectionEvents CreateSectionEvents() => new()
    {
        MetadataTitle = "Events",
        MetadataDescription = "Desc",
        MetadataTeaserImageAltText = "alt",
        HeroLayout = "default",
        EventsWebinarsListFilters = string.Empty,
        EventsWebinarsListFeaturedPages = [],
        SystemFields = new WebPageFields
        {
            WebPageItemID = 42,
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "events",
            WebPageUrlPath = "/events",
            WebPageItemTreePath = "/events",
            WebPageItemParentID = 7,
            ContentItemID = 100,
            ContentItemGUID = Guid.NewGuid(),
            WebPageUrlPathContentLanguageID = 1,
            ContentItemCommonDataFirstPublishedWhen = new DateTime(2026, 1, 1),
            ContentItemCommonDataLastPublishedWhen = new DateTime(2026, 2, 1)
        }
    };

    private void SetupEmptyMappingHelperOutputs()
    {
        _mappingHelperMock.Setup(x => x.BuildFeaturedEventOrWebinarReferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _mappingHelperMock.Setup(x => x.BuildContentAttachmentImageReferences(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    private void SetupSectionEvents(IEnumerable<SectionEvents>? events) =>
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events!);

    private static void AssertNotFoundWithMessage<T>(ActionResult<T> result, string expectedMessage)
    {
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo(expectedMessage));
        });
    }

    private static void Assert500WithMessage<T>(ActionResult<T> result, string expectedMessage)
    {
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(objectResult.Value, Is.EqualTo(expectedMessage));
        });
    }

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private string? channelName;

        public override string ChannelName
        {
            get => channelName ?? string.Empty;
            set => channelName = value;
        }
    }

    #region GetPagesAsync

    [Test]
    public async Task GetPagesAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task GetPagesAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task GetPagesAsync_NullCultureFromValidator_FallsBackToDefault_ThenChannelNotFound()
    {
        SetupCulture(null);
        SetupChannel(null);

        var result = await CreateService().GetPagesAsync(1, null, null, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
        _mappingHelperMock.Verify(
            x => x.ValidateAndNormalizeSiteCultureAsync(1, LanguageTag.GB.Language, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetPagesAsync_UnprocessableEntityThrown_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task GetPagesAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new InvalidOperationException("boom"));

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert500WithMessage(result, "boom");
    }

    [Test]
    public async Task GetPagesAsync_EmptyResult_ReturnsEmptyCollection()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetPagesAsync_OnlyPublishedFalse_UsesPreviewQuery()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        await CreateService().GetPagesAsync(1, "en-gb", false, CancellationToken.None);

        _contentRetrieverMock.Verify(x => x.GetPagesAsync<SectionEvents>(
            It.Is<RetrievePagesParameters>(p => p.IsForPreview == true),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPageAsync

    [Test]
    public async Task GetPageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task GetPageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), null, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task GetPageAsync_UnprocessableEntityThrown_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task GetPageAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new InvalidOperationException("fail"));

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert500WithMessage(result, "fail");
    }

    [Test]
    public async Task GetPageAsync_PageNotFound_ReturnsNotFoundResult()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    #endregion

    #region CreatePageAsync

    [Test]
    public async Task CreatePageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task CreatePageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var result = await CreateService().CreatePageAsync(1, 10, null, CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
        _pageContentValidatorMock.Verify(x => x.ValidateAndNormalizePageContent(It.IsAny<SectionEventsPageContent>()), Times.Never);
    }

    [Test]
    public async Task CreatePageAsync_UnprocessableEntityThrown_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreatePageAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new InvalidOperationException("create failed"));

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert500WithMessage(result, "create failed");
        _webPageManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CreatePageAsync_ParentIdZero_SkipsParentLookup_AndReturns500FromMapping()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));

        var result = await CreateService().CreatePageAsync(1, 0, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        _pageContentValidatorMock.Verify(x => x.ValidateAndNormalizePageContent(It.IsAny<SectionEventsPageContent>()), Times.Once);
    }

    [Test]
    public async Task CreatePageAsync_ParentPageNull_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupSectionEvents((IEnumerable<SectionEvents>)null!);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreatePageAsync_FilterList_IsNormalizedBeforeValidation()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateSectionEvents()]);

        var request = CreateRequest();
        request.EventsWebinarsListFilters = ["Tax;Audit", "Audit", null!, "  Advisory "];

        await CreateService().CreatePageAsync(1, 10, "en-gb", request, CancellationToken.None);

        Assert.That(request.EventsWebinarsListFilters, Is.EquivalentTo(ExpectedTaxAuditAdvisoryFilters));
    }

    [Test]
    public async Task CreatePageAsync_UpdateDraftFails_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupEmptyMappingHelperOutputs();
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateSectionEvents()]);
        _webPageManagerMock
            .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(77);
        _webPageManagerMock
            .Setup(x => x.TryUpdateDraft(77, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Failed to update page draft");
    }

    [Test]
    public async Task CreatePageAsync_SuccessfullyCreatesAndMapsPage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupEmptyMappingHelperOutputs();
        _mappingHelperMock
            .Setup(x => x.ResolveWebPageRelatedItemPathAsync(It.IsAny<WebPageRelatedItem?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var createdPage = CreateSectionEvents();
        createdPage.SystemFields!.WebPageItemID = 77;
        createdPage.EventsWebinarsListFilters = "Tax,Audit";
        SetupSectionEvents(new List<SectionEvents> { createdPage });
        _webPageManagerMock
            .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(77);
        _webPageManagerMock
            .Setup(x => x.TryUpdateDraft(77, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _webPageManagerMock
            .Setup(x => x.TryPublish(77, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateService().CreatePageAsync(1, 0, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.General.WebPageItemID, Is.EqualTo(77));
            Assert.That(result.Value.Content.MetadataTitle, Is.EqualTo("Events"));
            Assert.That(result.Value.Content.EventsWebinarsListFilters, Is.EquivalentTo(ExpectedTaxAuditFilters));
        });
    }

    #endregion

    #region CreatePageCultureVersionAsync

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidFromCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "xx", "en-us", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid from culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidToCulture_ReturnsNotFoundWithMessage()
    {
        SetupCultureSequence(LanguageTag.GB, EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "xx", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid to culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), null, null, CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
        _mappingHelperMock.Verify(
            x => x.ValidateAndNormalizeSiteCultureAsync(1, LanguageTag.GB.Language, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_UnprocessableEntityThrown_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new InvalidOperationException("variant failed"));

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", CreateRequest(), CancellationToken.None);

        Assert500WithMessage(result, "variant failed");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_PageNotFound_ReturnsNotFoundResult()
    {
        SetupCultureSequence(LanguageTag.GB, LanguageTag.US);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_FilterList_IsNormalizedBeforeValidation()
    {
        SetupCultureSequence(LanguageTag.GB, LanguageTag.US);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupSectionEvents(new List<SectionEvents> { CreateSectionEvents() });

        var request = CreateRequest();
        request.EventsWebinarsListFilters = ["Tax;Audit", "Audit"];

        await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", request, CancellationToken.None);

        Assert.That(request.EventsWebinarsListFilters, Is.EquivalentTo(ExpectedTaxAuditFilters));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SystemFieldsMissing_ReturnsNotFoundWithMessage()
    {
        SetupCultureSequence(LanguageTag.GB, LanguageTag.US);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupSectionEvents(new List<SectionEvents> { new SectionEvents { SystemFields = null! } });

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Page system fields are missing");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_CreateVariantFails_ReturnsNotFoundWithMessage()
    {
        SetupCultureSequence(LanguageTag.GB, LanguageTag.US);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupEmptyMappingHelperOutputs();
        SetupSectionEvents(new List<SectionEvents> { CreateSectionEvents() });
        _webPageManagerMock
            .Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-us", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Failed to create language variant");
    }

    #endregion

    #region UpdatePageAsync

    [Test]
    public async Task UpdatePageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task UpdatePageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), null, CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
        _mappingHelperMock.Verify(
            x => x.ValidateAndNormalizeSiteCultureAsync(1, LanguageTag.GB.Locale, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UpdatePageAsync_UnprocessableEntityThrown_Returns422()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task UpdatePageAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannelThrows(new InvalidOperationException("update failed"));

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert500WithMessage(result, "update failed");
    }

    [Test]
    public async Task UpdatePageAsync_PageNotFound_ReturnsNotFoundResult()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdatePageAsync_SystemFieldsMissing_ReturnsNotFoundWithMessage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupSectionEvents(new List<SectionEvents> { new SectionEvents { SystemFields = null! } });

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Page system fields are missing");
    }

    [Test]
    public async Task UpdatePageAsync_SuccessfullyUpdatesAndMapsPage()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        SetupEmptyMappingHelperOutputs();
        _mappingHelperMock
            .Setup(x => x.ResolveWebPageRelatedItemPathAsync(It.IsAny<WebPageRelatedItem?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var existingPage = CreateSectionEvents();
        var updatedPage = CreateSectionEvents();
        updatedPage.MetadataTitle = "Updated Events";
        updatedPage.EventsWebinarsListFilters = "Tax,Audit";
        _contentRetrieverMock
            .SetupSequence(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionEvents> { existingPage })
            .ReturnsAsync(new List<SectionEvents> { updatedPage });
        _webPageManagerMock
            .Setup(x => x.TryUpdateDraft(existingPage.SystemFields!.WebPageItemID, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _webPageManagerMock
            .Setup(x => x.TryPublish(existingPage.SystemFields!.WebPageItemID, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateService().UpdatePageAsync(1, existingPage.SystemFields.WebPageItemGUID, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Content.MetadataTitle, Is.EqualTo("Updated Events"));
            Assert.That(result.Value.Content.EventsWebinarsListFilters, Is.EquivalentTo(ExpectedTaxAuditFilters));
        });
    }

    #endregion

    #region DeletePageAsync

    [Test]
    public void DeletePageAsync_InvalidCulture_ThrowsNotFoundException()
    {
        SetupCulture(EmptyLocaleCulture);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Invalid culture specified"));
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_ThrowsNotFoundException()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(null);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), null, true, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel not found"));
        _cultureServiceMock.Verify(
            x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _webPageManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public void DeletePageAsync_NullCultureFromValidator_UsesDefaultLocale()
    {
        SetupCulture(null);
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), null, false, CancellationToken.None));

        _mappingHelperMock.Verify(
            x => x.ValidateAndNormalizeSiteCultureAsync(1, LanguageTag.GB.Locale, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task DeletePageAsync_SingleCulture_PageNotFound_ThrowsNotFoundException()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public async Task DeletePageAsync_SingleCulture_PageExists_DeletesVariant()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateSectionEvents()]);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None);

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeletePageAsync_AllCultures_DeletesExistingVariantsOnly()
    {
        SetupCulture(LanguageTag.GB);
        SetupChannel(CreateChannelInfo("TestChannel"));
        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GbAndUsCultures);
        _contentRetrieverMock
            .SetupSequence(x => x.GetPagesAsync<SectionEvents>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateSectionEvents()])
            .ReturnsAsync(Enumerable.Empty<SectionEvents>());

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None);

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
