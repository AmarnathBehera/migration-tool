using BDO;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Helpers;

public class PersonHelperTests
{
    private readonly Mock<IContentService> _contentServiceMock;
    private readonly PersonHelper _personHelper;

    public PersonHelperTests()
    {
        _contentServiceMock = new Mock<IContentService>();
        _personHelper = new PersonHelper(_contentServiceMock.Object);
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenPersonFoundByUrl_ReturnsEmailAndFullName()
    {
        var person = new Person
        {
            PersonFirstName = "John",
            PersonSurnamePrefixes = "van",
            PersonLastName = "Doe",
            PersonEmail = "john.doe@example.com"
        };

        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person> { person });

        var result = await _personHelper.GetPersonPageEmailId("Title", "/people/john-doe", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("john.doe@example.com"));
            Assert.That(result.Name, Is.EqualTo("John van Doe"));
        });
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenPersonFoundByUrl_WithoutPrefixes_ReturnsNameWithoutPrefix()
    {
        var person = new Person
        {
            PersonFirstName = "Jane",
            PersonSurnamePrefixes = null,
            PersonLastName = "Smith",
            PersonEmail = "jane.smith@example.com"
        };

        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person> { person });

        var result = await _personHelper.GetPersonPageEmailId("Title", "/people/jane-smith", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("jane.smith@example.com"));
            Assert.That(result.Name, Is.EqualTo("Jane Smith"));
        });
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenNotFoundByUrl_FallsBackToTitleSearch()
    {
        var fallbackPerson = new Person
        {
            PersonFirstName = "Alice",
            PersonSurnamePrefixes = "de",
            PersonLastName = "Vries",
            PersonEmail = "alice.devries@example.com"
        };

        _contentServiceMock
            .SetupSequence(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>())
            .ReturnsAsync(new List<Person> { fallbackPerson });

        var result = await _personHelper.GetPersonPageEmailId("Alice de Vries", "/missing", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("alice.devries@example.com"));
            Assert.That(result.Name, Is.EqualTo("Alice de Vries"));
        });
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenNoMatchInEitherQuery_ReturnsEmptyEmailAndName()
    {
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var result = await _personHelper.GetPersonPageEmailId("Unknown", "/unknown", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(string.Empty));
            Assert.That(result.Name, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenPersonEmailIsNull_ReturnsEmptyEmail()
    {
        var person = new Person
        {
            PersonFirstName = "Bob",
            PersonLastName = "Brown",
            PersonEmail = null
        };

        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person> { person });

        var result = await _personHelper.GetPersonPageEmailId("Title", "/people/bob-brown", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo(string.Empty));
            Assert.That(result.Name, Is.EqualTo("Bob Brown"));
        });
    }

    [Test]
    public async Task GetPersonPageEmailId_WhenFirstNameIsNull_ReturnsNameStartingWithEmpty()
    {
        var person = new Person
        {
            PersonFirstName = null,
            PersonSurnamePrefixes = "van der",
            PersonLastName = "Berg",
            PersonEmail = "vdb@example.com"
        };

        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person> { person });

        var result = await _personHelper.GetPersonPageEmailId("Title", "/people/berg", "en-US");

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("vdb@example.com"));
            Assert.That(result.Name, Is.EqualTo(" van der Berg"));
        });
    }
}
