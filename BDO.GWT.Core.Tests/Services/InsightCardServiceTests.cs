using System.Reflection;
using System.Runtime.Serialization;
using BDO;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCardServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private InsightCardService _service = null!;

    private RetrievePagesParameters? _capturedRetrievePagesParameters;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _cardsServiceMock = new Mock<ICardsService>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<Insight>()))
            .Returns<Insight>(x => $"/insights/{x.MetadataTitle}");

        _service = new InsightCardService(
            _contentRetrieverMock.Object,
            _cardsServiceMock.Object,
            _webPageUrlResolverMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var filters = new HashSet<string> { "businessLine" };
        var expected = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "businessLine",
                Options = Array.Empty<OptionDto>()
            }
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filters, "/insights", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(filters, "/insights", CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, "/insights", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void GetFilteredItemsAsync_Throws_WhenParentPathIsNullOrEmpty(string? parentPath)
    {
        var query = CreateCardsQuery();

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.GetFilteredItemsAsync(
                query,
                parentPath!,
                null,
                _ => false,
                CancellationToken.None));
    }

    [Test]
    public async Task GetFilteredItemsAsync_CallsRetriever_WithChildrenPath_AndLinkedItemsMaxLevel()
    {
        SetupRetrievePages();

        var query = CreateCardsQuery();

        await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_capturedRetrievePagesParameters, Is.Not.Null);
            Assert.That(_capturedRetrievePagesParameters!.LinkedItemsMaxLevel, Is.EqualTo(4));
            Assert.That(_capturedRetrievePagesParameters.PathMatch, Is.Not.Null);
        });

        _contentRetrieverMock.Verify(
            x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_InvokesDecorator_WhenRetrieverExecutesQueryAction()
    {
        var decoratorCalled = false;

        SetupRetrievePages(
            executeQueryAction: true,
            CreateInsight("Insight 1"));

        var query = CreateCardsQuery();

        await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            _ => decoratorCalled = true,
            _ => false,
            CancellationToken.None);

        Assert.That(decoratorCalled, Is.True);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReturnsPagedItems_OrderedByFeaturedFirst()
    {
        var insight1 = CreateInsight("Insight 1", new DateTime(2026, 01, 01));
        var insight2 = CreateInsight("Insight 2", new DateTime(2026, 01, 02));
        var featuredInsight = CreateInsight("Featured Insight", new DateTime(2025, 01, 01));

        SetupRetrievePages(insight1, insight2, featuredInsight);

        var query = CreateCardsQuery(
            currentPage: 1,
            pageSize: 2);

        var result = await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            x => x.MetadataTitle == "Featured Insight",
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(3));
            Assert.That(result.Data, Has.Length.EqualTo(2));
            Assert.That(result.Data[0].Title, Is.EqualTo("Featured Insight"));
            Assert.That(result.Data[0].IsFeatured, Is.True);
            Assert.That(result.Data[1].Title, Is.EqualTo("Insight 2"));
            Assert.That(result.Data[1].IsFeatured, Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesPagination()
    {
        var insight1 = CreateInsight("Insight 1", new DateTime(2026, 01, 03));
        var insight2 = CreateInsight("Insight 2", new DateTime(2026, 01, 02));
        var insight3 = CreateInsight("Insight 3", new DateTime(2026, 01, 01));

        SetupRetrievePages(insight1, insight2, insight3);

        var query = CreateCardsQuery(
            currentPage: 2,
            pageSize: 1);

        var result = await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(3));
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Title, Is.EqualTo("Insight 2"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesBusinessLineFilter()
    {
        var matchingInsight = CreateInsight("Audit Insight");
        var nonMatchingInsight = CreateInsight("Tax Insight");

        SetLinkedItemNames(
            matchingInsight,
            nameof(Insight.MetadataBusinessLine),
            "Audit");

        SetLinkedItemNames(
            nonMatchingInsight,
            nameof(Insight.MetadataBusinessLine),
            "Tax");

        SetupRetrievePages(matchingInsight, nonMatchingInsight);

        var query = CreateCardsQuery(
            filters: new Dictionary<string, string>
            {
                [FilterParameterName.BusinessLine] = "Audit"
            });

        var result = await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Title, Is.EqualTo("Audit Insight"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_MapsCardDto()
    {
        var insight = CreateInsight(
            title: "Mapped Insight",
            publishedDate: new DateTime(2026, 02, 01),
            description: "<p>This is test description</p>",
            comingSoon: true);

        SetupRetrievePages(insight);

        var query = CreateCardsQuery();

        var result = await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            _ => true,
            CancellationToken.None);

        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Mapped Insight"));
            Assert.That(card.Description, Is.EqualTo("This is test description"));
            Assert.That(card.isComingSoon, Is.True);
            Assert.That(card.IsFeatured, Is.True);
            Assert.That(card.ReadMoreLink.Title, Is.EqualTo(ResourceStringKey.LiveSite.ReadMore));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/insights/Mapped Insight"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_ConvertsRelativeAssetUrl_ToAbsoluteUrl()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("www.bdo.test");

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        var insight = CreateInsight("Insight With Image");

        SetCopyInsightImage(
            insight,
            "~/getContentAsset/test-image.jpg",
            "Image alt text");

        SetupRetrievePages(insight);

        var query = CreateCardsQuery();

        var result = await _service.GetFilteredItemsAsync(
            query,
            "/insights",
            null,
            _ => false,
            CancellationToken.None);

        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.ImageURL, Is.EqualTo("https://www.bdo.test/getContentAsset/test-image.jpg"));
            Assert.That(card.ImageAltText, Is.EqualTo("Image alt text"));
        });
    }

    private void SetupRetrievePages(
        params Insight[] pages)
    {
        SetupRetrievePages(false, pages);
    }

    private void SetupRetrievePages(
        bool executeQueryAction,
        params Insight[] pages)
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, Func<IContentQueryDataContainer, Insight, Task<Insight>>, CancellationToken>(
                (parameters, queryAction, _, _, _) =>
                {
                    _capturedRetrievePagesParameters = parameters;

                    if (executeQueryAction)
                    {
                        try
                        {
                            var queryParameters = CreateRetrievePagesQueryParameters();
                            queryAction(queryParameters);
                        }
                        catch (NullReferenceException)
                        {
                            InvokeDecoratorFromQueryAction(queryAction);
                        }
                    }
                })
            .ReturnsAsync(pages);
    }

    private static CardsQuery CreateCardsQuery(
        int? currentPage = 1,
        int? pageSize = 10,
        IDictionary<string, string>? filters = null)
    {
        return new CardsQuery
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            Filters = filters ?? new Dictionary<string, string>()
        };
    }

    private static Insight CreateInsight(
        string title,
        DateTime? publishedDate = null,
        string description = "<p>Description</p>",
        bool comingSoon = false)
    {
        var insight = new Insight
        {
            MetadataTitle = title,
            MetadataDescription = description,
            ComingSoon = comingSoon
        };

        SetEmptyEnumerableProperty(insight, nameof(Insight.CopyInsight));
        SetSystemFieldDates(insight, publishedDate ?? DateTime.UtcNow);

        return insight;
    }

    private static RetrievePagesQueryParameters CreateRetrievePagesQueryParameters()
    {
        RetrievePagesQueryParameters? instance = null;

        try
        {
            instance = Activator.CreateInstance(typeof(RetrievePagesQueryParameters), nonPublic: true)
                as RetrievePagesQueryParameters;
        }
        catch
        {
        }

        if (instance is not null)
        {
            return instance;
        }

#pragma warning disable SYSLIB0050
        return (RetrievePagesQueryParameters)FormatterServices.GetUninitializedObject(typeof(RetrievePagesQueryParameters));
#pragma warning restore SYSLIB0050
    }

    private static void InvokeDecoratorFromQueryAction(Action<RetrievePagesQueryParameters> queryAction)
    {
        var target = queryAction.Target;
        if (target is null)
        {
            return;
        }

        var decoratorField = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(field => field.FieldType == typeof(Action<RetrievePagesQueryParameters>));

        var decorator = decoratorField?.GetValue(target) as Action<RetrievePagesQueryParameters>;
        decorator?.Invoke(null!);
    }

    private static void SetSystemFieldDates(Insight insight, DateTime publishedDate)
    {
        var systemFieldsProperty = typeof(Insight).GetProperty(nameof(Insight.SystemFields));
        if (systemFieldsProperty is null)
        {
            return;
        }

        var systemFields = systemFieldsProperty.GetValue(insight);

        if (systemFields is null)
        {
            systemFields = CreateObject(systemFieldsProperty.PropertyType);
            SetPropertyValue(insight, nameof(Insight.SystemFields), systemFields);
        }

        SetPropertyValue(systemFields, "ContentItemCommonDataLastPublishedWhen", publishedDate);
        SetPropertyValue(systemFields, "ContentItemCommonDataFirstPublishedWhen", publishedDate.AddDays(-1));
    }

    private static void SetLinkedItemNames(
        Insight insight,
        string propertyName,
        params string[] names)
    {
        var property = typeof(Insight).GetProperty(propertyName);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found on Insight.");
        }

        var elementType = GetEnumerableElementType(property.PropertyType);
        var array = Array.CreateInstance(elementType, names.Length);

        for (var i = 0; i < names.Length; i++)
        {
            var item = CreateObject(elementType);

            SetPropertyValue(item, "Name", names[i]);
            SetPropertyValue(item, "MetadataTitle", names[i]);

            var systemFieldsProperty = elementType.GetProperty("SystemFields");
            if (systemFieldsProperty is not null)
            {
                var systemFields = systemFieldsProperty.GetValue(item);

                if (systemFields is null)
                {
                    systemFields = CreateObject(systemFieldsProperty.PropertyType);
                    SetPropertyValue(item, "SystemFields", systemFields);
                }

                SetPropertyValue(systemFields, "ContentItemName", names[i]);
            }

            array.SetValue(item, i);
        }

        SetPropertyValue(insight, propertyName, array);
    }

    private static void SetCopyInsightImage(
        Insight insight,
        string imageUrl,
        string imageAltText)
    {
        var copyInsightProperty = typeof(Insight).GetProperty(nameof(Insight.CopyInsight));
        if (copyInsightProperty is null)
        {
            throw new InvalidOperationException("CopyInsight property was not found on Insight.");
        }

        var copyInsightType = GetEnumerableElementType(copyInsightProperty.PropertyType);
        var copyInsight = CreateObject(copyInsightType);

        SetPropertyValue(copyInsight, "ImageFileAltText", imageAltText);

        var imageFileProperty = copyInsightType.GetProperty("ImageFile");
        if (imageFileProperty is null)
        {
            throw new InvalidOperationException("ImageFile property was not found on CopyInsight item.");
        }

        var imageFileType = GetEnumerableElementType(imageFileProperty.PropertyType);
        var imageFile = CreateObject(imageFileType);

        var assetProperty = imageFileType.GetProperty("ContentAttachmentAsset");
        if (assetProperty is null)
        {
            throw new InvalidOperationException("ContentAttachmentAsset property was not found on ImageFile item.");
        }

        var asset = CreateObject(assetProperty.PropertyType);
        SetPropertyValue(asset, "Url", imageUrl);
        SetPropertyValue(imageFile, "ContentAttachmentAsset", asset);

        var imageFileArray = Array.CreateInstance(imageFileType, 1);
        imageFileArray.SetValue(imageFile, 0);

        SetPropertyValue(copyInsight, "ImageFile", imageFileArray);

        var copyInsightArray = Array.CreateInstance(copyInsightType, 1);
        copyInsightArray.SetValue(copyInsight, 0);

        SetPropertyValue(insight, nameof(Insight.CopyInsight), copyInsightArray);
    }

    private static void SetEmptyEnumerableProperty(
        object target,
        string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property is null)
        {
            return;
        }

        var elementType = GetEnumerableElementType(property.PropertyType);
        var emptyArray = Array.CreateInstance(elementType, 0);

        SetPropertyValue(target, propertyName, emptyArray);
    }

    private static Type GetEnumerableElementType(Type enumerableType)
    {
        if (enumerableType.IsArray)
        {
            return enumerableType.GetElementType()!;
        }

        if (enumerableType.IsGenericType &&
            enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return enumerableType.GetGenericArguments()[0];
        }

        var enumerableInterface = enumerableType
            .GetInterfaces()
            .FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface is null)
        {
            throw new InvalidOperationException($"Type '{enumerableType.Name}' is not IEnumerable<T>.");
        }

        return enumerableInterface.GetGenericArguments()[0];
    }

    private static object CreateObject(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);

        if (constructor is not null)
        {
            return Activator.CreateInstance(type)!;
        }

#pragma warning disable SYSLIB0050
        return FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
    }

    private static void SetPropertyValue(
        object target,
        string propertyName,
        object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var backingField = target.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField is not null)
        {
            backingField.SetValue(target, value);
        }
    }
}