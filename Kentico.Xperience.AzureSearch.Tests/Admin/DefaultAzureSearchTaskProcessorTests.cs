using CMS.Core;
using CMS.Websites;

using Kentico.Xperience.AzureSearch.Admin;
using Kentico.Xperience.AzureSearch.Indexing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

/// <summary>
/// Tests for <see cref="DefaultAzureSearchTaskProcessor"/>.
/// </summary>
/// <remarks>
/// All dependencies are interfaces, so the processor can be exercised without any CMS bootstrapping.
/// <see cref="AzureSearchIndexStore"/> is a singleton and is reset in <see cref="SetUp"/>/<see cref="TearDown"/>.
/// </remarks>
[TestFixture]
[Category("Unit")]
internal class DefaultAzureSearchTaskProcessorTests
{
    private const string INDEX_NAME = "test-index";
    private const string OTHER_INDEX_NAME = "other-index";
    private const string CHANNEL_NAME = "test-channel";
    private const string LANGUAGE_NAME = "en";
    private const string TREE_PATH = "/home";

    private IAzureSearchClient azureSearchClient = null!;
    private IEventLogService eventLogService = null!;
    private IWebPageUrlRetriever urlRetriever = null!;
    private IServiceProvider serviceProvider = null!;
    private IAzureSearchIndexingStrategy strategy = null!;
    private DefaultAzureSearchTaskProcessor processor = null!;


    [SetUp]
    public void SetUp()
    {
        AzureSearchIndexStore.Instance.SetIndices([]);
        AzureSearchIndexStore.Instance.SetIndices([CreateConfiguration(INDEX_NAME, 1)]);

        azureSearchClient = Substitute.For<IAzureSearchClient>();
        eventLogService = Substitute.For<IEventLogService>();
        urlRetriever = Substitute.For<IWebPageUrlRetriever>();
        strategy = Substitute.For<IAzureSearchIndexingStrategy>();
        serviceProvider = Substitute.For<IServiceProvider>();

        // GetRequiredStrategy resolves the strategy type registered on the index.
        serviceProvider
            .GetService(typeof(BaseAzureSearchIndexingStrategy<BaseAzureSearchModel>))
            .Returns(strategy);

        azureSearchClient
            .DeleteRecords(Arg.Any<IEnumerable<string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<IEnumerable<string>>().Count());

        azureSearchClient
            .UpsertRecords(Arg.Any<IEnumerable<IAzureSearchModel>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<IEnumerable<IAzureSearchModel>>().Count());

        var options = Options.Create(new AzureSearchOptions { IndexItemDelay = 0 });

        processor = new DefaultAzureSearchTaskProcessor(
            azureSearchClient,
            eventLogService,
            urlRetriever,
            serviceProvider,
            options);
    }


    [TearDown]
    public void TearDown() => AzureSearchIndexStore.Instance.SetIndices([]);


    private static AzureSearchConfigurationModel CreateConfiguration(string indexName, int id) => new()
    {
        Id = id,
        IndexName = indexName,
        ChannelName = CHANNEL_NAME,
        StrategyName = "Default",
        LanguageNames = [LANGUAGE_NAME],
        Paths = [],
        ReusableContentTypeNames = []
    };


    private static IndexEventWebPageItemModel CreateWebPageItem(Guid? guid = null, bool isSecured = false) =>
        new(
            itemID: 1,
            itemGuid: guid ?? Guid.NewGuid(),
            languageName: LANGUAGE_NAME,
            contentTypeName: "Test.Page",
            name: "Test page",
            isSecured: isSecured,
            contentTypeID: 10,
            contentLanguageID: 20,
            websiteChannelName: CHANNEL_NAME,
            webPageItemTreePath: TREE_PATH,
            order: 0);


    private void MapsTo(IAzureSearchModel? model) =>
        strategy.MapToAzureSearchModelOrNull(Arg.Any<IIndexEventItemModel>()).Returns(Task.FromResult(model));


    private void UrlResolvesTo(string relativePath) =>
        urlRetriever
            .Retrieve(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new WebPageUrl(relativePath, $"https://localhost{relativePath.TrimStart('~')}"));

