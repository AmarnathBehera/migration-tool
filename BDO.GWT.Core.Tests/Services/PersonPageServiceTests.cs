using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class PersonPageServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IAuthoredContentService> _authoredContentServiceMock = null!;
    private Mock<ISettingService> _settingServiceMock = null!;
    private PersonPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _settingServiceMock = new Mock<ISettingService>();
        _authoredContentServiceMock = new Mock<IAuthoredContentService>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en");

        _service = new PersonPageService(
            _httpClientFactoryMock.Object,
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _settingServiceMock.Object,
            _authoredContentServiceMock.Object
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

    [Test]
    public void GetPersonDisplayNameAsync_WithNullPrefix_ReturnsFirstAndLastName()
    {
        var person = new Person
        {
            PersonFirstName = "Alice",
            PersonLastName = "Johnson",
            PersonSurnamePrefixes = null!
        };

        var result = PersonPageService.GetPersonDisplayNameAsync(person);

        Assert.That(result, Is.EqualTo("Alice Johnson"));
    }

    [Test]
    public async Task GetPersonPageDtoAsync_EmailIsSet_WhenSectionPageEnablesEmail()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonEmail = "john@bdo.com"
        };

        var sectionPage = new SectionPeoplePage
        {
            DisplayEmailAddressInPersonPage = true
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage> { sectionPage });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<LocationOffice>());

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.Email, Is.EqualTo("john@bdo.com"));
    }

    [Test]
    public async Task GetPersonPageDtoAsync_VCardUrl_IsNull_WhenVCardDisabled()
    {
        var person = new Person
        {
            PersonFirstName = "Jane",
            PersonLastName = "Doe",
            EnableVCard = false
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

        Assert.That(result.VCardUrl, Is.Null);
    }

    [Test]
    public async Task GetPersonPageDtoAsync_VCardUrl_IsSet_WhenVCardEnabled()
    {
        var contentItemGuid = Guid.NewGuid();
        var person = new Person
        {
            PersonFirstName = "Jane",
            PersonLastName = "Doe",
            EnableVCard = true,
            SystemFields = new WebPageFields { ContentItemGUID = contentItemGuid }
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

        Assert.That(result.VCardUrl, Is.EqualTo($"/vcard/{contentItemGuid}"));
    }

    [Test]
    public async Task GetPersonPageDtoAsync_SetsJobTitle()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonJobTitle = "Senior Manager"
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

        Assert.That(result.JobTitle, Is.EqualTo("Senior Manager"));
    }

    [Test]
    public async Task GetPersonPageDtoAsync_SetsSocialMediaUrls()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            Facebook = "https://facebook.com/johndoe",
            Twitter = "https://twitter.com/johndoe",
            LinkedIn = "https://linkedin.com/in/johndoe"
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

        Assert.Multiple(() =>
        {
            Assert.That(result.FacebookUrl, Is.EqualTo("https://facebook.com/johndoe"));
            Assert.That(result.TwitterUrl, Is.EqualTo("https://twitter.com/johndoe"));
            Assert.That(result.LinkedInUrl, Is.EqualTo("https://linkedin.com/in/johndoe"));
        });
    }

    [Test]
    public async Task GetPersonPageDtoAsync_SetsPhoneNumbers()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonPhone = "+1-555-0100",
            PersonMobilePhone = "+1-555-0101",
            PersonDirectDialNumber = "+1-555-0102"
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

        Assert.Multiple(() =>
        {
            Assert.That(result.OfficePhoneNumber, Is.EqualTo("+1-555-0100"));
            Assert.That(result.MobilePhoneNumber, Is.EqualTo("+1-555-0101"));
            Assert.That(result.DirectDialNumber, Is.EqualTo("+1-555-0102"));
        });
    }

    [Test]
    public async Task GetPersonPageDtoAsync_SetsEnableForm_FromPerson()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            EnableForm = true
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

        Assert.That(result.EnableForm, Is.True);
    }

    [Test]
    public async Task GetPersonPageDtoAsync_SetsParentEnableForm_FromSectionPage()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe"
        };

        var sectionPage = new SectionPeoplePage
        {
            EnableForm = true
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage> { sectionPage });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<LocationOffice>());

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.ParentEnableForm, Is.True);
    }

    [Test]
    public async Task GetPersonPageDtoAsync_DisplayFullEmailAddress_DefaultsFalse_WhenNoSectionPage()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe"
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

        Assert.That(result.DisplayFullEmailAddress, Is.False);
    }

    [Test]
    public async Task GetPersonPageDtoAsync_DisplayFullEmailAddress_SetFromSectionPage()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe"
        };

        var sectionPage = new SectionPeoplePage
        {
            DisplayFullEmailAddress = true,
            DisplayEmailAddressInPersonPage = true
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage> { sectionPage });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<LocationOffice>());

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.DisplayFullEmailAddress, Is.True);
    }

    [Test]
    public async Task GetPersonPageDtoAsync_Credentials_ReturnsNonEmptyCredentialNames()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonCredentials = new[]
            {
                new Credentials { Name = "CPA" },
                new Credentials { Name = "" },
                new Credentials { Name = "MBA" }
            }
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

        var credentials = result.Credentials.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(credentials, Has.Count.EqualTo(2));
            Assert.That(credentials, Does.Contain("CPA"));
            Assert.That(credentials, Does.Contain("MBA"));
            Assert.That(credentials, Does.Not.Contain(""));
        });
    }

    [Test]
    public async Task GetPersonPageDtoAsync_CallsAuthoredContentService()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe"
        };

        var expectedAuthoredContent = new AuthoredContentDto
        {
            Title = "Authored Content Title"
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

        _authoredContentServiceMock
            .Setup(a => a.GetAuthoredContentDtoAsync(person, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuthoredContent);

        var result = await _service.GetPersonPageDtoAsync(person, CancellationToken.None);

        Assert.That(result.AuthoredContent, Is.Not.Null);
        Assert.That(result.AuthoredContent!.Title, Is.EqualTo("Authored Content Title"));

        _authoredContentServiceMock.Verify(
            a => a.GetAuthoredContentDtoAsync(person, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task GetPersonPageDtoAsync_WhenCredentialsNull_ReturnsEmptyCollection()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            PersonCredentials = null
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetPersonPageDtoAsync(
            person,
            CancellationToken.None);

        Assert.That(result.Credentials, Is.Empty);
    }
    [Test]
    public async Task GetPersonPageDtoAsync_WhenNoAuthoredContent_ReturnsNull()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe"
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _authoredContentServiceMock
            .Setup(x => x.GetAuthoredContentDtoAsync(
                person,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthoredContentDto?)null);

        var result = await _service.GetPersonPageDtoAsync(
            person,
            CancellationToken.None);

        Assert.That(result.AuthoredContent, Is.Null);
    }
    [Test]
    public async Task GetPersonPageDtoAsync_WhenNoOfficeLocations_ReturnsEmptyCollection()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            MetadataOfficeLocation = []
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationCity>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetPersonPageDtoAsync(
            person,
            CancellationToken.None);

        Assert.That(result.OfficeLocations, Is.Empty);
    }
    [Test]
    public async Task GetPersonPageDtoAsync_CallsLocationCityLookup()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            MetadataOfficeLocation = []
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationCity>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.GetPersonPageDtoAsync(
            person,
            CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<LocationCity>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task GetPersonPageDtoAsync_CallsLocationOfficeLookup()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonLastName = "Doe",
            MetadataOfficeLocation = []
        };

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<LocationCity>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.GetPersonPageDtoAsync(
            person,
            CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
