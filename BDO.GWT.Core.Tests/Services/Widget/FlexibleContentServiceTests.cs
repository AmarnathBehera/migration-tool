using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget
{
    public class FlexibleContentServiceTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private Mock<IWebPageUrlRetriever> _urlRetrieverMock = null!;
        private FlexibleContentService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _urlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _service = new FlexibleContentService(_contentRetrieverMock.Object, _urlRetrieverMock.Object);
        }

        [Test]
        public async Task GetFlexibleContentAsync_MapsAllScalarFields_FromData()
        {
            var data = new FlexibleContentData
            {
                Title = "My Flexible Content",
                Description = "Some description",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "widget-1",
                LearnMoreButtonURLTarget = "_blank",
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("My Flexible Content"));
                Assert.That(result.Description, Is.EqualTo("Some description"));
                Assert.That(result.TopMargin, Is.EqualTo("40px"));
                Assert.That(result.BottomMargin, Is.EqualTo("20px"));
                Assert.That(result.WidgetID, Is.EqualTo("widget-1"));
                Assert.That(result.LearnMoreButtonURLTarget, Is.EqualTo("_blank"));
            });
        }

        [Test]
        public async Task GetFlexibleContentAsync_WithNoGuids_ReturnsEmptyItems()
        {
            var data = new FlexibleContentData
            {
                Title = "T",
                Description = "D",
                TopMargin = "0",
                BottomMargin = "0",
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            Assert.That(result.Items, Is.Not.Null);
            Assert.That(result.Items, Is.Empty);
        }

        [Test]
        public async Task GetFlexibleContentAsync_WithNoGuids_DoesNotInvokeContentRetriever()
        {
            var data = new FlexibleContentData
            {
                TopMargin = "0",
                BottomMargin = "0",
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            _contentRetrieverMock.VerifyNoOtherCalls();
            _urlRetrieverMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetFlexibleContentAsync_WithNullTitleAndDescription_ReturnsNulls()
        {
            var data = new FlexibleContentData
            {
                Title = null,
                Description = null,
                TopMargin = "0",
                BottomMargin = "0",
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.Null);
                Assert.That(result.Description, Is.Null);
            });
        }

        [Test]
        public async Task GetFlexibleContentAsync_DefaultsLearnMoreTarget_WhenNotProvided()
        {
            var data = new FlexibleContentData
            {
                TopMargin = "0",
                BottomMargin = "0",
                LearnMoreButtonURLTarget = null,
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            Assert.That(result.LearnMoreButtonURLTarget, Is.Null);
        }

        [Test]
        public async Task GetFlexibleContentAsync_WithNoGuids_ReturnsItemsAsEmptyArray()
        {
            var data = new FlexibleContentData
            {
                TopMargin = "0",
                BottomMargin = "0",
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Is.InstanceOf<FlexibleContentItemDto[]>());
                Assert.That(((FlexibleContentItemDto[])result.Items), Has.Length.EqualTo(0));
            });
        }

        //[Test]
        //public async Task GetFlexibleContentAsync_ReturnsNonNullDto_ForAllInputs()
        //{
        //    var data = new FlexibleContentData
        //    {
        //        TopMargin = "10",
        //        BottomMargin = "20",
        //    };

        //    var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

        //    Assert.That(result, Is.InstanceOf<DTOs.FlexibleContentDto>());
        //}

        [Test]
        public void Constructor_DoesNotThrow_WithValidArguments()
        {
            Assert.DoesNotThrow(() => new FlexibleContentService(
                _contentRetrieverMock.Object,
                _urlRetrieverMock.Object));
        }

        [Test]
        public async Task GetFlexibleContentAsync_PreservesEmptyStringFields()
        {
            var data = new FlexibleContentData
            {
                Title = string.Empty,
                Description = string.Empty,
                TopMargin = string.Empty,
                BottomMargin = string.Empty,
                WidgetID = string.Empty,
                LearnMoreButtonURLTarget = string.Empty,
                FlexibleContentGuids = [],
                FeaturedFlexibleContentGuids = [],
            };

            var result = await _service.GetFlexibleContentAsync(data, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo(string.Empty));
                Assert.That(result.Description, Is.EqualTo(string.Empty));
                Assert.That(result.TopMargin, Is.EqualTo(string.Empty));
                Assert.That(result.BottomMargin, Is.EqualTo(string.Empty));
                Assert.That(result.WidgetID, Is.EqualTo(string.Empty));
                Assert.That(result.LearnMoreButtonURLTarget, Is.EqualTo(string.Empty));
            });
        }
    }
}
