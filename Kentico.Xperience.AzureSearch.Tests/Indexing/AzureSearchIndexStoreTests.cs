using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

/// <summary>
/// Tests for <see cref="AzureSearchIndexStore"/>.
/// </summary>
/// <remarks>
/// <see cref="AzureSearchIndexStore"/> is a process-wide singleton, so every test resets its state in
/// <see cref="SetUp"/> and <see cref="TearDown"/> to remain independent of execution order.
/// </remarks>
[TestFixture]
[Category("Unit")]
internal class AzureSearchIndexStoreTests
{
    private const string INDEX_NAME = "test-index";
    private const string OTHER_INDEX_NAME = "other-index";
    private const string CHANNEL_NAME = "test-channel";
    private const string STRATEGY_NAME = "TestStrategy";


    [SetUp]
    public void SetUp() => AzureSearchIndexStore.Instance.SetIndices([]);


    [TearDown]
    public void TearDown() => AzureSearchIndexStore.Instance.SetIndices([]);


    private static AzureSearchConfigurationModel CreateConfiguration(
        string indexName = INDEX_NAME,
        int id = 1,
        IEnumerable<string>? languageNames = null) => new()
        {
            Id = id,
            IndexName = indexName,
            ChannelName = CHANNEL_NAME,
            StrategyName = STRATEGY_NAME,
            LanguageNames = languageNames ?? ["en"],
            Paths = [],
            ReusableContentTypeNames = []
        };


    private static AzureSearchIndex CreateIndex(string indexName = INDEX_NAME, int id = 1) =>
        new(CreateConfiguration(indexName, id), StrategyStorage.Strategies);


    #region Instance

    [Test]
    public void Instance_ReturnsSameSingletonInstance()
    {
        var first = AzureSearchIndexStore.Instance;
        var second = AzureSearchIndexStore.Instance;

        Assert.That(first, Is.SameAs(second));
    }

    #endregion

    #region GetAllIndices

    [Test]
    public void GetAllIndices_WhenNoIndicesRegistered_ReturnsEmpty()
    {
        Assert.That(AzureSearchIndexStore.Instance.GetAllIndices(), Is.Empty);
    }


