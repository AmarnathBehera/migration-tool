using BDO.GWT.Core;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Core;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class PersonPageServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private PersonPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _settingServiceMock = new Mock<ISettingService>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");

        _service = new PersonPageService(
            _httpClientFactoryMock.Object,
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _settingServiceMock.Object
          );
    }

    [Test]
    public async Task GetPersonPageDtoAsync_DisplayName_IncludesSurnamePrefixes()
    {
        var person = new Person
        {
            PersonFirstName = "Jan",
            PersonLastName = "Berg",
            PersonSurnamePrefixes = "van den"
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage>());

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<LocationOffice>());

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.DisplayName, Is.EqualTo("Jan van den Berg"));
    }

    [Test]
    public async Task GetPersonPageDtoAsync_EmailIsNull_WhenSectionPageDisablesEmail()
    {
        var person = new Person
        {
            PersonFirstName = "Jane",
            PersonLastName = "Smith",
            PersonEmail = "jane@bdo.com"
        };

        var sectionPage = new SectionPeoplePage
        {
            DisplayEmailAddressInPersonPage = false
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage> { sectionPage });

        _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<LocationOffice>)null!);

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.Email, Is.Null);
    }

    [Test]
    public async Task GeneratePersonVCardAsync_ReturnsNull_WhenPersonNotFound()
    {
        var personGuid = Guid.NewGuid();

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var result = await _service.GeneratePersonVCardAsync(personGuid, null, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GeneratePersonVCardAsync_ReturnsNull_WhenVCardDisabled()
    {
        var personGuid = Guid.NewGuid();
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            EnableVCard = false
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person> { person });

        var result = await _service.GeneratePersonVCardAsync(personGuid, null, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetPersonDisplayNameAsync_WithoutPrefix_ReturnsFirstAndLastName()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonSurnamePrefixes = ""
        };

        var result = PersonPageService.GetPersonDisplayNameAsync(person);

        Assert.That(result, Is.EqualTo("John Doe"));
    }

    [Test]
    public void GetPersonDisplayNameAsync_WithPrefix_IncludesPrefix()
    {
        var person = new Person
        {
            PersonFirstName = "Jan",
            PersonLastName = "Berg",
            PersonSurnamePrefixes = "van den"
        };

        var result = PersonPageService.GetPersonDisplayNameAsync(person);

        Assert.That(result, Is.EqualTo("Jan van den Berg"));
    }
}
