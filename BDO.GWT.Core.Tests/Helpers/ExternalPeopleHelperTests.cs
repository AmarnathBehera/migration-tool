//using BDO.GWT.Core;
//using BDO.GWT.Core.Helpers;
//using BDO.GWT.Core.Services;
//using CMS.Websites;
//using Kentico.Content.Web.Mvc;
//using Moq;

//namespace BDO.GWT.Core.Tests.Helpers;

//public class ExternalPeopleHelperTests
//{
//    private Mock<IContentService> _contentServiceMock = null!;
//    private Mock<ISettingService> _settingServiceMock = null!;
//    private ExternalPeopleHelper _helper = null!;

//    [SetUp]
//    public void SetUp()
//    {
//        _contentServiceMock = new Mock<IContentService>();
//        _settingServiceMock = new Mock<ISettingService>();
//        _helper = new ExternalPeopleHelper(_contentServiceMock.Object, _settingServiceMock.Object);
//    }

//    private void SetupExternalPeople(IEnumerable<ExternalPerson> people)
//    {
//        _contentServiceMock
//            .Setup(x => x.GetPagesAsync<ExternalPerson>(
//                It.IsAny<RetrievePagesParameters>(),
//                It.IsAny<Action<RetrievePagesQueryParameters>>(),
//                It.IsAny<RetrievalCacheSettings>(),
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(people);
//    }

//    private void SetupUsePersonPageTitle(bool? value)
//    {
//        if (value is null)
//        {
//            _settingServiceMock
//                .Setup(x => x.GetUsePersonPageTitleInfo())
//                .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);
//        }
//        else
//        {
//            var info = new TestOurPeopleInfo(value.Value);
//            _settingServiceMock
//                .Setup(x => x.GetUsePersonPageTitleInfo())
//                .ReturnsAsync(new List<WebChannelSnippetInfo> { info });
//        }
//    }

//    private sealed class TestOurPeopleInfo : WebChannelSnippetInfo
//    {
//        private readonly bool _usePageTitle;

//        public TestOurPeopleInfo(bool usePageTitle)
//        {
//            _usePageTitle = usePageTitle;
//        }

//        public override bool UsePageTitleToDisplayPersonName
//        {
//            get => _usePageTitle;
//            set { }
//        }
//    }

//    [Test]
//    public async Task GetDisplayNameAsync_WhenUsePageTitleTrue_ReturnsMetadataTitle()
//    {
//        SetupUsePersonPageTitle(true);

//        var person = new ExternalPerson
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            SurnamePrefixes = "van",
//            MetadataTitle = "Mr. John Doe"
//        };

//        var result = await _helper.GetDisplayNameAsync(person);

//        Assert.That(result, Is.EqualTo("Mr. John Doe"));
//    }

//    [Test]
//    public async Task GetDisplayNameAsync_WhenUsePageTitleFalse_AndNoPrefix_ReturnsFirstAndLastName()
//    {
//        SetupUsePersonPageTitle(false);

//        var person = new ExternalPerson
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            SurnamePrefixes = string.Empty,
//            MetadataTitle = "Ignored"
//        };

//        var result = await _helper.GetDisplayNameAsync(person);

//        Assert.That(result, Is.EqualTo("John Doe"));
//    }

//    [Test]
//    public async Task GetDisplayNameAsync_WhenUsePageTitleFalse_AndPrefixPresent_IncludesPrefix()
//    {
//        SetupUsePersonPageTitle(false);

//        var person = new ExternalPerson
//        {
//            FirstName = "Jan",
//            LastName = "Berg",
//            SurnamePrefixes = "van den"
//        };

//        var result = await _helper.GetDisplayNameAsync(person);

//        Assert.That(result, Is.EqualTo("Jan van den Berg"));
//    }

//    [Test]
//    public async Task GetDisplayNameAsync_WhenSettingNull_DefaultsToFirstAndLastName()
//    {
//        SetupUsePersonPageTitle(null);

//        var person = new ExternalPerson
//        {
//            FirstName = "Jane",
//            LastName = "Smith",
//            SurnamePrefixes = null!
//        };

//        var result = await _helper.GetDisplayNameAsync(person);

//        Assert.That(result, Is.EqualTo("Jane Smith"));
//    }

//    [Test]
//    public async Task GetExternalPeopleDisplayNames_ReturnsDisplayNamesInOrder()
//    {
//        SetupUsePersonPageTitle(false);
//        SetupExternalPeople(new[]
//        {
//            new ExternalPerson { FirstName = "John", LastName = "Doe", SurnamePrefixes = "" },
//            new ExternalPerson { FirstName = "Jan", LastName = "Berg", SurnamePrefixes = "van den" }
//        });

//        var result = await _helper.GetExternalPeopleDisplayNames(new List<Guid> { Guid.NewGuid() }, CancellationToken.None);

//        Assert.That(result, Is.EqualTo(new List<string> { "John Doe", "Jan van den Berg" }));
//    }

