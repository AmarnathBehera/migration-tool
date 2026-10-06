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
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class DealCrudServiceTests
{
    // These tests exercise null-request guard clauses that return before any dependency is used.
    private static DealCrudService CreateServiceWithoutConstructor()
    {
        return (DealCrudService)RuntimeHelpers.GetUninitializedObject(typeof(DealCrudService));
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<DealPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<DealPageContent>> result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<DealPageContent>> result = await service.UpdatePageAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }
}

[TestFixture]
public class DealCrudServiceBranchTests
{
    private Mock<IContentService> _contentRetrieverMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentService>();
        _validationStateMock = new Mock<IValidationState>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _cultureServiceMock = new Mock<ICultureService>();
        _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _webPageManagerMock = new Mock<IWebPageManager>();
        _mappingHelperMock = new Mock<IMappingHelper>();
        _pageContentValidatorMock = new Mock<IPageContentValidator>();

        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB });
    }

    private DealCrudService CreateService()
    {
        var service = (DealCrudService)RuntimeHelpers.GetUninitializedObject(typeof(DealCrudService));

        SetPrivateField(service, "_contentRetriever", _contentRetrieverMock.Object);
        SetPrivateField(service, "_validationState", _validationStateMock.Object);
        SetPrivateField(service, "_websiteChannelContext", _websiteChannelContextMock.Object);
        SetPrivateField(service, "_cultureService", _cultureServiceMock.Object);
        SetPrivateField(service, "_channelInfoProvider", _channelInfoProviderMock.Object);
        SetPrivateField(service, "_webPageManager", _webPageManagerMock.Object);
        SetPrivateField(service, "_mappingHelper", _mappingHelperMock.Object);
        SetPrivateField(service, "_pageContentValidator", _pageContentValidatorMock.Object);
        SetPrivateField(service, "CHANNEL_NAME", string.Empty);

        return service;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
        field!.SetValue(target, value);
    }

    private void SetupChannel(string? channelName)
    {
        FakeChannelInfo? channel = null;
        if (channelName is not null)
        {
            channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
            channel.ChannelName = channelName;
        }

        _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);
    }

    private void SetupDeals(params IEnumerable<Deal>?[] results)
    {
        var sequence = _contentRetrieverMock.SetupSequence(x => x.GetPagesAsync<Deal>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()));

        foreach (var result in results)
        {
            sequence = sequence.ReturnsAsync(result!);
        }
    }

    private void SetupSectionDeals(IEnumerable<SectionDeals>? result) =>
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<SectionDeals>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result!);

    private static Deal CreateDeal(int webPageItemId) =>
        new() { SystemFields = new WebPageFields { WebPageItemID = webPageItemId, ContentItemName = "deal" } };

    private static DealPageContent CreateRequest() => new() { MetadataTitle = "Deal title" };

    [Test]
    public void GetDealPagesAsync_ChannelNotFound_ThrowsNotFound()
    {
        SetupChannel(null);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPagesAsync(1, "en-gb", true, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel not found"));
    }

    [Test]
    public async Task GetDealPagesAsync_NoDeals_ReturnsEmptyListAndUsesPreviewFlag()
    {
        SetupChannel("TestChannel");
        RetrievePagesParameters? captured = null;
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<Deal>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => captured = p)
            .ReturnsAsync(Enumerable.Empty<Deal>());

        var result = await CreateService().GetDealPagesAsync(1, null, false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.Empty);
            Assert.That(captured!.IsForPreview, Is.True);
            Assert.That(captured.ChannelName, Is.EqualTo("TestChannel"));
            Assert.That(captured.LinkedItemsMaxLevel, Is.EqualTo(5));
        });
    }

    [Test]
    public void GetDealPagesAsync_CultureNotOnSite_AddsValidationError()
    {
        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageTag>());
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPagesAsync(1, "en-gb", true, CancellationToken.None));

        _validationStateMock.Verify(x => x.AddError("culture", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetDealPagesAsync_InvalidCultureFormat_AddsFormatErrorAndFallsBackToDefault()
    {
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPagesAsync(1, "not a culture!!", true, CancellationToken.None));

        _validationStateMock.Verify(x => x.AddError("culture", It.IsAny<string>()), Times.Once);
        _cultureServiceMock.Verify(
            x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void GetDealPageAsync_ChannelNotFound_ThrowsNotFound()
    {
        SetupChannel(string.Empty);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel not found"));
    }

    [Test]
    public void GetDealPageAsync_DealMissing_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(Enumerable.Empty<Deal>());

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None));
    }

    [Test]
    public void GetDealPageAsync_NullResult_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals((IEnumerable<Deal>?)null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDealPageAsync(1, Guid.NewGuid(), null, CancellationToken.None));
    }

    [Test]
    public void CreatePageAsync_ChannelNotFound_ThrowsAndSkipsValidation()
    {
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None));

        _pageContentValidatorMock.Verify(
            x => x.ValidateAndNormalizePageContent(It.IsAny<DealPageContent>()), Times.Never);
    }

    [Test]
    public void CreatePageAsync_ValidationErrors_ThrowsBeforeParentLookup()
    {
        SetupChannel("TestChannel");
        _validationStateMock.Setup(x => x.HasErrors()).Returns(true);

        Assert.ThrowsAsync<ValidationException>(() =>
            CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None));

        _contentRetrieverMock.Verify(
            x => x.GetPagesAsync<SectionDeals>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void CreatePageAsync_ParentNull_AddsErrorAndThrowsUnprocessable()
    {
        SetupChannel("TestChannel");
        SetupSectionDeals(null);

        Assert.ThrowsAsync<UnprocessableEntityException>(() =>
            CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None));

        _validationStateMock.Verify(
            x => x.AddUnprocessableEntityError("nodeParentID", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CreatePageAsync_UpdateDraftFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupSectionDeals(Enumerable.Empty<SectionDeals>());
        _webPageManagerMock.Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(5, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to update page draft"));
    }

    [Test]
    public void CreatePageAsync_PublishFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupSectionDeals(Enumerable.Empty<SectionDeals>());
        _webPageManagerMock.Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(5, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(5, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to publish updated page"));
    }

    [Test]
    public async Task CreatePageAsync_CreatedPageNotFound_ReturnsBadRequestAndUsesParentId()
    {
        SetupChannel("TestChannel");
        SetupSectionDeals(Enumerable.Empty<SectionDeals>());
        CreateWebPageParameters? captured = null;
        _webPageManagerMock
            .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Callback<CreateWebPageParameters, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(5);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(5, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(5, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SetupDeals(Enumerable.Empty<Deal>());

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
            Assert.That(captured!.ParentWebPageItemID, Is.EqualTo(10));
        });
    }

    [Test]
    public void CreatePageCultureVersionAsync_ChannelNotFound_ThrowsNotFound()
    {
        SetupChannel(null);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel not found"));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SourcePageMissing_ReturnsNotFoundResult()
    {
        SetupChannel("TestChannel");
        SetupDeals(Enumerable.Empty<Deal>());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), null, null, CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void CreatePageCultureVersionAsync_VariantCreationFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(8) });
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to create language variant"));
    }

    [Test]
    public void CreatePageCultureVersionAsync_UpdateDraftFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(8) });
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to update page draft"));
    }

    [Test]
    public void CreatePageCultureVersionAsync_PublishFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(8) });
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to publish updated page"));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_VariantNotFoundAfterPublish_ReturnsBadRequest()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(8) }, Enumerable.Empty<Deal>());
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void UpdatePageAsync_ChannelNotFound_ThrowsNotFound()
    {
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None));
    }

    [Test]
    public async Task UpdatePageAsync_PageMissing_ReturnsNotFoundResult()
    {
        SetupChannel("TestChannel");
        SetupDeals((IEnumerable<Deal>?)null);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void UpdatePageAsync_UpdateDraftFails_ThrowsAfterCreatingDraft()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(3) });
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to update page draft"));
        _webPageManagerMock.Verify(x => x.TryCreateDraft(3, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void UpdatePageAsync_PublishFails_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(3) });
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Failed to publish updated page"));
    }

    [Test]
    public async Task UpdatePageAsync_UpdatedPageNotFound_ReturnsBadRequest()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(3) }, Enumerable.Empty<Deal>());
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_ThrowsNotFound()
    {
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public void DeletePageAsync_SingleCulture_PageMissing_ThrowsNotFound()
    {
        SetupChannel("TestChannel");
        SetupDeals(Enumerable.Empty<Deal>());

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeletePageAsync_SingleCulture_DeletesPermanently()
    {
        SetupChannel("TestChannel");
        SetupDeals(new[] { CreateDeal(12) });
        DeleteWebPageParameters? captured = null;
        _webPageManagerMock
            .Setup(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteWebPageParameters, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.WebPageItemID, Is.EqualTo(12));
            Assert.That(captured.Permanently, Is.True);
        });
    }

    [Test]
    public async Task DeletePageAsync_AllCultures_DeletesOnlyExistingVariants()
    {
        SetupChannel("TestChannel");
        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB, LanguageTag.GB, LanguageTag.GB });
        SetupDeals(new[] { CreateDeal(4) }, Enumerable.Empty<Deal>(), (IEnumerable<Deal>?)null);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None);

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Once);
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
}
