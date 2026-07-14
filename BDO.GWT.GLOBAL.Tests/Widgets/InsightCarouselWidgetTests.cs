using BDO.GWT.GLOBAL.Widgets;
using BDO.GWT.GLOBAL.Components.Widget.InsightCarouselWidget;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using CMS.ContentEngine;
using BDO.GWT.Core.DTOs;
using CMS.Websites;
using BDO.GWT.Core;
using BDO.GWT.Web.Core.Data;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Runtime.Serialization;

namespace BDO.GWT.GLOBAL.Tests.Widgets
{
    public class InsightCarouselWidgetTests
    {
        private Mock<IInsightCarouselService> _insightCarouselServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private InsightCarouselWidget _widget = null!;

        [SetUp]
        public void Setup()
        {
            _insightCarouselServiceMock = new Mock<IInsightCarouselService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _contentServiceMock = new Mock<IContentService>();

            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(It.IsAny<IWebPageFieldsSource>(), It.IsAny<InsightCarouselData>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new InsightCarouselDto { FilteringTooltipEnabled = false });

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new[] { (IWebPageFieldsSource)new DummyPage() });

            _widget = new InsightCarouselWidget(_insightCarouselServiceMock.Object, _contentServiceMock.Object, _contentRetrieverMock.Object);
        }

        private class DummyPage : IWebPageFieldsSource
        {
            public DummyPage()
            {
            }

            public WebPageFields SystemFields { get; set; } = new WebPageFields { ContentItemGUID = Guid.NewGuid() };
        }

        [Test]
        public async Task InvokeAsync_WhenPageIsNull_ReturnsEmptyDtoView()
        {
            // ComponentViewModel has read-only properties; construct and set via reflection
            var model = CreateComponentViewModel(null, new InsightCarouselWidgetProperties());

            var result = await _widget.InvokeAsync(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            var dto = result!.ViewData.Model as InsightCarouselDto;
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.FilteringTooltipEnabled, Is.False);
        }

        [Test]
        public async Task InvokeAsync_WhenPageExists_InvokesServiceAndReturnsDto()
        {
            var pageGuid = Guid.NewGuid();
            var pageContext = new { WebPageItemGUID = pageGuid };
            var model = CreateComponentViewModel(pageContext, new InsightCarouselWidgetProperties());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.Is<List<Guid>>(l => l.Contains(pageGuid))))
                .ReturnsAsync(new[] { (IWebPageFieldsSource)new DummyPage() });

            var result = await _widget.InvokeAsync(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            var dto = result!.ViewData.Model as InsightCarouselDto;
            Assert.That(dto, Is.Not.Null);
        }

        private static ComponentViewModel<T> CreateComponentViewModel<T>(object? page, T properties)
            where T : class, Kentico.PageBuilder.Web.Mvc.IComponentProperties, new()
        {
            var model = (ComponentViewModel<T>)FormatterServices.GetUninitializedObject(typeof(ComponentViewModel<T>));

            // set the readonly 'Page' and 'Properties' backing fields
            var pageField = typeof(ComponentViewModel<T>).GetField("<Page>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var propertiesField = typeof(ComponentViewModel<T>).GetField("<Properties>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            pageField?.SetValue(model, page);
            propertiesField?.SetValue(model, properties);

            return model;
        }
    }
}