    #region ProcessAzureSearchTasks - empty input

    [Test]
    public async Task ProcessAzureSearchTasks_WithNoItems_ReturnsZeroAndCallsNoClientMethods()
    {
        int result = await processor.ProcessAzureSearchTasks([], CancellationToken.None);

        Assert.That(result, Is.Zero);

        await azureSearchClient.DidNotReceiveWithAnyArgs()
            .UpsertRecords(default!, default!, default);
        await azureSearchClient.DidNotReceiveWithAnyArgs()
            .DeleteRecords(default!, default!, default);
    }

    #endregion

    #region Upsert path

    [Test]
    public async Task ProcessAzureSearchTasks_WithUpdateTask_UpsertsMappedModel()
    {
        var model = new BaseAzureSearchModel { Url = "/existing" };
        MapsTo(model);

        var item = CreateWebPageItem();
        var queueItem = new AzureSearchQueueItem(item, AzureSearchTaskType.UPDATE, INDEX_NAME);

        int result = await processor.ProcessAzureSearchTasks([queueItem], CancellationToken.None);

        await azureSearchClient.Received(1).UpsertRecords(
            Arg.Is<IEnumerable<IAzureSearchModel>>(models => models.Count() == 1 && models.Single() == model),
            INDEX_NAME,
            Arg.Any<CancellationToken>());

        Assert.That(result, Is.EqualTo(1));
    }


