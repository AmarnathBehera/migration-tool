using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget;

public class KeyContactsServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IPersonPageService> _personPageServiceMock = null!;
    private Mock<IExternalPeopleHelper> _externalPeopleHelperMock = null!;
    private KeyContactsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _personPageServiceMock = new Mock<IPersonPageService>();
        _externalPeopleHelperMock = new Mock<IExternalPeopleHelper>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-US");

        SetupEmptyPagesAsync<Person>();
        SetupEmptyPagesAsync<ExternalPerson>();
        SetupEmptyPagesAsync<InternalPerson>();

        _service = new KeyContactsService(
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _personPageServiceMock.Object,
            _externalPeopleHelperMock.Object);
    }

    private void SetupEmptyPagesAsync<TPage>() where TPage : class, IWebPageFieldsSource
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<TPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<TPage>());
    }

    private static KeyContactsData CreateDefaultData() => new()
    {
        Title = "Title",
        Description = "Description",
        OrderedPeopleNodeGuids = [],
        OrderedContributorsNodeGuids = [],
        TopMargin = "0px",
        BottomMargin = "0px",
    };

    [Test]
    public async Task GetKeyContactsAsync_MapsScalarDataFieldsToDto()
    {
        var data = new KeyContactsData
        {
            Title = "Key Contacts",
            Description = "A description",
            OrderedPeopleNodeGuids = [],
            OrderedContributorsNodeGuids = [],
            TopMargin = "40px",
            BottomMargin = "20px",
            WidgetID = "key-contacts-1",
        };

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Key Contacts"));
            Assert.That(result.Description, Is.EqualTo("A description"));
            Assert.That(result.TopMargin, Is.EqualTo("40px"));
            Assert.That(result.BottomMargin, Is.EqualTo("20px"));
            Assert.That(result.WidgetID, Is.EqualTo("key-contacts-1"));
        });
    }

    [Test]
    public async Task GetKeyContactsAsync_WithEmptyGuids_ReturnsEmptyItemsAndExternalItems()
    {
        var data = CreateDefaultData();

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Not.Null);
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.ExternalItems, Is.Not.Null);
            Assert.That(result.ExternalItems, Is.Empty);
        });
    }

    [Test]
    public async Task GetKeyContactsAsync_WithSeeAllLink_BuildsLinkItemDto()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = "See all";
        data.SeeAllLinkUrl = "/people";
        data.SeeAllLinkURLTarget = "_blank";

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.SeeAllLink!.Title, Is.EqualTo("See all"));
            Assert.That(result.SeeAllLink!.URL, Is.EqualTo("/people"));
            Assert.That(result.SeeAllLink!.Target, Is.EqualTo("_blank"));
        });
    }

    [Test]
    public async Task GetKeyContactsAsync_WithSeeAllLinkAndNoTarget_DefaultsTargetToSelf()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = "See all";
        data.SeeAllLinkUrl = "/people";
        data.SeeAllLinkURLTarget = null;

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Not.Null);
        Assert.That(result.SeeAllLink!.Target, Is.EqualTo("_self"));
    }

    [Test]
    public async Task GetKeyContactsAsync_WithNullSeeAllLinkText_ReturnsNullSeeAllLink()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = null;
        data.SeeAllLinkUrl = "/people";

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetKeyContactsAsync_WithNullSeeAllLinkUrl_ReturnsNullSeeAllLink()
    {
        var data = CreateDefaultData();
        data.SeeAllLinkText = "See all";
        data.SeeAllLinkUrl = null;

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetKeyContactsAsync_UsesCurrentLanguageTag_FromCultureContext()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("fr-FR");

        RetrievePagesParameters? capturedParameters = null;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedParameters = p)
            .ReturnsAsync(Enumerable.Empty<Person>());

        var data = CreateDefaultData();
        data.OrderedPeopleNodeGuids = [new ContentItemReference { Identifier = Guid.NewGuid() }];

        await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.That(capturedParameters, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedParameters!.LanguageName, Is.EqualTo("fr-FR"));
            Assert.That(capturedParameters!.UseLanguageFallbacks, Is.False);
            Assert.That(capturedParameters!.LinkedItemsMaxLevel, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetKeyContactsAsync_PassesCancellationTokenToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (_, _, _, t) => capturedToken = t)
            .ReturnsAsync(Enumerable.Empty<Person>());

        await _service.GetKeyContactsAsync(CreateDefaultData(), token);

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task GetKeyContactsAsync_QueriesPeople_ExternalContributors_AndInternalContributors()
    {
        var data = CreateDefaultData();

        await _service.GetKeyContactsAsync(data, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<ExternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<InternalPerson>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetKeyContactsAsync_WithNullTitleAndDescription_ReturnsDtoWithNulls()
    {
        var data = CreateDefaultData();
        data.Title = null;
        data.Description = null;

        var result = await _service.GetKeyContactsAsync(data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Description, Is.Null);
        });
    }

    [Test]
    public async Task GetKeyContactsAsync_ReturnsPeopleGridDtoInstance()
    {
        var result = await _service.GetKeyContactsAsync(CreateDefaultData(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<PeopleGridDto>());
    }

    [Test]
    public void Constructor_DoesNotThrow_WithValidArguments()
    {
        Assert.DoesNotThrow(() => new KeyContactsService(
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _personPageServiceMock.Object,
            _externalPeopleHelperMock.Object));
    }
}
