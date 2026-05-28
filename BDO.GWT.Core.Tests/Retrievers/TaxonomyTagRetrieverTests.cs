using CMS.ContentEngine;
using CMS.DataEngine;
using Moq;
using BDO.GWT.Core.Retrievers;
using TaxonomyTag = CMS.ContentEngine.Tag;

namespace BDO.GWT.Core.Tests.Retrievers
{
    public class TaxonomyTagRetrieverTests
    {
        private Mock<ITaxonomyRetriever> _taxonomyRetrieverMock = null!;
        private Mock<IInfoProvider<TagInfo>> _tagProviderMock = null!;
        private TaxonomyTagRetriever _service = null!;

        [SetUp]
        public void Setup()
        {
            _taxonomyRetrieverMock = new Mock<ITaxonomyRetriever>();
            _tagProviderMock = new Mock<IInfoProvider<TagInfo>>();

            _service = new TaxonomyTagRetriever(
                _taxonomyRetrieverMock.Object,
                _tagProviderMock.Object
            );
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_WhenTaxonomyNotFound_ReturnsEmpty()
        {
            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaxonomyData?)null);

            var result = await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_WithNullFilter_ReturnsAllTags()
        {
            var tags = new[] { CreateTag("tag-1"), CreateTag("tag-2"), CreateTag("tag-3") };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTaxonomyData(tags));

            var result = await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en", null);

            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_WithEmptyFilter_ReturnsAllTags()
        {
            var tags = new[] { CreateTag("tag-1"), CreateTag("tag-2") };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTaxonomyData(tags));

            var result = await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en", new HashSet<string>());

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_WithMatchingFilter_ReturnsOnlyMatchedTags()
        {
            var tags = new[] { CreateTag("tag-1"), CreateTag("tag-2"), CreateTag("tag-3") };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTaxonomyData(tags));

            var result = await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en", new HashSet<string> { "tag-1", "tag-3" });

            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.Any(t => t.Name == "tag-1"), Is.True);
                Assert.That(result.Any(t => t.Name == "tag-3"), Is.True);
                Assert.That(result.Any(t => t.Name == "tag-2"), Is.False);
            });
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_WithNoMatchingFilter_ReturnsEmpty()
        {
            var tags = new[] { CreateTag("tag-1"), CreateTag("tag-2") };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTaxonomyData(tags));

            var result = await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en", new HashSet<string> { "non-existing" });

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetTaxonomyTagsAsync_PassesCancellationTokenToRetriever()
        {
            var cts = new CancellationTokenSource();

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>(), cts.Token))
                .ReturnsAsync((TaxonomyData?)null);

            await _service.GetTaxonomyTagsAsync("TestTaxonomy", "en", cancellationToken: cts.Token);

            _taxonomyRetrieverMock.Verify(
                x => x.RetrieveTaxonomy("TestTaxonomy", "en", cts.Token),
                Times.Once);
        }

        [Test]
        public async Task GetTagAsync_WhenTaxonomyExists_ReturnsTaxonomyData()
        {
            var taxonomyData = CreateTaxonomyData(Array.Empty<TaxonomyTag>());

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy("TestTaxonomy", "en"))
                .ReturnsAsync(taxonomyData);

            var result = await _service.GetTagAsync("TestTaxonomy", "someValue", "en");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetTagAsync_WhenTaxonomyNotFound_ReturnsNull()
        {
            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TaxonomyData?)null);

            var result = await _service.GetTagAsync("TestTaxonomy", "someValue", "en");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTagAsync_CallsRetrieverWithCorrectParameters()
        {
            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTaxonomy(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TaxonomyData?)null);

            await _service.GetTagAsync("TestTaxonomy", "someValue", "en");

            _taxonomyRetrieverMock.Verify(
                x => x.RetrieveTaxonomy("TestTaxonomy", "en"),
                Times.Once);
        }

        [Test]
        [Ignore("IInfoProvider<T>.GetAsync is an extension method and cannot be mocked with Moq. Wrap IInfoProvider in a custom interface to enable testing.")]
        public async Task GetTaxonomyTagByNameAsync_WhenTagExists_ReturnsTagInfo()
        {
            var tagInfo = new TagInfo();

            _tagProviderMock
                .Setup(x => x.GetAsync("my-tag"))
                .ReturnsAsync(tagInfo);

            var result = await _service.GetTaxonomyTagByNameAsync("my-tag");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [Ignore("IInfoProvider<T>.GetAsync is an extension method and cannot be mocked with Moq. Wrap IInfoProvider in a custom interface to enable testing.")]
        public async Task GetTaxonomyTagByNameAsync_WhenTagNotFound_ReturnsNull()
        {
            _tagProviderMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<TagInfo>(null!));

            var result = await _service.GetTaxonomyTagByNameAsync("non-existing");

            Assert.That(result, Is.Null);
        }

        [Test]
        [Ignore("IInfoProvider<T>.GetAsync is an extension method and cannot be mocked with Moq. Wrap IInfoProvider in a custom interface to enable testing.")]
        public async Task GetTaxonomyTagByNameAsync_CallsProviderWithCorrectName()
        {
            _tagProviderMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<TagInfo>(null!));

            await _service.GetTaxonomyTagByNameAsync("my-tag");

            _tagProviderMock.Verify(x => x.GetAsync("my-tag"), Times.Once);
        }

        [Test]
        public async Task GetTaxonomyTagByGUIDs_WhenTagsFound_ReturnsTags()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var tagRefs = new[]
            {
                new TagReference { Identifier = guid1 },
                new TagReference { Identifier = guid2 },
            };
            var expectedTags = new[] { CreateTag("tag-1"), CreateTag("tag-2") };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTags(It.IsAny<IEnumerable<Guid>>(), "en"))
                .ReturnsAsync(expectedTags);

            var result = await _service.GetTaxonomyTagByGUIDs(tagRefs, "en");

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetTaxonomyTagByGUIDs_WhenRetrieverReturnsNull_ReturnsEmpty()
        {
            var tagRefs = new[] { new TagReference { Identifier = Guid.NewGuid() } };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTags(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<TaxonomyTag>?)null);

            var result = await _service.GetTaxonomyTagByGUIDs(tagRefs, "en");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetTaxonomyTagByGUIDs_PassesCorrectGuidsToRetriever()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var tagRefs = new[]
            {
                new TagReference { Identifier = guid1 },
                new TagReference { Identifier = guid2 },
            };

            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTags(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<TaxonomyTag>());

            await _service.GetTaxonomyTagByGUIDs(tagRefs, "en");

            _taxonomyRetrieverMock.Verify(
                x => x.RetrieveTags(
                    It.Is<IEnumerable<Guid>>(ids => ids.Contains(guid1) && ids.Contains(guid2)),
                    "en"),
                Times.Once);
        }

        [Test]
        public async Task GetTaxonomyTagByGUIDs_WithEmptyInput_ReturnsEmpty()
        {
            _taxonomyRetrieverMock
                .Setup(x => x.RetrieveTags(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<TaxonomyTag>());

            var result = await _service.GetTaxonomyTagByGUIDs(Enumerable.Empty<TagReference>(), "en");

            Assert.That(result, Is.Empty);
        }

        private static TaxonomyTag CreateTag(string name) =>
            new TaxonomyTag { Name = name };

        private static TaxonomyData CreateTaxonomyData(IEnumerable<TaxonomyTag> tags) =>
            new TaxonomyData { Tags = tags };
    }
}