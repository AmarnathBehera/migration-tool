using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Core;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class GlobalLocationCustomTableDataServiceTests
    {
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private GlobalLocationCustomTableDataService _service = null!;

        [SetUp]
        public void Setup()
        {
            _eventLogServiceMock = new Mock<IEventLogService>();
        }
               
        
        // IConfiguration.GetConnectionString is an extension method (ConfigurationExtensions), which
        // Moq cannot set up directly. Instead, a real IConfigurationRoot is built from an in-memory
        // collection so the extension method resolves naturally against real configuration data.
        private static IConfiguration BuildConfiguration(string? connectionString)
        {
            var data = new Dictionary<string, string?>();

            if (connectionString is not null)
            {
                data["ConnectionStrings:CMSConnectionString"] = connectionString;
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
        }

        [Test]
        public async Task GetLocationsAsync_WhenConnectionStringIsMissing_ReturnsDefaultDto()
        {
            var configuration = BuildConfiguration(connectionString: null);
            _service = new GlobalLocationCustomTableDataService(configuration, _eventLogServiceMock.Object);

            // GetConnectionString("CMSConnectionString") resolves to null when not configured,
            // which the service force-unwraps with '!' before passing to SqlConnection, causing
            // an ArgumentNullException that is caught internally.
            var result = await _service.GetLocationsAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<GlobalLocationCustomTableDto>());
            Assert.That(result.ItemId, Is.EqualTo(0));
            Assert.That(result.GlobalLocationTitle, Is.Null);
        }

        [Test]
        public async Task GetLocationsAsync_WhenConnectionStringIsMissing_LogsException()
        {
            var configuration = BuildConfiguration(connectionString: null);
            _service = new GlobalLocationCustomTableDataService(configuration, _eventLogServiceMock.Object);

            var result = await _service.GetLocationsAsync(1);

            // LogException is an extension method (EventLogServiceExtensions.LogException) and cannot
            // be used in Moq Verify/Setup expressions. It ultimately delegates to the mockable
            // interface method IEventLogService.LogEvent(EventLogData), so we verify that instead.
            _eventLogServiceMock.Verify(
                x => x.LogEvent(It.Is<EventLogData>(e =>
                    e.Source == "GlobalLocationCustomTableDataService" &&
                    e.EventCode == "GetLocationsAsync")),
                Times.Once);
        }

        [Test]
        public async Task GetLocationsAsync_WhenConnectionStringIsInvalid_DoesNotThrow()
        {
            var configuration = BuildConfiguration(
                "Server=(invalid-host-does-not-exist);Database=DoesNotExist;User Id=sa;Password=invalid;Connect Timeout=1;TrustServerCertificate=True;");
            _service = new GlobalLocationCustomTableDataService(configuration, _eventLogServiceMock.Object);

            GlobalLocationCustomTableDto? result = null;

            Assert.DoesNotThrowAsync(async () =>
                result = await _service.GetLocationsAsync(1));

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ItemId, Is.EqualTo(0));
        }

        [Test]
        public async Task GetLocationsAsync_WhenConnectionStringIsInvalid_LogsException()
        {
            var configuration = BuildConfiguration(
                "Server=(invalid-host-does-not-exist);Database=DoesNotExist;User Id=sa;Password=invalid;Connect Timeout=1;TrustServerCertificate=True;");
            _service = new GlobalLocationCustomTableDataService(configuration, _eventLogServiceMock.Object);

            await _service.GetLocationsAsync(1);

            // See note above: verify the underlying LogEvent call instead of the LogException
            // extension method, which Moq cannot verify directly.
            _eventLogServiceMock.Verify(
                x => x.LogEvent(It.Is<EventLogData>(e =>
                    e.Source == "GlobalLocationCustomTableDataService" &&
                    e.EventCode == "GetLocationsAsync")),
                Times.Once);
        }

        [Test]
        public void Constructor_AllowsNullConfiguration()
        {
            // Constructor does not null-check its dependencies.
            Assert.DoesNotThrow(() => new GlobalLocationCustomTableDataService(null!, _eventLogServiceMock.Object));
        }

        [Test]
        public void Constructor_AllowsNullEventLogService()
        {
            var configuration = BuildConfiguration(connectionString: null);
            Assert.DoesNotThrow(() => new GlobalLocationCustomTableDataService(configuration, null!));
        }
    }
}