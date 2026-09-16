using BDO.GWT.GLOBAL.API.Dto;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Core;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;

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

    #region GlobalLocationCustomTableUpsertDto

    [Test]
    public void UpsertDto_Properties_CanBeSetAndRetrieved()
    {
        var dto = new GlobalLocationCustomTableUpsertDto
        {
            MetadataLocationPinColour = "Red",
            GlobalLocationTitle = "Title",
            GlobalLocationAddress1 = "Address1",
            GlobalLocationAddress2 = "Address2",
            GlobalLocationCity = "City",
            GlobalLocationState = "State",
            GlobalLocationCountry = "Country",
            GlobalLocationPostalCode = "12345",
            GlobalLocationRegion = "Region",
            GlobalLocationLatitude = 1.23m,
            GlobalLocationLongitude = 4.56m,
            GlobalLocationUrl = "https://example.com",
            MemberFirmId = 1,
            ChannelId = 2,
            ItemOrder = 3
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.MetadataLocationPinColour, Is.EqualTo("Red"));
            Assert.That(dto.GlobalLocationTitle, Is.EqualTo("Title"));
            Assert.That(dto.GlobalLocationAddress1, Is.EqualTo("Address1"));
            Assert.That(dto.GlobalLocationAddress2, Is.EqualTo("Address2"));
            Assert.That(dto.GlobalLocationCity, Is.EqualTo("City"));
            Assert.That(dto.GlobalLocationState, Is.EqualTo("State"));
            Assert.That(dto.GlobalLocationCountry, Is.EqualTo("Country"));
            Assert.That(dto.GlobalLocationPostalCode, Is.EqualTo("12345"));
            Assert.That(dto.GlobalLocationRegion, Is.EqualTo("Region"));
            Assert.That(dto.GlobalLocationLatitude, Is.EqualTo(1.23m));
            Assert.That(dto.GlobalLocationLongitude, Is.EqualTo(4.56m));
            Assert.That(dto.GlobalLocationUrl, Is.EqualTo("https://example.com"));
            Assert.That(dto.MemberFirmId, Is.EqualTo(1));
            Assert.That(dto.ChannelId, Is.EqualTo(2));
            Assert.That(dto.ItemOrder, Is.EqualTo(3));
        });
    }

    [Test]
    public void UpsertDto_Properties_DefaultToNullOrDefaultValues()
    {
        var dto = new GlobalLocationCustomTableUpsertDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.MetadataLocationPinColour, Is.Null);
            Assert.That(dto.GlobalLocationTitle, Is.Null);
            Assert.That(dto.GlobalLocationAddress1, Is.Null);
            Assert.That(dto.GlobalLocationAddress2, Is.Null);
            Assert.That(dto.GlobalLocationCity, Is.Null);
            Assert.That(dto.GlobalLocationState, Is.Null);
            Assert.That(dto.GlobalLocationCountry, Is.Null);
            Assert.That(dto.GlobalLocationPostalCode, Is.Null);
            Assert.That(dto.GlobalLocationRegion, Is.Null);
            Assert.That(dto.GlobalLocationLatitude, Is.Null);
            Assert.That(dto.GlobalLocationLongitude, Is.Null);
            Assert.That(dto.GlobalLocationUrl, Is.Null);
            Assert.That(dto.MemberFirmId, Is.EqualTo(0));
            Assert.That(dto.ChannelId, Is.EqualTo(0));
            Assert.That(dto.ItemOrder, Is.Null);
        });
    }

    [Test]
    public void UpsertDto_Deserialize_MissingMemberFirmId_ThrowsJsonException()
    {
        var json = """
        {
            "ChannelId": 2
        }
        """;

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<GlobalLocationCustomTableUpsertDto>(json));
    }

    [Test]
    public void UpsertDto_Deserialize_MissingChannelId_ThrowsJsonException()
    {
        var json = """
        {
            "MemberFirmId": 1
        }
        """;

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<GlobalLocationCustomTableUpsertDto>(json));
    }

    [Test]
    public void UpsertDto_Deserialize_WithRequiredFields_Succeeds()
    {
        var json = """
        {
            "MemberFirmId": 1,
            "ChannelId": 2,
            "GlobalLocationTitle": "Title"
        }
        """;

        var dto = JsonSerializer.Deserialize<GlobalLocationCustomTableUpsertDto>(json);

        Assert.Multiple(() =>
        {
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.MemberFirmId, Is.EqualTo(1));
            Assert.That(dto.ChannelId, Is.EqualTo(2));
            Assert.That(dto.GlobalLocationTitle, Is.EqualTo("Title"));
        });
    }

    #endregion

    #region GlobalLocationCustomTableApiDto

    [Test]
    public void ApiDto_Properties_CanBeSetAndRetrieved()
    {
        var itemGuid = Guid.NewGuid();
        var createdWhen = new DateTime(2024, 1, 1);
        var modifiedWhen = new DateTime(2024, 2, 1);

        var dto = new GlobalLocationCustomTableApiDto
        {
            ItemId = 10,
            ItemGuid = itemGuid,
            MetadataLocationPinColour = "Blue",
            GlobalLocationTitle = "Title",
            GlobalLocationAddress1 = "Address1",
            GlobalLocationAddress2 = "Address2",
            GlobalLocationCity = "City",
            GlobalLocationState = "State",
            GlobalLocationCountry = "Country",
            GlobalLocationPostalCode = "54321",
            GlobalLocationRegion = "Region",
            GlobalLocationLatitude = 9.87m,
            GlobalLocationLongitude = 6.54m,
            GlobalLocationUrl = "https://example.com/api",
            MemberFirmId = 5,
            ChannelId = 6,
            ItemCreatedBy = 7,
            ItemCreatedWhen = createdWhen,
            ItemModifiedBy = 8,
            ItemModifiedWhen = modifiedWhen,
            ItemOrder = 9
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.ItemId, Is.EqualTo(10));
            Assert.That(dto.ItemGuid, Is.EqualTo(itemGuid));
            Assert.That(dto.MetadataLocationPinColour, Is.EqualTo("Blue"));
            Assert.That(dto.GlobalLocationTitle, Is.EqualTo("Title"));
            Assert.That(dto.GlobalLocationAddress1, Is.EqualTo("Address1"));
            Assert.That(dto.GlobalLocationAddress2, Is.EqualTo("Address2"));
            Assert.That(dto.GlobalLocationCity, Is.EqualTo("City"));
            Assert.That(dto.GlobalLocationState, Is.EqualTo("State"));
            Assert.That(dto.GlobalLocationCountry, Is.EqualTo("Country"));
            Assert.That(dto.GlobalLocationPostalCode, Is.EqualTo("54321"));
            Assert.That(dto.GlobalLocationRegion, Is.EqualTo("Region"));
            Assert.That(dto.GlobalLocationLatitude, Is.EqualTo(9.87m));
            Assert.That(dto.GlobalLocationLongitude, Is.EqualTo(6.54m));
            Assert.That(dto.GlobalLocationUrl, Is.EqualTo("https://example.com/api"));
            Assert.That(dto.MemberFirmId, Is.EqualTo(5));
            Assert.That(dto.ChannelId, Is.EqualTo(6));
            Assert.That(dto.ItemCreatedBy, Is.EqualTo(7));
            Assert.That(dto.ItemCreatedWhen, Is.EqualTo(createdWhen));
            Assert.That(dto.ItemModifiedBy, Is.EqualTo(8));
            Assert.That(dto.ItemModifiedWhen, Is.EqualTo(modifiedWhen));
            Assert.That(dto.ItemOrder, Is.EqualTo(9));
        });
    }

    [Test]
    public void ApiDto_Properties_DefaultToNullOrDefaultValues()
    {
        var dto = new GlobalLocationCustomTableApiDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.ItemId, Is.EqualTo(0));
            Assert.That(dto.ItemGuid, Is.EqualTo(Guid.Empty));
            Assert.That(dto.MetadataLocationPinColour, Is.Null);
            Assert.That(dto.GlobalLocationTitle, Is.Null);
            Assert.That(dto.GlobalLocationAddress1, Is.Null);
            Assert.That(dto.GlobalLocationAddress2, Is.Null);
            Assert.That(dto.GlobalLocationCity, Is.Null);
            Assert.That(dto.GlobalLocationState, Is.Null);
            Assert.That(dto.GlobalLocationCountry, Is.Null);
            Assert.That(dto.GlobalLocationPostalCode, Is.Null);
            Assert.That(dto.GlobalLocationRegion, Is.Null);
            Assert.That(dto.GlobalLocationLatitude, Is.Null);
            Assert.That(dto.GlobalLocationLongitude, Is.Null);
            Assert.That(dto.GlobalLocationUrl, Is.Null);
            Assert.That(dto.MemberFirmId, Is.EqualTo(0));
            Assert.That(dto.ChannelId, Is.EqualTo(0));
            Assert.That(dto.ItemCreatedBy, Is.Null);
            Assert.That(dto.ItemCreatedWhen, Is.Null);
            Assert.That(dto.ItemModifiedBy, Is.Null);
            Assert.That(dto.ItemModifiedWhen, Is.Null);
            Assert.That(dto.ItemOrder, Is.Null);
        });
    }

    #endregion
}