    [Test]
    public async Task ProcessAzureSearchTasks_WithUpdateTask_SetsBaseProperties()
    {
        var model = new BaseAzureSearchModel { Url = "/existing" };
        MapsTo(model);

        var guid = Guid.NewGuid();
        var item = CreateWebPageItem(guid);

        await processor.ProcessAzureSearchTasks(
            [new AzureSearchQueueItem(item, AzureSearchTaskType.UPDATE, INDEX_NAME)],
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(model.ContentTypeName, Is.EqualTo(item.ContentTypeName));
            Assert.That(model.LanguageName, Is.EqualTo(LANGUAGE_NAME));
            Assert.That(model.ItemGuid, Is.EqualTo(guid.ToString()));
            Assert.That(model.ObjectID, Is.EqualTo($"{guid}_{LANGUAGE_NAME}"));
        });
    }


    [Test]
    public async Task ProcessAzureSearchTasks_WhenModelHasNoUrl_ResolvesUrlFromRetriever()
    {
        var model = new BaseAzureSearchModel { Url = string.Empty };
        MapsTo(model);
        UrlResolvesTo("/resolved-path");

        await processor.ProcessAzureSearchTasks(
            [new AzureSearchQueueItem(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME)],
            CancellationToken.None);

        Assert.That(model.Url, Is.EqualTo("/resolved-path"));
        await urlRetriever.Received(1).Retrieve(TREE_PATH, CHANNEL_NAME, LANGUAGE_NAME);
    }


    [Test]
    public async Task ProcessAzureSearchTasks_WhenModelAlreadyHasUrl_DoesNotCallRetriever()
    {
        var model = new BaseAzureSearchModel { Url = "/already-set" };
        MapsTo(model);

        await processor.ProcessAzureSearchTasks(
            [new AzureSearchQueueItem(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME)],
            CancellationToken.None);

        Assert.That(model.Url, Is.EqualTo("/already-set"));
        await urlRetriever.DidNotReceiveWithAnyArgs().Retrieve(default!, default!, default!);
    }


    [Test]
    public async Task ProcessAzureSearchTasks_WhenUrlRetrievalThrows_SetsEmptyUrlAndStillUpserts()
    {
        var model = new BaseAzureSearchModel { Url = string.Empty };
        MapsTo(model);

        urlRetriever
            .Retrieve(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns<WebPageUrl>(_ => throw new InvalidOperationException("page deleted"));

        int result = await processor.ProcessAzureSearchTasks(
            [new AzureSearchQueueItem(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME)],
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(model.Url, Is.Empty);
            Assert.That(result, Is.EqualTo(1));
        });
    }

    #endregion

    #region Delete path

    [Test]
    public async Task ProcessAzureSearchTasks_WithDeleteTask_DeletesExpectedId()
    {
        var guid = Guid.NewGuid();
        var queueItem = new AzureSearchQueueItem(CreateWebPageItem(guid), AzureSearchTaskType.DELETE, INDEX_NAME);

        int result = await processor.ProcessAzureSearchTasks([queueItem], CancellationToken.None);

        await azureSearchClient.Received(1).DeleteRecords(
            Arg.Is<IEnumerable<string>>(ids => ids.Single() == $"{guid}_{LANGUAGE_NAME}"),
            INDEX_NAME,
            Arg.Any<CancellationToken>());

        Assert.That(result, Is.EqualTo(1));
    }


    [Test]
    public async Task ProcessAzureSearchTasks_WhenStrategyReturnsNull_ConvertsUpdateIntoDelete()
    {
        MapsTo(null);

        var guid = Guid.NewGuid();
        var queueItem = new AzureSearchQueueItem(CreateWebPageItem(guid, isSecured: true), AzureSearchTaskType.UPDATE, INDEX_NAME);

        await processor.ProcessAzureSearchTasks([queueItem], CancellationToken.None);

        await azureSearchClient.Received(1).DeleteRecords(
            Arg.Is<IEnumerable<string>>(ids => ids.Single() == $"{guid}_{LANGUAGE_NAME}"),
            INDEX_NAME,
            Arg.Any<CancellationToken>());

        await azureSearchClient.Received(1).UpsertRecords(
            Arg.Is<IEnumerable<IAzureSearchModel>>(models => !models.Any()),
            INDEX_NAME,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Grouping, batching and unknown indices

    [Test]
    public async Task ProcessAzureSearchTasks_GroupsItemsByIndexName()
    {
        AzureSearchIndexStore.Instance.SetIndices(
        [
            CreateConfiguration(INDEX_NAME, 1),
            CreateConfiguration(OTHER_INDEX_NAME, 2)
        ]);

        MapsTo(new BaseAzureSearchModel { Url = "/url" });

        AzureSearchQueueItem[] items =
        [
            new(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME),
            new(CreateWebPageItem(), AzureSearchTaskType.UPDATE, OTHER_INDEX_NAME)
        ];

        await processor.ProcessAzureSearchTasks(items, CancellationToken.None);

        await azureSearchClient.Received(1).UpsertRecords(Arg.Any<IEnumerable<IAzureSearchModel>>(), INDEX_NAME, Arg.Any<CancellationToken>());
        await azureSearchClient.Received(1).UpsertRecords(Arg.Any<IEnumerable<IAzureSearchModel>>(), OTHER_INDEX_NAME, Arg.Any<CancellationToken>());
    }


   


    [Test]
    public async Task ProcessAzureSearchTasks_RespectsMaximumBatchSize()
    {
        MapsTo(new BaseAzureSearchModel { Url = "/url" });

        var items = Enumerable
            .Range(0, 5)
            .Select(_ => new AzureSearchQueueItem(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME))
            .ToArray();

        await processor.ProcessAzureSearchTasks(items, CancellationToken.None, maximumBatchSize: 2);

        // 5 items, batch size 2 => 3 batches => 3 upsert calls.
        await azureSearchClient.Received(3).UpsertRecords(
            Arg.Any<IEnumerable<IAzureSearchModel>>(),
            INDEX_NAME,
            Arg.Any<CancellationToken>());
    }


    [Test]
    public async Task ProcessAzureSearchTasks_SumsSuccessfulOperationsAcrossBatches()
    {
        MapsTo(new BaseAzureSearchModel { Url = "/url" });

        var items = Enumerable
            .Range(0, 4)
            .Select(_ => new AzureSearchQueueItem(CreateWebPageItem(), AzureSearchTaskType.UPDATE, INDEX_NAME))
            .ToArray();

        int result = await processor.ProcessAzureSearchTasks(items, CancellationToken.None, maximumBatchSize: 2);

        Assert.That(result, Is.EqualTo(4));
    }

    #endregion

    #region Error handling

   

    #endregion
}
