using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class AuthoredContentServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private AuthoredContentService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock
            .Setup(c => c.CurrentLanguageTag)
            .Returns("en-US");

        _service = new AuthoredContentService(
            _contentServiceMock.Object,
            _cultureContextMock.Object);
    }

    [Test]
    public async Task GetAuthoredContentDtoAsync_ReturnsNull_WhenParentSectionNotFound()
    {
        var person = new Person
        {
            SystemFields = new WebPageFields()
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SectionPeoplePage>());

        var result = await _service.GetAuthoredContentDtoAsync(person, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAuthoredContentDtoAsync_ReturnsNull_WhenShowAuthoredInsightsIsFalse()
    {
        var person = new Person
        {
            SystemFields = new WebPageFields()
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new SectionPeoplePage { ShowAuthoredInsights = false } });

        var result = await _service.GetAuthoredContentDtoAsync(person, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAuthoredContentDtoAsync_ReturnsDtoWithEmptyItems_WhenEnabledAndNoDisplayedTypes()
    {
        var person = new Person
        {
            SystemFields = new WebPageFields()
        };

        RetrievePagesParameters? capturedParameters = null;

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (parameters, _, _, _) => capturedParameters = parameters)
            .ReturnsAsync(new[]
            {
                new SectionPeoplePage
                {
                    ShowAuthoredInsights = true,
                    AuthoredInsightsTitle = "Authored insights",
                    AuthoredInsightsDescription = "Section description",
                    DisplayedAuthoredContentPageTypes = string.Empty
                }
            });

        var result = await _service.GetAuthoredContentDtoAsync(person, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Authored insights"));
            Assert.That(result.Description, Is.EqualTo("Section description"));
            Assert.That(result.Items, Is.Empty);
            Assert.That(capturedParameters, Is.Not.Null);
            Assert.That(capturedParameters!.LanguageName, Is.EqualTo("en-US"));
            Assert.That(capturedParameters.UseLanguageFallbacks, Is.False);
        });
    }

    [Test]
    public async Task GetAuthoredContentDtoAsync_RequestsOnlyConfiguredQueryableTypes()
    {
        var person = new Person
        {
            SystemFields = new WebPageFields()
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SectionPeoplePage
                {
                    ShowAuthoredInsights = true,
                    DisplayedAuthoredContentPageTypes = $"{nameof(NewsArticle)}|{nameof(Deal)}"
                }
            });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<NewsArticle>());

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Deal>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Deal>());

        await _service.GetAuthoredContentDtoAsync(person, CancellationToken.None);

        _contentServiceMock.Verify(s => s.GetPagesAsync<NewsArticle>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _contentServiceMock.Verify(s => s.GetPagesAsync<Deal>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _contentServiceMock.Verify(s => s.GetPagesAsync<BlogPost>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}