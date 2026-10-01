using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class InsightCrudServiceTests
{
    private static readonly List<string> s_originalIndustries = ["Finance", "Tech"];
    private static readonly List<string> s_originalServiceLines = ["Audit"];
    private static readonly List<string> s_originalInvolvedCountries = ["US", "UK"];
    private static readonly List<string> s_imageFile = ["image.png"];

    private static InsightCrudService CreateServiceWithoutConstructor() =>
        (InsightCrudService)RuntimeHelpers.GetUninitializedObject(typeof(InsightCrudService));

    // ---- Mock-injected service setup ------------------------------------------------
    // The real constructor resolves a Kentico UserInfo (an AbstractInfoBase) which triggers
    // the CMS IoC container and fails outside a running CMS runtime. So the service is created
    // without its constructor and the mocked dependencies are injected into the private fields.
    private Mock<IContentService> _contentRetrieverMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IInfoByIdProvider<ChannelInfo>> _channelByIdProviderMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;

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

    private InsightCrudService CreateService()
    {
        var service = CreateServiceWithoutConstructor();

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
        var field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            Assert.Fail($"Field '{fieldName}' was not found on {target.GetType().Name}. " +
                        "The service's private field names may have changed.");
        }

        field!.SetValue(target, value);
    }

    // A LanguageTag whose Locale/Language are populated (valid culture).
    private static LanguageTag ValidCulture => LanguageTag.GB;

    // A LanguageTag whose Locale is empty (used to trigger "Invalid culture specified").
    private static LanguageTag EmptyLocaleCulture => new("en", null, "GB", "");

    // A LanguageTag whose Language is empty (used to trigger "Invalid from/to culture specified").
    private static LanguageTag EmptyLanguageCulture => new("", null, "GB", "en-GB");

    private void SetupCulture(LanguageTag? culture) =>
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

    private void SetupChannel(ChannelInfo? channel) =>
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);

    #region GetInsightPagesAsync

    [Test]
    public async Task GetInsightPagesAsync_InvalidCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        var result = await service.GetInsightPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetInsightPagesAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.GetInsightPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region GetInsightPageAsync

    [Test]
    public async Task GetInsightPageAsync_InvalidCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        var result = await service.GetInsightPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetInsightPageAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.GetInsightPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetInsightPageAsync_NullCulture_DefaultsAndReturnsNotFound_WhenChannelMissing()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.GetInsightPageAsync(1, Guid.NewGuid(), culture: null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region CreatePageAsync

    [Test]
    public async Task CreatePageAsync_InvalidCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        var result = await service.CreatePageAsync(1, 10, "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreatePageAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.CreatePageAsync(1, 10, "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region CreatePageCultureVersionAsync

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidFromCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLanguageCulture);
        var service = CreateService();

        var result = await service.CreatePageCultureVersionAsync(
            1, Guid.NewGuid(), "en-gb", "en-us", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.CreatePageCultureVersionAsync(
            1, Guid.NewGuid(), "en-gb", "en-us", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region UpdatePageAsync

    [Test]
    public async Task UpdatePageAsync_InvalidCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        var result = await service.UpdatePageAsync(1, Guid.NewGuid(), "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdatePageAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.UpdatePageAsync(1, Guid.NewGuid(), "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region DeletePageAsync

    [Test]
    public void DeletePageAsync_InvalidCulture_ThrowsNotFoundException()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        Assert.ThrowsAsync<BDO.GWT.Common.Core.Exceptions.NotFoundException>(() =>
            service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", deleteAllCultures: false, CancellationToken.None));
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_ThrowsNotFoundException()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        Assert.ThrowsAsync<BDO.GWT.Common.Core.Exceptions.NotFoundException>(() =>
            service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", deleteAllCultures: false, CancellationToken.None));
    }

    #endregion

    #region CreateCopyInsightAsync

    [Test]
    public async Task CreateCopyInsightAsync_NullRequest_ReturnsBadRequest()
    {
        var service = CreateService();

        var result = await service.CreateCopyInsightAsync(1, "en-gb", "workspace", null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreateCopyInsightAsync_InvalidCulture_ReturnsNotFound()
    {
        SetupCulture(EmptyLocaleCulture);
        var service = CreateService();

        var result = await service.CreateCopyInsightAsync(1, "en-gb", "workspace", new CopyInsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateCopyInsightAsync_ChannelNotFound_ReturnsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);
        var service = CreateService();

        var result = await service.CreateCopyInsightAsync(1, "en-gb", "workspace", new CopyInsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
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
            pageGUID: Guid.NewGuid(),
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
            pageGUID: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void InsightCrudService_ImplementsIInsightCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IInsightCrudService>());
    }

    [Test]
    public void InsightPageContent_DefaultValues_AreInitialized()
    {
        var content = new InsightPageContent();

        Assert.Multiple(() =>
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.InsightContent, Is.Empty);
            Assert.That(content.MetadataBusinessIssues, Is.Empty);
            Assert.That(content.ContentType, Is.Empty);
            Assert.That(content.MetadataBusinessLines, Is.Empty);
            Assert.That(content.MetadataServiceAreas, Is.Empty);
            Assert.That(content.MetadataServices, Is.Empty);
            Assert.That(content.MetadataIndustryCategories, Is.Empty);
            Assert.That(content.MetadataIndustries, Is.Empty);
            Assert.That(content.MetadataSpecialtiesCategories, Is.Empty);
            Assert.That(content.MetadataSpecialtiesAreas, Is.Empty);
            Assert.That(content.MetadataSpecialtiesPages, Is.Empty);
            Assert.That(content.MetadataOfficeLocation, Is.Empty);
            Assert.That(content.MicrositeMasterPages, Is.Empty);
            Assert.That(content.MicrositePages, Is.Empty);
            Assert.That(content.MetadataRssCategory, Is.Empty);
            Assert.That(content.Insights, Is.Empty);
            Assert.That(content.RelevantContactPerson, Is.Empty);
            Assert.That(content.RelatedExternalPeople, Is.Empty);
        });
    }

    [Test]
    public void InsightPageContent_StringProperties_CanBeSet()
    {
        var content = new InsightPageContent
        {
            MetadataTitle = "Test Title",
            MetadataTeaserImageAltText = "Alt text",
            MetadataDescription = "A description",
            MetadataTeaserImage = "image.jpg",
            MetadataCanonical = "https://example.com",
            Sharingbuttons = "twitter,linkedin",
            Layoutwithoutsidebar = "wide",
            DownloadButtonAttachment = "file.pdf",
            MetadataRssImage = "rss.jpg",
            MetadataOGTitle = "OG Title",
            MetadataOGDescription = "OG Desc",
            MetadataOGImage = "og.jpg",
            MetadataOGTwitterImage = "tw.jpg",
            RedirectOnUnpublish = "/redirect",
            PageDescription = "Page desc",
            PageKeywords = "keyword1,keyword2"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Test Title"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("Alt text"));
            Assert.That(content.MetadataDescription, Is.EqualTo("A description"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("image.jpg"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com"));
            Assert.That(content.Sharingbuttons, Is.EqualTo("twitter,linkedin"));
            Assert.That(content.Layoutwithoutsidebar, Is.EqualTo("wide"));
            Assert.That(content.DownloadButtonAttachment, Is.EqualTo("file.pdf"));
            Assert.That(content.MetadataRssImage, Is.EqualTo("rss.jpg"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("OG Title"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("OG Desc"));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.jpg"));
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("tw.jpg"));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/redirect"));
            Assert.That(content.PageDescription, Is.EqualTo("Page desc"));
            Assert.That(content.PageKeywords, Is.EqualTo("keyword1,keyword2"));
        });
    }

    [Test]
    public void InsightPageContent_BoolProperties_CanBeSet()
    {
        var content = new InsightPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            ComingSoon = true,
            ShowSidebar = true,
            ShowPrintButton = true,
            ShowInsightPrintButton = true,
            ShowSocialMediaButtons = true,
            UseNarrowLayout = true,
            ShowAuthorsAtTheTop = true,
            IsSearchExcluded = true,
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataNofollow = true,
            MetadataNoindex = true,
            IncludeInSitemap = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.ComingSoon, Is.True);
            Assert.That(content.ShowSidebar, Is.True);
            Assert.That(content.ShowPrintButton, Is.True);
            Assert.That(content.ShowInsightPrintButton, Is.True);
            Assert.That(content.ShowSocialMediaButtons, Is.True);
            Assert.That(content.UseNarrowLayout, Is.True);
            Assert.That(content.ShowAuthorsAtTheTop, Is.True);
            Assert.That(content.IsSearchExcluded, Is.True);
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataNofollow, Is.True);
            Assert.That(content.MetadataNoindex, Is.True);
            Assert.That(content.IncludeInSitemap, Is.True);
        });
    }

    [Test]
    public void InsightPageContent_ListProperties_CanBePopulated()
    {
        var content = new InsightPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            InsightContent = ["item1"],
            MetadataBusinessIssues = ["bi1"],
            ContentType = ["ct1"],
            MetadataBusinessLines = ["bl1"],
            MetadataServiceAreas = ["sa1"],
            MetadataServices = ["svc1"],
            MetadataIndustryCategories = ["ic1"],
            MetadataIndustries = ["ind1"],
            MetadataSpecialtiesCategories = ["sc1"],
            MetadataSpecialtiesAreas = ["spa1"],
            MetadataSpecialtiesPages = ["sp1"],
            MetadataOfficeLocation = ["ol1"],
            MicrositeMasterPages = ["mmp1"],
            MicrositePages = ["mp1"],
            MetadataRssCategory = ["rss1"],
            Insights = ["ins1"],
            RelevantContactPerson = ["rcp1"],
            RelatedExternalPeople = ["rep1"]
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.InsightContent, Has.Count.EqualTo(1));
            Assert.That(content.MetadataBusinessIssues, Has.Count.EqualTo(1));
            Assert.That(content.ContentType, Has.Count.EqualTo(1));
            Assert.That(content.MetadataBusinessLines, Has.Count.EqualTo(1));
            Assert.That(content.MetadataServiceAreas, Has.Count.EqualTo(1));
            Assert.That(content.MetadataServices, Has.Count.EqualTo(1));
            Assert.That(content.MetadataIndustryCategories, Has.Count.EqualTo(1));
            Assert.That(content.MetadataIndustries, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesCategories, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesAreas, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesPages, Has.Count.EqualTo(1));
            Assert.That(content.MetadataOfficeLocation, Has.Count.EqualTo(1));
            Assert.That(content.MicrositeMasterPages, Has.Count.EqualTo(1));
            Assert.That(content.MicrositePages, Has.Count.EqualTo(1));
            Assert.That(content.MetadataRssCategory, Has.Count.EqualTo(1));
            Assert.That(content.Insights, Has.Count.EqualTo(1));
            Assert.That(content.RelevantContactPerson, Has.Count.EqualTo(1));
            Assert.That(content.RelatedExternalPeople, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void InsightPageContent_DateProperties_CanBeSet()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);
        var content = new InsightPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
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
    public void PageOutput_InsightPageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new InsightPageContent();
        var output = new PageOutput<InsightPageContent>
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
    public async Task GetInsightPagesAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetInsightPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPagesAsync(1, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetInsightPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

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
    public void InsightCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IInsightCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetInsightPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetInsightPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreateCopyInsightAsync"), Is.Not.Null);
        });
    }

    [Test]
    public async Task CreateCopyInsightAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreateCopyInsightAsync(
            channelId: 1,
            culture: "en-gb",
            workspaceName: "KenticoDefault",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreateCopyInsightAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var request = new CopyInsightPageContent
        {
            Title = "Copied Insight",
            ImageFileAltText = "Alt text"
        };

        var result = await service.CreateCopyInsightAsync(
            channelId: 1,
            culture: "en-gb",
            workspaceName: "KenticoDefault",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public void CopyInsightPageContent_DefaultValues_AreInitialized()
    {
        var content = new CopyInsightPageContent
        {
            Title = "T",
            ImageFileAltText = "A"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.OriginalIndustries, Is.Empty);
            Assert.That(content.OriginalServiceLines, Is.Empty);
            Assert.That(content.OriginalInvolvedCountries, Is.Empty);
            Assert.That(content.ImageFile, Is.Empty);
            Assert.That(content.ShowInsightTitle, Is.False);
            Assert.That(content.ShowInsightDescription, Is.False);
            Assert.That(content.ShowCallToAction, Is.False);
            Assert.That(content.SharingOptOut, Is.False);
        });
    }

    [Test]
    public void CopyInsightPageContent_Properties_CanBeSet()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);

        var content = new CopyInsightPageContent
        {
            Title = "Copied Insight",
            ShowInsightTitle = true,
            Description = "A description",
            ShowInsightDescription = true,
            InsightContent = "<p>body</p>",
            CallToActionPosition = "top",
            DownloadButtonTitle = "Download",
            DownloadButtonText = "Get it",
            SharingOptOut = true,
            SharingHeadline = true,
            OriginalSiteGuid = Guid.NewGuid().ToString(),
            OriginalInvolvedCountriesAll = true,
            OriginalDocumentOwnerOptOut = true,
            OriginalDocumentOwnerName = "Owner",
            OriginalDocumentOwnerEmail = "owner@example.com",
            ShowCallToAction = true,
            CallToActionButtonText = "Click",
            CallToActionButtonUrl = "https://example.com",
            ImageFileAltText = "Alt text",
            OriginalIndustries = ["ind1"],
            OriginalServiceLines = ["sl1"],
            OriginalInvolvedCountries = ["US"],
            ImageFile = ["image.jpg"],
            PublishFrom = from,
            PublishTo = to,
            RedirectOnUnpublish = "/redirect"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.Title, Is.EqualTo("Copied Insight"));
            Assert.That(content.ShowInsightTitle, Is.True);
            Assert.That(content.Description, Is.EqualTo("A description"));
            Assert.That(content.ShowInsightDescription, Is.True);
            Assert.That(content.InsightContent, Is.EqualTo("<p>body</p>"));
            Assert.That(content.SharingOptOut, Is.True);
            Assert.That(content.SharingHeadline, Is.True);
            Assert.That(content.OriginalInvolvedCountriesAll, Is.True);
            Assert.That(content.OriginalDocumentOwnerOptOut, Is.True);
            Assert.That(content.OriginalDocumentOwnerEmail, Is.EqualTo("owner@example.com"));
            Assert.That(content.ShowCallToAction, Is.True);
            Assert.That(content.CallToActionButtonUrl, Is.EqualTo("https://example.com"));
            Assert.That(content.ImageFileAltText, Is.EqualTo("Alt text"));
            Assert.That(content.OriginalIndustries, Has.Count.EqualTo(1));
            Assert.That(content.OriginalServiceLines, Has.Count.EqualTo(1));
            Assert.That(content.OriginalInvolvedCountries, Has.Count.EqualTo(1));
            Assert.That(content.ImageFile, Has.Count.EqualTo(1));
            Assert.That(content.PublishFrom, Is.EqualTo(from));
            Assert.That(content.PublishTo, Is.EqualTo(to));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/redirect"));
        });
    }

    [Test]
    public void PageOutput_CopyInsightPageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new CopyInsightPageContent { Title = "T", ImageFileAltText = "A" };
        var output = new PageOutput<CopyInsightPageContent>
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

    #region CopyInsightDto

    [Test]
    public void CopyInsightDto_DefaultValues_AreInitialized()
    {
        var dto = new CopyInsightDto
        {
            Title = "My Insight",
            ImageFileAltText = "Alt text"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("My Insight"));
            Assert.That(dto.ImageFileAltText, Is.EqualTo("Alt text"));
            Assert.That(dto.ShowInsightTitle, Is.False);
            Assert.That(dto.Description, Is.Null);
            Assert.That(dto.ShowInsightDescription, Is.False);
            Assert.That(dto.InsightContent, Is.Null);
            Assert.That(dto.CallToActionPosition, Is.Null);
            Assert.That(dto.Attachments, Is.Null);
            Assert.That(dto.InsightAttachment, Is.Null);
            Assert.That(dto.DownloadButtonTitle, Is.Null);
            Assert.That(dto.DownloadButtonText, Is.Null);
            Assert.That(dto.Contentsharing, Is.Null);
            Assert.That(dto.SharingOptOut, Is.False);
            Assert.That(dto.SharingHeadline, Is.False);
            Assert.That(dto.OriginalSiteGuid, Is.Null);
            Assert.That(dto.OriginalDocumentAttachmentType, Is.Null);
            Assert.That(dto.OriginalIndustries, Is.Empty);
            Assert.That(dto.OriginalServiceLines, Is.Empty);
            Assert.That(dto.OriginalInvolvedCountriesAll, Is.False);
            Assert.That(dto.OriginalInvolvedCountries, Is.Empty);
            Assert.That(dto.OriginalDocumentOwnerOptOut, Is.False);
            Assert.That(dto.OriginalDocumentOwnerName, Is.Null);
            Assert.That(dto.OriginalDocumentOwnerEmail, Is.Null);
            Assert.That(dto.SharingSourceFirm, Is.Null);
            Assert.That(dto.SharingInstructions, Is.Null);
            Assert.That(dto.ShowCallToAction, Is.False);
            Assert.That(dto.CallToActionDescription, Is.Null);
            Assert.That(dto.CallToActionButtonText, Is.Null);
            Assert.That(dto.CallToActionButtonUrl, Is.Null);
            Assert.That(dto.ImageFile, Is.Empty);
        });
    }

    [Test]
    public void CopyInsightDto_Properties_CanBeSet()
    {
        var dto = new CopyInsightDto
        {
            Title = "Title",
            ShowInsightTitle = true,
            Description = "Description",
            ShowInsightDescription = true,
            InsightContent = "Content",
            CallToActionPosition = "Top",
            Attachments = "attach",
            InsightAttachment = "insight-attach",
            DownloadButtonTitle = "Download",
            DownloadButtonText = "Click",
            Contentsharing = "sharing",
            SharingOptOut = true,
            SharingHeadline = true,
            OriginalSiteGuid = "site-guid",
            OriginalDocumentAttachmentType = "pdf",
            OriginalIndustries = s_originalIndustries,
            OriginalServiceLines = s_originalServiceLines,
            OriginalInvolvedCountriesAll = true,
            OriginalInvolvedCountries = s_originalInvolvedCountries,
            OriginalDocumentOwnerOptOut = true,
            OriginalDocumentOwnerName = "Owner",
            OriginalDocumentOwnerEmail = "owner@example.com",
            SharingSourceFirm = "Firm",
            SharingInstructions = "Instructions",
            ShowCallToAction = true,
            CallToActionDescription = "CTA description",
            CallToActionButtonText = "Go",
            CallToActionButtonUrl = "https://example.com",
            ImageFile = s_imageFile,
            ImageFileAltText = "Alt"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("Title"));
            Assert.That(dto.ShowInsightTitle, Is.True);
            Assert.That(dto.Description, Is.EqualTo("Description"));
            Assert.That(dto.ShowInsightDescription, Is.True);
            Assert.That(dto.InsightContent, Is.EqualTo("Content"));
            Assert.That(dto.CallToActionPosition, Is.EqualTo("Top"));
            Assert.That(dto.Attachments, Is.EqualTo("attach"));
            Assert.That(dto.InsightAttachment, Is.EqualTo("insight-attach"));
            Assert.That(dto.DownloadButtonTitle, Is.EqualTo("Download"));
            Assert.That(dto.DownloadButtonText, Is.EqualTo("Click"));
            Assert.That(dto.Contentsharing, Is.EqualTo("sharing"));
            Assert.That(dto.SharingOptOut, Is.True);
            Assert.That(dto.SharingHeadline, Is.True);
            Assert.That(dto.OriginalSiteGuid, Is.EqualTo("site-guid"));
            Assert.That(dto.OriginalDocumentAttachmentType, Is.EqualTo("pdf"));
            Assert.That(dto.OriginalIndustries, Is.EqualTo(s_originalIndustries));
            Assert.That(dto.OriginalServiceLines, Is.EqualTo(s_originalServiceLines));
            Assert.That(dto.OriginalInvolvedCountriesAll, Is.True);
            Assert.That(dto.OriginalInvolvedCountries, Is.EqualTo(s_originalInvolvedCountries));
            Assert.That(dto.OriginalDocumentOwnerOptOut, Is.True);
            Assert.That(dto.OriginalDocumentOwnerName, Is.EqualTo("Owner"));
            Assert.That(dto.OriginalDocumentOwnerEmail, Is.EqualTo("owner@example.com"));
            Assert.That(dto.SharingSourceFirm, Is.EqualTo("Firm"));
            Assert.That(dto.SharingInstructions, Is.EqualTo("Instructions"));
            Assert.That(dto.ShowCallToAction, Is.True);
            Assert.That(dto.CallToActionDescription, Is.EqualTo("CTA description"));
            Assert.That(dto.CallToActionButtonText, Is.EqualTo("Go"));
            Assert.That(dto.CallToActionButtonUrl, Is.EqualTo("https://example.com"));
            Assert.That(dto.ImageFile, Is.EqualTo(s_imageFile));
            Assert.That(dto.ImageFileAltText, Is.EqualTo("Alt"));
        });
    }

    [Test]
    public void CopyInsightDto_MissingRequiredFields_FailValidation()
    {
        var dto = new CopyInsightDto();
        var results = new List<ValidationResult>();
        var context = new ValidationContext(dto);

        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(results, Has.Some.Matches<ValidationResult>(r => r.MemberNames.Contains(nameof(CopyInsightDto.Title))));
            Assert.That(results, Has.Some.Matches<ValidationResult>(r => r.MemberNames.Contains(nameof(CopyInsightDto.ImageFileAltText))));
        });
    }

    [Test]
    public void CopyInsightDto_ValidRequiredFields_PassValidation()
    {
        var dto = new CopyInsightDto
        {
            Title = "Title",
            ImageFileAltText = "Alt"
        };
        var results = new List<ValidationResult>();
        var context = new ValidationContext(dto);

        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);
        });
    }

    #endregion

    #region Branch coverage (mocked channel and pages)

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private string? channelName;

        public override string ChannelName
        {
            get => channelName ?? string.Empty;
            set => channelName = value;
        }
    }

    private void SetupNamedChannel(string channelName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelName = channelName;
        SetupChannel(channel);
    }

    private void SetupInsights(params IEnumerable<Insight>?[] results)
    {
        var sequence = _contentRetrieverMock.SetupSequence(x => x.GetPagesAsync<Insight>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()));

        foreach (var result in results)
        {
            sequence = sequence.ReturnsAsync(result!);
        }
    }

    private void SetupSectionInsights(IEnumerable<SectionInsightsPage>? result) =>
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result!);

    private static Insight CreateInsight(int id) =>
        new() { SystemFields = new WebPageFields { WebPageItemID = id, ContentItemName = "insight" } };

    private static void AssertNotFoundMessage<T>(ActionResult<T> result, string message)
    {
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo(message));
    }

    [Test]
    public async Task GetInsightPagesAsync_NullResults_ReturnsEmptyList()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights((IEnumerable<Insight>?)null);

        var result = await CreateService().GetInsightPagesAsync(1, "en-gb", false, CancellationToken.None);

        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetInsightPageAsync_PageMissing_ReturnsNotFoundResult()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(Enumerable.Empty<Insight>());

        var result = await CreateService().GetInsightPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePageAsync_ParentNull_Returns422AndAddsError()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupSectionInsights(null);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        _validationStateMock.Verify(x => x.AddUnprocessableEntityError("nodeParentID", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreatePageAsync_UnprocessableValidationErrors_Returns422WithoutCreatingPage()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupSectionInsights(Enumerable.Empty<SectionInsightsPage>());
        _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        _webPageManagerMock.Verify(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreatePageAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupCulture(ValidCulture);
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", new InsightPageContent(), CancellationToken.None);

        var objectResult = (ObjectResult)result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(objectResult.Value, Is.EqualTo("boom"));
        });
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidToCulture_ReturnsNotFoundMessage()
    {
        _mappingHelperMock
            .SetupSequence(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidCulture)
            .ReturnsAsync(EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", new InsightPageContent(), CancellationToken.None);

        AssertNotFoundMessage(result, "Invalid to culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SourceMissing_ReturnsNotFoundResult()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(Enumerable.Empty<Insight>());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), null, null, new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SystemFieldsMissing_ReturnsNotFoundMessage()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(new[] { new Insight { SystemFields = null! } });

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", new InsightPageContent(), CancellationToken.None);

        AssertNotFoundMessage(result, "Page system fields are missing");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_ValidationErrors_Returns422()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task UpdatePageAsync_PageMissing_ReturnsNotFoundResult()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights((IEnumerable<Insight>?)null);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), null, new InsightPageContent(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdatePageAsync_SystemFieldsMissing_ReturnsNotFoundMessage()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(new[] { new Insight { SystemFields = null! } });

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", new InsightPageContent(), CancellationToken.None);

        AssertNotFoundMessage(result, "Page system fields are missing");
        _webPageManagerMock.Verify(x => x.TryCreateDraft(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void DeletePageAsync_SingleCulture_PageMissing_ThrowsNotFound()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(Enumerable.Empty<Insight>());

        Assert.ThrowsAsync<BDO.GWT.Common.Core.Exceptions.NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", deleteAllCultures: false, CancellationToken.None));

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeletePageAsync_SingleCulture_DeletesPermanently()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        SetupInsights(new[] { CreateInsight(12) });
        DeleteWebPageParameters? captured = null;
        _webPageManagerMock
            .Setup(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteWebPageParameters, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", deleteAllCultures: false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured!.WebPageItemID, Is.EqualTo(12));
            Assert.That(captured.Permanently, Is.True);
        });
    }

    [Test]
    public async Task DeletePageAsync_AllCultures_DeletesOnlyFoundVariants()
    {
        SetupCulture(ValidCulture);
        SetupNamedChannel("TestChannel");
        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB, LanguageTag.GB });
        SetupInsights(new[] { CreateInsight(4) }, Enumerable.Empty<Insight>());

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", deleteAllCultures: true, CancellationToken.None);

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
