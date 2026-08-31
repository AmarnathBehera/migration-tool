using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.DTOs.Cards;
using Kentico.Content.Web.Mvc;
using CMS.ContentEngine;
using CMS.Websites;
using BDO.GWT.Core.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Web.Core.Data;

namespace BDO.GWT.Core.Tests.Services
{
    public class InsightCarouselServiceTests
    {
        private Mock<IFilteringHelper> _filteringHelperMock = null!;
        private Mock<IInsightCardService> _insightCardServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IInsightCarouselStateStore> _stateStoreMock = null!;
        private InsightCarouselService _service = null!;

        [SetUp]
        public void Setup()
        {
            _filteringHelperMock = new Mock<IFilteringHelper>();
            _insightCardServiceMock = new Mock<IInsightCardService>();
            _contentServiceMock = new Mock<IContentService>();
            _stateStoreMock = new Mock<IInsightCarouselStateStore>();

            _filteringHelperMock.Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _insightCardServiceMock
                .Setup(x => x.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<RetrievePagesQueryParameters>?>(), It.IsAny<Func<Insight, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

            _service = new InsightCarouselService(
                _filteringHelperMock.Object,
                _insightCardServiceMock.Object,
                _contentServiceMock.Object,
                _stateStoreMock.Object);
        }

        private class DummyPage : IWebPageFieldsSource
        {
            public WebPageFields SystemFields { get; set; } = new WebPageFields { ContentItemGUID = Guid.NewGuid() };
        }

        private class ContentType : IWebPageFieldsSource
        {
            public WebPageFields SystemFields { get; set; } = new WebPageFields { ContentItemGUID = Guid.NewGuid() };
        }

        [Test]
        public async Task GetInsightCarouselAsync_WhenFilteringEnabled_CallsCreateFilters_And_PopulatesDto()
        {
            var currentPage = new DummyPage();
            var filters = new[] { new FilterItemDto<OptionDto> { Name = "businessLine", Options = Array.Empty<OptionDto>() } };

            _insightCardServiceMock
                .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(filters);

            _insightCardServiceMock
                .Setup(x => x.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<RetrievePagesQueryParameters>?>(), It.IsAny<Func<Insight, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = new[] { new ContentPageCardDto { Title = "t1" } }, TotalCount = 1 });

            var data = new InsightCarouselData
            {
                Title = "T",
                Description = "D",
                FilteringEnabled = true,
                Filters = new HashSet<string> { "businessLine" },
            };

            var dto = await _service.GetInsightCarouselAsync(currentPage, data, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(dto.Title, Is.EqualTo("T"));
                Assert.That(dto.Filters, Is.Not.Null);
                Assert.That(dto.Items!.Count(), Is.EqualTo(1));
                Assert.That(dto.StateGUID, Is.Not.EqualTo(Guid.Empty));
                _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
                _stateStoreMock.Verify(x => x.Save(dto.StateGUID, It.IsAny<HashSet<ContentItemReference>>()), Times.Once);
            });
        }

        [Test]
        public async Task GetInsightCarouselAsync_WhenFilteringDisabled_DoesNotCallCreateFilters()
        {
            var currentPage = new DummyPage();

            var data = new InsightCarouselData
            {
                FilteringEnabled = false
            };

            var dto = await _service.GetInsightCarouselAsync(currentPage, data, CancellationToken.None);

            Assert.That(dto.Filters, Is.Null);
            _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetAPIFilteredItemsAsync_WithUnknownPageType_PassesDefaultPageSize_ToCardsService()
        {
            CardsQuery? captured = null;

            _insightCardServiceMock
                .Setup(x => x.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<RetrievePagesQueryParameters>?>(), It.IsAny<Func<Insight, bool>>(), It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((q, _, __, ___, ____, _____) => captured = q)
                .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

            var query = new CardsQuery { PageType = "UnknownType", CurrentPage = null, PageSize = null };

            await _service.GetAPIFilteredItemsAsync(query,"fr-fr", CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.CurrentPage, Is.EqualTo(1));
                Assert.That(captured.PageSize, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task GetAPIFilteredItemsAsync_WithState_UsesStateStore()
        {
            var stateGuid = Guid.NewGuid();
            var query = new CardsQuery { State = stateGuid };

            _stateStoreMock
                .Setup(x => x.TryGet(stateGuid, out It.Ref<HashSet<ContentItemReference>>.IsAny))
                .Returns((Guid _, out HashSet<ContentItemReference> featured) =>
                {
                    featured = new HashSet<ContentItemReference> { new() { Identifier = Guid.NewGuid() } };
                    return true;
                });

            await _service.GetAPIFilteredItemsAsync(query,"en-gb", CancellationToken.None);

            _stateStoreMock.Verify(x => x.TryGet(stateGuid, out It.Ref<HashSet<ContentItemReference>>.IsAny), Times.Once);
        }

        [Test]
        public async Task GetFilteredItemsAsync_WithContentTypePageName_CallsCardsService()
        {
            var page = new ContentType();
            CardsQuery? captured = null;

            _insightCardServiceMock
                .Setup(x => x.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<RetrievePagesQueryParameters>?>(), It.IsAny<Func<Insight, bool>>(), It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((q, _, __, ___, ____, _____) => captured = q)
                .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

            var result = await _service.GetFilteredItemsAsync(page, new HashSet<ContentItemReference>(), new CardsQuery(), CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(result, Is.Not.Null);
            });
        }
    }
}