//    [Test]
//    public async Task GetExternalPeopleDisplayNames_WhenNoPeople_ReturnsEmptyList()
//    {
//        SetupUsePersonPageTitle(false);
//        SetupExternalPeople(Array.Empty<ExternalPerson>());

//        var result = await _helper.GetExternalPeopleDisplayNames(new List<Guid>(), CancellationToken.None);

//        Assert.That(result, Is.Empty);
//    }

//    [Test]
//    public async Task GetExternalPeopleDisplayNames_WhenUsePageTitleTrue_ReturnsMetadataTitles()
//    {
//        SetupUsePersonPageTitle(true);
//        SetupExternalPeople(new[]
//        {
//            new ExternalPerson { FirstName = "John", LastName = "Doe", MetadataTitle = "Title A" },
//            new ExternalPerson { FirstName = "Jane", LastName = "Smith", MetadataTitle = "Title B" }
//        });

//        var result = await _helper.GetExternalPeopleDisplayNames(new List<Guid> { Guid.NewGuid() }, CancellationToken.None);

//        Assert.That(result, Is.EqualTo(new List<string> { "Title A", "Title B" }));
//    }

//    [Test]
//    public async Task GetSidebarExternalPeopleDtosAsync_WhenNoPeople_ReturnsEmptyArray()
//    {
//        SetupUsePersonPageTitle(false);
//        SetupExternalPeople(Array.Empty<ExternalPerson>());

//        var result = await _helper.GetSidebarExternalPeopleDtosAsync(new List<Guid>(), CancellationToken.None);

//        Assert.That(result, Is.Empty);
//    }

//    [Test]
//    public async Task GetSidebarExternalPeopleDtosAsync_WhenPersonHasNoImageOrUrl_MapsBasicFields()
//    {
//        SetupUsePersonPageTitle(false);
//        var personGuid = Guid.NewGuid();
//        SetupExternalPeople(new[]
//        {
//            new ExternalPerson
//            {
//                FirstName = "John",
//                LastName = "Doe",
//                SurnamePrefixes = "",
//                JobTitle = "Engineer",
//                ImageFile = Enumerable.Empty<CMS.ContentEngine.IContentItemFieldsSource>().OfType<ContentAttachment>(),
//                Url = string.Empty,
//                SystemFields = new WebPageFields { ContentItemGUID = personGuid }
//            }
//        });

//        var result = await _helper.GetSidebarExternalPeopleDtosAsync(new List<Guid> { personGuid }, CancellationToken.None);

//        Assert.That(result, Has.Length.EqualTo(1));
//        Assert.Multiple(() =>
//        {
//            Assert.That(result[0].FullName, Is.EqualTo("John Doe"));
//            Assert.That(result[0].JobTitle, Is.EqualTo("Engineer"));
//            Assert.That(result[0].Image, Is.Null);
//            Assert.That(result[0].Url, Is.Null);
//        });
//    }

//    [Test]
//    public async Task GetSidebarExternalPeopleDtosAsync_WhenUrlsAllEmptyGuid_DoesNotCallGetItemsByGuid()
//    {
//        SetupUsePersonPageTitle(false);
//        var personGuid = Guid.NewGuid();
//        SetupExternalPeople(new[]
//        {
//            new ExternalPerson
//            {
//                FirstName = "Jane",
//                LastName = "Smith",
//                JobTitle = "Manager",
//                Url = string.Empty,
//                ImageFile = Enumerable.Empty<ContentAttachment>(),
//                SystemFields = new WebPageFields { ContentItemGUID = personGuid }
//            }
//        });

//        var result = await _helper.GetSidebarExternalPeopleDtosAsync(new List<Guid> { personGuid }, CancellationToken.None);

//        Assert.That(result, Has.Length.EqualTo(1));
//        Assert.That(result[0].Url, Is.Null);
//    }

//    [Test]
//    public async Task GetSidebarExternalPeopleDtosAsync_WhenUrlReferencePresent_CallsGetItemsByGuidAndMapsUrl()
//    {
//        SetupUsePersonPageTitle(false);
//        var personGuid = Guid.NewGuid();
//        SetupExternalPeople(new[]
//        {
//            new ExternalPerson
//            {
//                FirstName = "Alice",
//                LastName = "Wonder",
//                JobTitle = "Director",
//                Url = "/people/alice",
//                ImageFile = Enumerable.Empty<ContentAttachment>(),
//                SystemFields = new WebPageFields { ContentItemGUID = personGuid }
//            }
//        });

//        _contentServiceMock
//            .Setup(x => x.EnsureAbsoluteUrl("/people/alice"))
//            .Returns("/people/alice");

//        var result = await _helper.GetSidebarExternalPeopleDtosAsync(new List<Guid> { personGuid }, CancellationToken.None);

//        Assert.That(result, Has.Length.EqualTo(1));
//        Assert.That(result[0].Url, Is.EqualTo("/people/alice"));

//    }
//}
