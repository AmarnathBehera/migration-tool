using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Services;

[TestFixture]
public class CareersPageServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private CareersPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _cardsServiceMock = new Mock<ICardsService>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _settingServiceMock = new Mock<ISettingService>();

        _cultureContextMock
            .Setup(c => c.CurrentLanguageTag)
            .Returns("en-gb");

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HeroData d, CancellationToken _) => new HeroDto
            {
                Layout = d.Layout,
                Title = d.Title,
                Description = d.Description,
                CtaButtonText = d.CallToActionButtonText,
                CtaButtonType = d.CallToActionButtonType,
            });

        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/careers", "https://example.com/careers"));

        _settingServiceMock
            .Setup(s => s.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);

        _service = new CareersPageService(
            _heroServiceMock.Object,
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _cardsServiceMock.Object,
            _webPageUrlRetrieverMock.Object,
            _settingServiceMock.Object);
    }

    // -- Builders ------------------------------------------------------------

    private static Careers CreateDefaultCareersPage(
        bool filteringEnabled = false,
        string careerListFilters = "",
        string? metadataTitle = "Careers") => new Careers
        {
            HeroTitle = "Career Hero Title",
            HeroDescription = "Career Hero Description",
            HeroLayout = "default",
            HeroBackgroundImageAltText = "BG Alt",
            HeroBackgroundImage_MobileAltText = "Mobile BG Alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Apply Now",
            ItemListTitle = "Open Positions",
            ItemListDescription = "Find your role",
            ItemListFilteringEnabled = filteringEnabled,
            CareerListFilters = careerListFilters,
            ItemListAdvancedSearchDescription = "Need help?",
            ItemListAdvancedSearchLinkText = "Advanced Search",
            MetadataTitle = metadataTitle!,
            SystemFields = new WebPageFields
            {
                WebPageItemID = 100,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "Careers",
                WebPageUrlPath = "/careers",
                WebPageItemTreePath = "/careers",
            },
        };

    private static Career CreateDefaultCareer(
        int webPageItemId = 200,
        string title = "Software Engineer",
        string? reference = "REF-001",
        DateTime? publishDate = null) => new Career
    {
        MetadataTitle = title,
        MetadataTeaserImageAltText = "Career Image Alt",
        CareerReference = reference,
        SystemFields = new WebPageFields
        {
            WebPageItemID = webPageItemId,
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = title,
            WebPageUrlPath = "/careers/" + webPageItemId,
            WebPageItemTreePath = "/careers/" + webPageItemId,
            ContentItemCommonDataFirstPublishedWhen = publishDate,
        },
    };

    private void SetupCurrentPage(Careers? page) =>
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Careers>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

    private void SetupCareersPagesLookup(IEnumerable<Careers> pages) =>
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Careers>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);

    private void SetupCareerPagesLookup(IEnumerable<Career>? pages) =>
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Career>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages!);

    // -- GetCareersPageDtoAsync ----------------------------------------------

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
    {
        SetupCurrentPage(null);

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
            Assert.That(result.CardsGrid, Is.Null);
        });
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageIsNull_DoesNotCallHeroService()
    {
        SetupCurrentPage(null);

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageIsNull_DoesNotCallCardsService()
    {
        SetupCurrentPage(null);

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageExists_MapsHeroFields()
    {
        SetupCurrentPage(CreateDefaultCareersPage());

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Layout, Is.EqualTo("default"));
            Assert.That(result.Hero.Title, Is.EqualTo("Career Hero Title"));
            Assert.That(result.Hero.Description, Is.EqualTo("Career Hero Description"));
            Assert.That(result.Hero.CtaButtonText, Is.EqualTo("Apply Now"));
            Assert.That(result.Hero.CtaButtonType, Is.EqualTo("primary"));
        });
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageExists_CallsHeroServiceOnce()
    {
        SetupCurrentPage(CreateDefaultCareersPage());

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenPageExists_BuildsCardsGridFromPageFields()
    {
        var page = CreateDefaultCareersPage();
        SetupCurrentPage(page);

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CardsGrid, Is.Not.Null);
            Assert.That(result.CardsGrid.Title, Is.EqualTo("Open Positions"));
            Assert.That(result.CardsGrid.Description, Is.EqualTo("Find your role"));
            Assert.That(result.CardsGrid.PageSize, Is.EqualTo(20));
            Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.EqualTo("Need help?"));
            Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.EqualTo("Advanced Search"));
            Assert.That(
                result.CardsGrid.ApiEndpointName,
                Is.EqualTo($"Careers/{page.SystemFields.WebPageItemGUID}/Get"));
        });
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenFilteringDisabled_FiltersAreNull()
    {
        SetupCurrentPage(CreateDefaultCareersPage(filteringEnabled: false));

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.That(result.CardsGrid.Filters, Is.Null);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenFilteringDisabled_DoesNotCallCardsService()
    {
        SetupCurrentPage(CreateDefaultCareersPage(filteringEnabled: false));

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenFilteringEnabledButFilterStringEmpty_DoesNotCallCardsService()
    {
        SetupCurrentPage(CreateDefaultCareersPage(filteringEnabled: true, careerListFilters: ""));

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenFilteringEnabledWithCommaSeparatedFilters_CallsCardsServiceWithParsedSet()
    {
        SetupCurrentPage(CreateDefaultCareersPage(
            filteringEnabled: true,
            careerListFilters: "careerEmploymentType, careerLevel"));

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.Is<HashSet<string>>(s =>
                    s.Count == 2 &&
                    s.Contains("careerEmploymentType") &&
                    s.Contains("careerLevel")),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_FilterSeparators_SemicolonAndNewlinesAreHonored()
    {
        SetupCurrentPage(CreateDefaultCareersPage(
            filteringEnabled: true,
            careerListFilters: "careerEmploymentType;careerLevel\ncareerJobTitle"));

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.GetCareersPageDtoAsync(CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.Is<HashSet<string>>(s =>
                    s.Count == 3 &&
                    s.Contains("careerEmploymentType") &&
                    s.Contains("careerLevel") &&
                    s.Contains("careerJobTitle")),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_WhenFilteringEnabled_PassesReturnedFiltersToCardsGrid()
    {
        SetupCurrentPage(CreateDefaultCareersPage(
            filteringEnabled: true,
            careerListFilters: "careerLevel"));

        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "careerLevel",
                Title = "Level",
                Type = "Select",
                Options = Array.Empty<OptionDto>(),
            },
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.That(result.CardsGrid.Filters, Is.EqualTo(expectedFilters));
    }

    [Test]
    public async Task GetCareersPageDtoAsync_FilteringTooltipEnabled_IsFalse_WhenSettingsReturnsNull()
    {
        SetupCurrentPage(CreateDefaultCareersPage(filteringEnabled: true));

        _settingServiceMock
            .Setup(s => s.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.False);
    }

    [Test]
    public async Task GetCareersPageDtoAsync_FilteringTooltipEnabled_IsFalse_WhenFilteringDisabled()
    {
        SetupCurrentPage(CreateDefaultCareersPage(filteringEnabled: false));

        var result = await _service.GetCareersPageDtoAsync(CancellationToken.None);

        Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.False);
    }

    // -- GetCareerPaginationResultAsync --------------------------------------

    [Test]
    public void GetCareerPaginationResultAsync_WhenCareersPageNotFound_ThrowsMissingSectionPageException()
    {
        SetupCareersPagesLookup(Array.Empty<Careers>());

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetCareerPaginationResultAsync(
                "en-gb",
                Guid.NewGuid(),
                new CardsQuery(),
                CancellationToken.None));
    }

    [Test]
    public void GetCareerPaginationResultAsync_WhenCareersPageNotFound_ExceptionMessageContainsPageGuid()
    {
        SetupCareersPagesLookup(Array.Empty<Careers>());
        var pageGuid = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetCareerPaginationResultAsync(
                "en-gb",
                pageGuid,
                new CardsQuery(),
                CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenNoCareerPages_ReturnsEmptyPagination()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(Array.Empty<Career>());

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenCareerPagesIsNull_ReturnsEmptyPagination()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(null);

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenCareersExist_ReturnsCorrectTotalCount()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[]
        {
            CreateDefaultCareer(webPageItemId: 1, title: "Job A"),
            CreateDefaultCareer(webPageItemId: 2, title: "Job B"),
            CreateDefaultCareer(webPageItemId: 3, title: "Job C"),
        });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenCareersExist_ReturnsCardForEachCareer()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[]
        {
            CreateDefaultCareer(webPageItemId: 1, title: "Job A"),
            CreateDefaultCareer(webPageItemId: 2, title: "Job B"),
        });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result.Data, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_OrdersByDocumentPublishFromDescending()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[]
        {
            CreateDefaultCareer(webPageItemId: 1, title: "Oldest", publishDate: new DateTime(2020, 1, 1)),
            CreateDefaultCareer(webPageItemId: 2, title: "Newest", publishDate: new DateTime(2024, 1, 1)),
            CreateDefaultCareer(webPageItemId: 3, title: "Middle", publishDate: new DateTime(2022, 1, 1)),
        });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data[0].Title, Is.EqualTo("Newest"));
            Assert.That(result.Data[1].Title, Is.EqualTo("Middle"));
            Assert.That(result.Data[2].Title, Is.EqualTo("Oldest"));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenQueryIsNull_UsesDefaultPageSizeOf20()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        var careers = Enumerable.Range(1, 25)
            .Select(i => CreateDefaultCareer(webPageItemId: i, title: $"Job {i:00}"))
            .ToArray();
        SetupCareerPagesLookup(careers);

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            null!,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(20));
            Assert.That(result.TotalCount, Is.EqualTo(25));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenPageSizeProvided_RespectsPageSize()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        var careers = Enumerable.Range(1, 10)
            .Select(i => CreateDefaultCareer(webPageItemId: i, title: $"Job {i:00}"))
            .ToArray();
        SetupCareerPagesLookup(careers);

        var query = new CardsQuery { PageSize = 5, CurrentPage = 1 };

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(5));
            Assert.That(result.TotalCount, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenCurrentPageProvided_ReturnsCorrectSlice()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        var careers = Enumerable.Range(1, 10)
            .Select(i => CreateDefaultCareer(
                webPageItemId: i,
                title: $"Job {i:00}",
                publishDate: new DateTime(2024, 1, i)))
            .ToArray();
        SetupCareerPagesLookup(careers);

        var query = new CardsQuery { PageSize = 3, CurrentPage = 2 };

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(3));
            // Sorted descending by publish date: 10, 09, 08 (page 1), 07, 06, 05 (page 2)
            Assert.That(result.Data[0].Title, Is.EqualTo("Job 07"));
            Assert.That(result.Data[1].Title, Is.EqualTo("Job 06"));
            Assert.That(result.Data[2].Title, Is.EqualTo("Job 05"));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_WhenPageSizeZero_FallsBackToDefault20()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        var careers = Enumerable.Range(1, 22)
            .Select(i => CreateDefaultCareer(webPageItemId: i, title: $"Job {i:00}"))
            .ToArray();
        SetupCareerPagesLookup(careers);

        var query = new CardsQuery { PageSize = 0, CurrentPage = 0 };

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            query,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(20));
            Assert.That(result.TotalCount, Is.EqualTo(22));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_MapsCareerScalarFieldsToCardDto()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[]
        {
            CreateDefaultCareer(
                webPageItemId: 1,
                title: "QA Engineer",
                reference: "QA-001",
                publishDate: new DateTime(2024, 5, 10)),
        });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        var dto = result.Data.Single();
        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("QA Engineer"));
            Assert.That(dto.Reference, Is.EqualTo("QA-001"));
            Assert.That(dto.ImageAltText, Is.EqualTo("Career Image Alt"));
            Assert.That(dto.PublishDate, Is.EqualTo(new DateTime(2024, 5, 10)));
            Assert.That(dto.ApplyURL, Is.EqualTo("https://example.com/careers"));
        });
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_ImageURL_IsNull_WhenTeaserImageIsNotSet()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[] { CreateDefaultCareer() });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_Locations_DefaultsToEmptyArray_WhenMetadataOfficeLocationIsNull()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[] { CreateDefaultCareer() });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result.Data.Single().Locations, Is.Empty);
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_JobTitleURL_IsNull_WhenCareersPageMetadataTitleIsNull()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage(metadataTitle: null) });
        SetupCareerPagesLookup(new[] { CreateDefaultCareer() });

        var result = await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result.Data.Single().JobTitleURL, Is.Null);
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_UsesCurrentLanguageTagForUrlRetrieval()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("fr-fr");
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[] { CreateDefaultCareer() });

        await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        _webPageUrlRetrieverMock.Verify(
            r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                "fr-fr",
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_CallsUrlRetrieverForCareersPageAndEachCareer()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(new[]
        {
            CreateDefaultCareer(webPageItemId: 1, title: "Job 1"),
            CreateDefaultCareer(webPageItemId: 2, title: "Job 2"),
        });

        await _service.GetCareerPaginationResultAsync(
            "en-gb",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        // Once for the careers page itself + once per career
        _webPageUrlRetrieverMock.Verify(
            r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Test]
    public async Task GetCareerPaginationResultAsync_RequestsCareerPagesUsingProvidedLanguageName()
    {
        SetupCareersPagesLookup(new[] { CreateDefaultCareersPage() });
        SetupCareerPagesLookup(Array.Empty<Career>());

        await _service.GetCareerPaginationResultAsync(
            "de-de",
            Guid.NewGuid(),
            new CardsQuery(),
            CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Career>(
                It.Is<RetrievePagesParameters>(p =>
                    p.LanguageName == "de-de" &&
                    p.UseLanguageFallbacks == false &&
                    p.LinkedItemsMaxLevel == 1),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}