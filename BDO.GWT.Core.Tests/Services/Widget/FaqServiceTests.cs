using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Widget;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget
{
    public class FaqServiceTests
    {
        private static readonly string[] ExpectedTitles = ["First", "Second", "Third"];

        private Mock<IContentService> _contentServiceMock;
        private FaqService _service;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _service = new FaqService(_contentServiceMock.Object);
        }

        [Test]
        public async Task GetFaqAsync_MapsAllFaqDataFieldsToDto()
        {
            var data = new FaqData
            {
                Title = "My FAQ",
                Layout = "accordion",
                HoverColour = "red",
                TopMargin = "40px",
                BottomMargin = "20px",
                WidgetID = "faq-1",
                PageGuids = []
            };

            _contentServiceMock
                .Setup(s => s.GetPagesAsync<FaqPage>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _service.GetFaqAsync(data, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("My FAQ"));
                Assert.That(result.Layout, Is.EqualTo("accordion"));
                Assert.That(result.HoverColour, Is.EqualTo("red"));
                Assert.That(result.TopMargin, Is.EqualTo("40px"));
                Assert.That(result.BottomMargin, Is.EqualTo("20px"));
                Assert.That(result.WidgetID, Is.EqualTo("faq-1"));
            });
        }

        [Test]
        public async Task GetFaqAsync_WithEmptyGuids_ReturnsEmptyTopics()
        {
            var data = new FaqData { Layout = "cards", PageGuids = [] };

            var result = await _service.GetFaqAsync(data, CancellationToken.None);

            Assert.That(result.Topics, Is.Empty);

            _contentServiceMock.Verify(
                s => s.GetPagesAsync<FaqPage>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetFaqAsync_WithNullTitle_ReturnsDtoWithNullTitle()
        {
            var data = new FaqData { Title = null, Layout = "list", PageGuids = [] };

            var result = await _service.GetFaqAsync(data, CancellationToken.None);

            Assert.That(result.Title, Is.Null);
        }

        [Test]
        public void GetFaqSchemaOrgAsync_WithTopics_MapsQuestionsAndAnswers()
        {
            var faqDto = new FaqDto
            {
                Topics =
                [
                    new FaqPageDto { Title = "What is BDO?", Description = "BDO is a firm." },
                    new FaqPageDto { Title = "Where are you?", Description = "Worldwide." }
                ]
            };

            var result = _service.GetFaqSchemaOrgAsync(faqDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MainEntity, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                Assert.That(result.MainEntity![0].Name, Is.EqualTo("What is BDO?"));
                Assert.That(result.MainEntity![0].AcceptedAnswer?.Text, Is.EqualTo("BDO is a firm."));
                Assert.That(result.MainEntity![1].Name, Is.EqualTo("Where are you?"));
                Assert.That(result.MainEntity![1].AcceptedAnswer?.Text, Is.EqualTo("Worldwide."));
            });
        }

        [Test]
        public void GetFaqSchemaOrgAsync_WithEmptyTopics_ReturnsEmptyMainEntity()
        {
            var faqDto = new FaqDto { Topics = [] };

            var result = _service.GetFaqSchemaOrgAsync(faqDto);

            Assert.That(result.MainEntity, Is.Empty);
        }

        [Test]
        public void GetFaqSchemaOrgAsync_WithNullTitleAndDescription_MapsNullValues()
        {
            var faqDto = new FaqDto
            {
                Topics = [new FaqPageDto { Title = null, Description = null }]
            };

            var result = _service.GetFaqSchemaOrgAsync(faqDto);

            Assert.That(result.MainEntity, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.MainEntity![0].Name, Is.Null);
                Assert.That(result.MainEntity![0].AcceptedAnswer?.Text, Is.Null);
            });
        }

        [Test]
        public void GetFaqSchemaOrgAsync_PreservesTopicOrder()
        {
            var faqDto = new FaqDto
            {
                Topics =
                [
                    new FaqPageDto { Title = "First" },
                    new FaqPageDto { Title = "Second" },
                    new FaqPageDto { Title = "Third" }
                ]
            };

            var result = _service.GetFaqSchemaOrgAsync(faqDto);

            Assert.That(result.MainEntity!.Select(q => q.Name),
                Is.EqualTo(ExpectedTitles));
        }
    }
}