    [Test]
    public void GetAllIndices_ReturnsAllRegisteredIndices()
    {
        AzureSearchIndexStore.Instance.SetIndices(
        [
            CreateConfiguration(INDEX_NAME, id: 1),
            CreateConfiguration(OTHER_INDEX_NAME, id: 2)
        ]);

        var result = AzureSearchIndexStore.Instance.GetAllIndices().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(x => x.IndexName), Is.EquivalentTo(new[] { INDEX_NAME, OTHER_INDEX_NAME }));
        });
    }

    #endregion

    #region GetIndex(string)

    [Test]
    public void GetIndexByName_WithRegisteredIndex_ReturnsIndex()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        var result = AzureSearchIndexStore.Instance.GetIndex(INDEX_NAME);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IndexName, Is.EqualTo(INDEX_NAME));
    }


    [Test]
    public void GetIndexByName_IsCaseInsensitive()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        var result = AzureSearchIndexStore.Instance.GetIndex(INDEX_NAME.ToUpperInvariant());

        Assert.That(result, Is.Not.Null);
    }


    [Test]
    public void GetIndexByName_WithUnregisteredName_ReturnsNull()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        Assert.That(AzureSearchIndexStore.Instance.GetIndex("not-registered"), Is.Null);
    }


    [TestCase("")]
    [TestCase(null)]
    public void GetIndexByName_WithNullOrEmptyName_ReturnsNull(string? indexName)
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        Assert.That(AzureSearchIndexStore.Instance.GetIndex(indexName!), Is.Null);
    }

    #endregion

    #region GetIndex(int)

    [Test]
    public void GetIndexByIdentifier_WithRegisteredIdentifier_ReturnsIndex()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 42));

        var result = AzureSearchIndexStore.Instance.GetIndex(42);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Identifier, Is.EqualTo(42));
    }


    [Test]
    public void GetIndexByIdentifier_WithUnregisteredIdentifier_ReturnsNull()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 1));

        Assert.That(AzureSearchIndexStore.Instance.GetIndex(999), Is.Null);
    }

    #endregion

    #region GetRequiredIndex

    [Test]
    public void GetRequiredIndex_WithRegisteredIndex_ReturnsIndex()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        var result = AzureSearchIndexStore.Instance.GetRequiredIndex(INDEX_NAME);

        Assert.That(result.IndexName, Is.EqualTo(INDEX_NAME));
    }


    [Test]
    public void GetRequiredIndex_WithUnregisteredIndex_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.GetRequiredIndex("missing-index"));

        Assert.That(exception!.Message, Is.EqualTo("The index 'missing-index' is not registered."));
    }


    [TestCase("")]
    [TestCase(null)]
    public void GetRequiredIndex_WithNullOrEmptyName_ThrowsArgumentException(string? indexName)
    {
        Assert.Throws<ArgumentException>(() =>
            AzureSearchIndexStore.Instance.GetRequiredIndex(indexName!));
    }

    #endregion

    #region AddIndex

    [Test]
    public void AddIndex_WithNewIndex_RegistersIt()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        Assert.That(AzureSearchIndexStore.Instance.GetAllIndices().Count(), Is.EqualTo(1));
    }


    [Test]
    public void AddIndex_WithNullIndex_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            AzureSearchIndexStore.Instance.AddIndex(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("index"));
    }


    [Test]
    public void AddIndex_WithDuplicateIndexName_ThrowsInvalidOperationException()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 1));

        Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 2)));
    }


    [Test]
    public void AddIndex_WithDuplicateIdentifier_ThrowsInvalidOperationException()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 1));

        Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.AddIndex(CreateIndex(OTHER_INDEX_NAME, id: 1)));
    }


    [Test]
    public void AddIndex_WithDuplicateIndexNameDifferingByCase_ThrowsInvalidOperationException()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 1));

        Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME.ToUpperInvariant(), id: 2)));
    }

    #endregion

    #region SetIndices

    [Test]
    public void SetIndices_ClearsPreviouslyRegisteredIndices()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(INDEX_NAME, id: 1));

        AzureSearchIndexStore.Instance.SetIndices([CreateConfiguration(OTHER_INDEX_NAME, id: 2)]);

        var result = AzureSearchIndexStore.Instance.GetAllIndices().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IndexName, Is.EqualTo(OTHER_INDEX_NAME));
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(INDEX_NAME), Is.Null);
        });
    }


    [Test]
    public void SetIndices_WithEmptyCollection_LeavesStoreEmpty()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex());

        AzureSearchIndexStore.Instance.SetIndices([]);

        Assert.That(AzureSearchIndexStore.Instance.GetAllIndices(), Is.Empty);
    }


    [Test]
    public void SetIndices_MapsConfigurationModelOntoIndex()
    {
        AzureSearchIndexStore.Instance.SetIndices(
        [
            CreateConfiguration(INDEX_NAME, id: 7, languageNames: ["en", "fr"])
        ]);

        var index = AzureSearchIndexStore.Instance.GetRequiredIndex(INDEX_NAME);

        Assert.Multiple(() =>
        {
            Assert.That(index.Identifier, Is.EqualTo(7));
            Assert.That(index.WebSiteChannelName, Is.EqualTo(CHANNEL_NAME));
            Assert.That(index.LanguageNames, Is.EquivalentTo(new[] { "en", "fr" }));
            Assert.That(index.IncludedReusableContentTypes, Is.Empty);
        });
    }


    [Test]
    public void SetIndices_WithDuplicateConfigurations_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.SetIndices(
            [
                CreateConfiguration(INDEX_NAME, id: 1),
                CreateConfiguration(INDEX_NAME, id: 2)
            ]));
    }


    [Test]
    public void SetIndices_FromConfigurationService_RegistersIndicesFromService()
    {
        var mockConfigurationService = Substitute.For<IAzureSearchConfigurationStorageService>();
        mockConfigurationService.GetAllIndexData().Returns(
        [
            CreateConfiguration(INDEX_NAME, id: 1),
            CreateConfiguration(OTHER_INDEX_NAME, id: 2)
        ]);

        AzureSearchIndexStore.SetIndices(mockConfigurationService);

        Assert.Multiple(() =>
        {
            Assert.That(AzureSearchIndexStore.Instance.GetAllIndices().Count(), Is.EqualTo(2));
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(INDEX_NAME), Is.Not.Null);
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(OTHER_INDEX_NAME), Is.Not.Null);
        });
        mockConfigurationService.Received(1).GetAllIndexData();
    }


    [Test]
    public void SetIndices_FromConfigurationService_WithNoData_LeavesStoreEmpty()
    {
        var mockConfigurationService = Substitute.For<IAzureSearchConfigurationStorageService>();
        mockConfigurationService.GetAllIndexData().Returns([]);

        AzureSearchIndexStore.SetIndices(mockConfigurationService);

        Assert.That(AzureSearchIndexStore.Instance.GetAllIndices(), Is.Empty);
    }

    #endregion
}