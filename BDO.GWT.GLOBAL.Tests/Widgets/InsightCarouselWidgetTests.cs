using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.GLOBAL.Components.Widget.InsightCarouselWidget;
using BDO.GWT.GLOBAL.Widgets;
using BDO.GWT.Web.Core.Data;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Widgets
{
    public class InsightCarouselWidgetTests
    {
        private Mock<IInsightCarouselService> _insightCarouselServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private InsightCarouselWidget _widget = null!;

        [SetUp]
        public void Setup()
        {
            _insightCarouselServiceMock = new Mock<IInsightCarouselService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new InsightCarouselDto { FilteringTooltipEnabled = false });

            _widget = new InsightCarouselWidget(_insightCarouselServiceMock.Object, _contentRetrieverMock.Object);
        }

        [Test]
        public async Task InvokeAsync_WhenPageIsNull_ReturnsEmptyDtoView()
        {
            // ComponentViewModel has read-only properties; construct and set via reflection
            var model = CreateComponentViewModel(null, new InsightCarouselWidgetProperties());

            var result = await _widget.InvokeAsync(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            var dto = result?.ViewData?.Model as InsightCarouselDto;
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.FilteringTooltipEnabled, Is.False);
        }

        [Test]
        public void Debug_ListRoutedWebPageFields()
        {
            var fields = typeof(RoutedWebPage).GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            foreach (var f in fields)
            {
                TestContext.WriteLine($"{f.Name} : {f.FieldType}");
            }

            var props = typeof(RoutedWebPage).GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            foreach (var p in props)
            {
                TestContext.WriteLine($"{p.Name} : {p.PropertyType} (CanWrite={p.CanWrite})");
            }
        }
        private static RoutedWebPage CreateRoutedWebPage(Guid webPageItemGuid, string contentTypeName)
        {
#pragma warning disable SYSLIB0050
            var page = (RoutedWebPage)FormatterServices.GetUninitializedObject(typeof(RoutedWebPage));
#pragma warning restore SYSLIB0050

            var guidField = typeof(RoutedWebPage).GetField("<WebPageItemGUID>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var typeNameField = typeof(RoutedWebPage).GetField("<ContentTypeName>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            guidField?.SetValue(page, webPageItemGuid);
            typeNameField?.SetValue(page, contentTypeName);

            return page;
        }

        private static ComponentViewModel<T> CreateComponentViewModel<T>(object? page, T properties)
            where T : class, Kentico.Builder.Web.Mvc.IComponentProperties, new()
        {
            ComponentViewModel<T> model;
            // FormatterServices.GetUninitializedObject is obsolete; suppress only for this test construction
#pragma warning disable SYSLIB0050
            model = (ComponentViewModel<T>)FormatterServices.GetUninitializedObject(typeof(ComponentViewModel<T>));
#pragma warning restore SYSLIB0050

            // set the readonly 'Page' and 'Properties' backing fields
            var pageField = typeof(ComponentViewModel<T>).GetField("<Page>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var propertiesField = typeof(ComponentViewModel<T>).GetField("<Properties>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            pageField?.SetValue(model, page);
            propertiesField?.SetValue(model, properties);

            return model;
        }
    }
}