using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Base;
using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ResultListingTests
    {
        private Mock<ISqlBrowserResultProvider> _queryProvider;
        private Mock<ISqlBrowserExporter> _exporter;
        private Mock<IEventLogService> _eventLog;
        private Mock<IPageLinkGenerator> _pageLinkGenerator;

        private ResultListing _sut;

        [SetUp]
        public void Setup()
        {
            _queryProvider = new Mock<ISqlBrowserResultProvider>();
            _exporter = new Mock<ISqlBrowserExporter>();
            _eventLog = new Mock<IEventLogService>();
            _pageLinkGenerator = new Mock<IPageLinkGenerator>();

            _pageLinkGenerator
                .Setup(x => x.GetPath<ResultListing>())
                .Returns("/resultlisting");

            _pageLinkGenerator
                .Setup(x => x.GetPath<EditQuery>())
                .Returns("/editquery");

            _sut = new ResultListing(
                _queryProvider.Object,
                _exporter.Object,
                _eventLog.Object,
                _pageLinkGenerator.Object);
        }

        [Test]
        public async Task Export_InvalidExportType_ReturnsResponse()
        {
            var model = new ExportConfirmationDialogModel
            {
                ExportType = "invalid"
            };

            var result = await _sut.Export(model);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Export_EmptyContent_ReturnsResponse()
        {
            _exporter
                .Setup(x => x.Export(
                    It.IsAny<SqlBrowserExportType>(),
                    It.IsAny<string>()))
                .ReturnsAsync((
                    Array.Empty<byte>(),
                    "file.csv",
                    string.Empty));

            var model = new ExportConfirmationDialogModel
            {
                ExportType = "csv"
            };

            var result = await _sut.Export(model);

            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public async Task SubmitQuery_WithEmptyQuery_ReturnsResponse()
        {
            var model = new EditQueryModel
            {
                QueryText = string.Empty
            };

            var result = await _sut.SubmitQuery(model);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task SubmitQuery_WithValidQuery_CallsSetQuery()
        {
            _pageLinkGenerator
                .Setup(x => x.GetPath<ResultListing>())
                .Returns("/resultlisting");

            var model = new EditQueryModel
            {
                QueryText = "SELECT * FROM CMS_User"
            };

            await _sut.SubmitQuery(model);

            _queryProvider.Verify(
                x => x.SetQuery("SELECT * FROM CMS_User"),
                Times.Once);
        }

        [Test]
        public async Task OpenEditQuery_WithChannelId_CallsSetQuery()
        {
            _sut.ReportingChannelSettingId = 5;

            await _sut.OpenEditQuery();

            _queryProvider.Verify(
                x => x.SetQuery(It.Is<string>(
                    s => s.Contains("ChannelID = 5"))),
                Times.Once);
        }

        [Test]
        public async Task OpenEditQuery_ReturnsNavigationResponse()
        {
            var result = await _sut.OpenEditQuery();

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task BackToChannel_ReturnsNavigationResponse()
        {
            var result = await _sut.BackToChannel();

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task LoadDataContainers_ReturnsProviderResults()
        {
            var expected =
                new List<IDataContainer>();

            _queryProvider
                .Setup(x => x.GetRowsAsDataContainer())
                .ReturnsAsync(expected);

            var method = typeof(ResultListing)
                .GetMethod(
                    "LoadDataContainers",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            var task =
                (Task<IEnumerable<IDataContainer>>)method!
                    .Invoke(
                        _sut,
                        new object[]
                        {
                            CancellationToken.None
                        })!;

            var result = await task;

            Assert.That(result, Is.SameAs(expected));
        }

        [Test]
        public void GetIdentifier_ReturnsExpectedValue()
        {
            var container = new Mock<IDataContainer>();

            container
                .Setup(x =>
                    x[SqlBrowserResultProvider
                        .ROW_IDENTIFIER_COLUMN])
                .Returns(99);

            var method = typeof(ResultListing)
                .GetMethod(
                    "GetIdentifier",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            var result = method!.Invoke(
                _sut,
                new object[]
                {
                    container.Object
                });

            Assert.That(result, Is.EqualTo(99));
        }

    }
}