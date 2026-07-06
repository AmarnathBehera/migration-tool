using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget;

public class RelatedContactsServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IPersonPageService> _personPageServiceMock = null!;
    private RelatedContactsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _personPageServiceMock = new Mock<IPersonPageService>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-US");

        SetupEmptySectionPeoplePages();
        SetupEmptyPersonPages();

        _service = new RelatedContactsService(
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _personPageServiceMock.Object);
    }

    private void SetupEmptySectionPeoplePages()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionPeoplePage>());
    }

    private void SetupEmptyPersonPages()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());
    }

    private static RelatedContactsData CreateDefaultData() => new()
    {
        Title = "Title",
        Description = "Description",
        TopMargin = "0px",
        BottomMargin = "0px",
    };

    private static BusinessLine CreateBusinessLinePage(Guid? guid = null) =>
        new() { SystemFields = new WebPageFields { ContentItemGUID = guid ?? Guid.NewGuid() } };

    private void SetupSectionPeoplePages(params SectionPeoplePage[] pages)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);
    }

    private void SetupPersonPages(params Person[] people)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(people);
    }

    private static SectionPeoplePage CreateSectionPage(int webPageItemId = 5) =>
        new() { SystemFields = new WebPageFields { WebPageItemID = webPageItemId } };

    [Test]
    public async Task GetRelatedContactsDtoAsync_MapsScalarDataFieldsToDto()
    {
        var data = new RelatedContactsData
        {
            Title = "Related Contacts",
            Description = "A description",
            TopMargin = "40px",
            BottomMargin = "20px",
            WidgetID = "related-contacts-1",
        };

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Related Contacts"));
            Assert.That(result.Description, Is.EqualTo("A description"));
            Assert.That(result.TopMargin, Is.EqualTo("40px"));
            Assert.That(result.BottomMargin, Is.EqualTo("20px"));
            Assert.That(result.WidgetID, Is.EqualTo("related-contacts-1"));
        });
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithNullTitleAndDescription_ReturnsDtoWithNulls()
    {
        var data = CreateDefaultData();
        data.Title = null;
        data.Description = null;

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Description, Is.Null);
        });
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithSeeAllLink_BuildsLinkItemDto()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = "See all";
        data.SeeAllLinkUrl = "/people";
        data.SeeAllLinkURLTarget = "_blank";

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.SeeAllLink!.Title, Is.EqualTo("See all"));
            Assert.That(result.SeeAllLink!.URL, Is.EqualTo("/people"));
            Assert.That(result.SeeAllLink!.Target, Is.EqualTo("_blank"));
        });
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithNullSeeAllLinkText_ReturnsNullSeeAllLink()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = null;
        data.SeeAllLinkUrl = "/people";

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithNullSeeAllLinkUrl_ReturnsNullSeeAllLink()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = "See all";
        data.SeeAllLinkUrl = null;

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithNoSectionPeoplePages_ReturnsEmptyItems()
    {
        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_UsesCurrentLanguageTag_FromCultureContext()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("fr-FR");

        RetrievePagesParameters? capturedSectionParams = null;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedSectionParams = p)
            .ReturnsAsync(Enumerable.Empty<SectionPeoplePage>());

        await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        Assert.That(capturedSectionParams, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedSectionParams!.LanguageName, Is.EqualTo("fr-FR"));
            Assert.That(capturedSectionParams!.UseLanguageFallbacks, Is.False);
        });
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_PassesCancellationTokenToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (_, _, _, t) => capturedToken = t)
            .ReturnsAsync(Enumerable.Empty<SectionPeoplePage>());

        await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), token);

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithUnknownPageType_ReturnsEmptyItems()
    {
        var data = CreateDefaultData();

        // BlogPost has no switch-case in the service, so its filter will be null -> no people filtered in
        var unknownPage = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } };

        SetupSectionPeoplePages(CreateSectionPage());
        SetupPersonPages();

        var result = await _service.GetRelatedContactsDtoAsync(unknownPage, data, CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithBusinessLinePage_DoesNotCallPersonPageService_WhenNoPeople()
    {
        SetupSectionPeoplePages(CreateSectionPage(10));
        SetupPersonPages();

        await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        _personPageServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithNoSectionPeoplePages_DoesNotQueryPeople()
    {
        await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithSectionPeoplePage_QueriesPeopleWithLinkedItemsMaxLevel2()
    {
        SetupSectionPeoplePages(CreateSectionPage());
        SetupPersonPages();

        RetrievePagesParameters? capturedParams = null;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedParams = p)
            .ReturnsAsync(Enumerable.Empty<Person>());

        await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        Assert.That(capturedParams, Is.Not.Null);
        Assert.That(capturedParams!.LinkedItemsMaxLevel, Is.EqualTo(2));
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithEmptyPeopleResult_ReturnsEmptyItems()
    {
        SetupSectionPeoplePages(CreateSectionPage());
        SetupPersonPages();

        var result = await _service.GetRelatedContactsDtoAsync(CreateBusinessLinePage(), CreateDefaultData(), CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithLocationCityPage_AndNoChildOffices_ReturnsEmptyItems()
    {
        var data = CreateDefaultData();
        var page = new LocationCity { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid(), WebPageItemID = 42 } };

        _contentServiceMock
            .Setup(x => x.GetChildPagesByParentGuidAsync<LocationOffice>(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<LocationOffice>());

        var result = await _service.GetRelatedContactsDtoAsync(page, data, CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetRelatedContactsDtoAsync_WithLocationCityPage_AndChildOffices_QueriesPeople()
    {
        var officeGuid = Guid.NewGuid();
        var data = CreateDefaultData();
        var page = new LocationCity { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid(), WebPageItemID = 42 } };

        var office = new LocationOffice
        {
            SystemFields = new WebPageFields { ContentItemGUID = officeGuid }
        };

        _contentServiceMock
            .Setup(x => x.GetChildPagesByParentGuidAsync<LocationOffice>(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { office });

        SetupSectionPeoplePages(CreateSectionPage());
        SetupPersonPages();

        var result = await _service.GetRelatedContactsDtoAsync(page, data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(nameof(ServiceArea))]
    [TestCase(nameof(Service))]
    [TestCase(nameof(Industry))]
    [TestCase(nameof(IndustryService))]
    [TestCase(nameof(IndustryCategory))]
    [TestCase(nameof(LocationOffice))]
    [TestCase(nameof(MicrositeMasterPage))]
    [TestCase(nameof(MicrositePage))]
    public async Task GetRelatedContactsDtoAsync_WithKnownPageTypes_TriesToQueryPeople(string pageTypeName)
    {
        var data = CreateDefaultData();

        SetupSectionPeoplePages(CreateSectionPage());
        SetupPersonPages();

        IWebPageFieldsSource page = pageTypeName switch
        {
            nameof(ServiceArea) => new ServiceArea { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(Service) => new Service { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(Industry) => new Industry { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(IndustryService) => new IndustryService { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(IndustryCategory) => new IndustryCategory { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(LocationOffice) => new LocationOffice { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(MicrositeMasterPage) => new MicrositeMasterPage { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            nameof(MicrositePage) => new MicrositePage { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } },
            _ => throw new ArgumentOutOfRangeException(nameof(pageTypeName), pageTypeName, null)
        };

        var result = await _service.GetRelatedContactsDtoAsync(page, data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
