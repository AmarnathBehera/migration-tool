using BDO.GWT.GLOBAL.API.Dto;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Core;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class CustomGlobalLocationsServiceTests
{
    private Mock<IEventLogService> _eventLogServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        _eventLogServiceMock = new Mock<IEventLogService>();
    }

    private static IConfiguration BuildConfiguration(string? connectionString)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:CMSConnectionString"] = connectionString
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private CustomGlobalLocationsService CreateService(string? connectionString = "Server=nonexistent;Database=fake;Integrated Security=true;Connect Timeout=1") =>
        new(BuildConfiguration(connectionString), _eventLogServiceMock.Object);

    private static GlobalLocationCustomTableUpsertDto CreateUpsertDto() => new()
    {
        GlobalLocationTitle = "Head Office",
        MemberFirmId = 1,
        ChannelId = 42
    };

    [Test]
    public void Service_ImplementsICustomGlobalLocationsService()
    {
        var service = CreateService();

        Assert.That(service, Is.InstanceOf<ICustomGlobalLocationsService>());
    }

    [Test]
    public async Task GetAllAsync_WhenConnectionFails_ReturnsEmptyList()
    {
        // Malformed connection string throws at SqlConnection construction (inside the try/catch),
        // which the service swallows after logging, returning an empty list.
        var service = CreateService("this is not a valid connection string");

        var result = await service.GetAllAsync(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CreateAsync_WhenConnectionFails_ThrowsPersistenceException()
    {
        var service = CreateService("this is not a valid connection string");
        var dto = CreateUpsertDto();

        Assert.That(
            async () => await service.CreateAsync(dto),
            Throws.TypeOf<GlobalLocationPersistenceException>());
    }

    [Test]
    public void UpdateAsync_WhenConnectionFails_ThrowsPersistenceException()
    {
        var service = CreateService("this is not a valid connection string");
        var dto = CreateUpsertDto();

        Assert.That(
            async () => await service.UpdateAsync(1, dto),
            Throws.TypeOf<GlobalLocationPersistenceException>());
    }

    [Test]
    public void DuplicateMemberFirmException_MessageContainsMemberFirmId()
    {
        var ex = new DuplicateMemberFirmException(7);

        Assert.That(ex.Message, Does.Contain("7"));
    }

    [Test]
    public void GlobalLocationPersistenceException_PreservesInnerException()
    {
        var inner = new InvalidOperationException("boom");

        var ex = new GlobalLocationPersistenceException("failed", inner);

        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("failed"));
            Assert.That(ex.InnerException, Is.SameAs(inner));
        });
    }
}
