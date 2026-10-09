using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category("Unit")]
internal class IndexStoreTests
{
    private const string DEFAULT_INDEX = "SimpleIndex";
    private const string DEFAULT_CHANNEL = "DefaultChannel";
    private const string STRATEGY_NAME = "strategy";
    private const string WEB_PAGE_TREE_PATH = "/%";


    private static AzureSearchIndex CreateIndex(string indexName, int id) => new(
        new AzureSearchConfigurationModel
        {
            Id = id,
            IndexName = indexName,
            ChannelName = DEFAULT_CHANNEL,
            LanguageNames = ["en", "cz"],
            Paths = [new AzureSearchIndexIncludedPath(WEB_PAGE_TREE_PATH)],
            StrategyName = STRATEGY_NAME
        },
        []);


    [SetUp]
    public void SetUp() => AzureSearchIndexStore.Instance.SetIndices([]);


    [Test]
    public void AddAndGetIndex()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(DEFAULT_INDEX, 0));
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex("TestIndex", 1));

        Assert.Multiple(() =>
        {
            Assert.That(AzureSearchIndexStore.Instance.GetIndex("TestIndex") is not null);
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(DEFAULT_INDEX) is not null);
        });
    }


    [Test]
    public void AddIndex_AlreadyExists()
    {
        AzureSearchIndexStore.Instance.AddIndex(CreateIndex(DEFAULT_INDEX, 0));

        Assert.Throws<InvalidOperationException>(() =>
            AzureSearchIndexStore.Instance.AddIndex(CreateIndex(DEFAULT_INDEX, 0)));
    }


    [Test]
    public void SetIndicies()
    {
        var defaultIndex = new AzureSearchConfigurationModel { IndexName = "DefaultIndex", Id = 0 };
        var simpleIndex = new AzureSearchConfigurationModel { IndexName = "SimpleIndex", Id = 1 };

        AzureSearchIndexStore.Instance.SetIndices([defaultIndex, simpleIndex]);

        Assert.Multiple(() =>
        {
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(defaultIndex.IndexName) is not null);
            Assert.That(AzureSearchIndexStore.Instance.GetIndex(simpleIndex.IndexName) is not null);
        });
    }
}
