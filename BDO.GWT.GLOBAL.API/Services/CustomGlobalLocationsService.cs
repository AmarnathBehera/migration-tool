using BDO.GWT.GLOBAL.API.Dto;
using CMS.Core;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BDO.GWT.GLOBAL.API.Services;

public interface ICustomGlobalLocationsService
{
    Task<List<GlobalLocationCustomTableApiDto>> GetAllAsync(int? memberFirmId);
    Task<GlobalLocationCustomTableApiDto> CreateAsync(GlobalLocationCustomTableUpsertDto dto);
    Task<GlobalLocationCustomTableApiDto?> UpdateAsync(int memberFirmId, GlobalLocationCustomTableUpsertDto dto);
}

public class DuplicateMemberFirmException : Exception
{
    public DuplicateMemberFirmException(int memberFirmId)
        : base($"A location with MemberFirmID '{memberFirmId}' already exists.") { }
}

public class InvalidGlobalLocationRegionException : Exception
{
    public InvalidGlobalLocationRegionException(string? region)
        : base($"GlobalLocationRegion '{region}' is invalid. Allowed values are 1, 2 or 3. " +
            $"1 for AMERICAS, 2 for EMEA, and 3 for ASIA PACIFIC.") { }
}
public class GlobalLocationPersistenceException : Exception
{
    public GlobalLocationPersistenceException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class CustomGlobalLocationsService : ICustomGlobalLocationsService
{
    private const string MemberFirmIdParameter = "@MemberFirmID";
    private static readonly string[] AllowedRegions = { "1", "2", "3" };

    private readonly IConfiguration _configuration;
    private readonly IEventLogService eventLogService;

    public CustomGlobalLocationsService(
        IConfiguration configuration,
        IEventLogService eventLogService)
    {
        _configuration = configuration;
        this.eventLogService = eventLogService;
    }

    public async Task<List<GlobalLocationCustomTableApiDto>> GetAllAsync(int? memberFirmId)
    {
        var locations = new List<GlobalLocationCustomTableApiDto>();

        try
        {
            string connectionString = _configuration.GetConnectionString("CMSConnectionString")!;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
            SELECT
                ItemID, ItemGUID, MetadataLocationPinColour, GlobalLocationTitle, GlobalLocationAddress1,
                GlobalLocationAddress2, GlobalLocationCity, GlobalLocationState, GlobalLocationCountry,
                GlobalLocationPostalCode, GlobalLocationRegion, GlobalLocationLatitude, GlobalLocationLongitude,
                GlobalLocationUrl, MemberFirmID, ChannelID, ItemCreatedBy, ItemCreatedWhen,
                ItemModifiedBy, ItemModifiedWhen, ItemOrder
            FROM dbo.BDO_GlobalLocation_Custom
            WHERE (@MemberFirmID IS NULL OR MemberFirmID = @MemberFirmID)
            ORDER BY GlobalLocationTitle";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(MemberFirmIdParameter, SqlDbType.Int).Value = (object?)memberFirmId ?? DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                locations.Add(MapReaderToDto(reader));
            }
        }
        catch (Exception ex)
        {
            eventLogService.LogException("CustomGlobalLocationsService", "GetAllAsync", ex, ex.Message);
        }

        return locations;
    }

    public async Task<GlobalLocationCustomTableApiDto> CreateAsync(GlobalLocationCustomTableUpsertDto dto)
    {
        try
        {
            ValidateRegion(dto.GlobalLocationRegion);
            string connectionString = _configuration.GetConnectionString("CMSConnectionString")!;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            if (await MemberFirmIdExistsAsync(connection, dto.MemberFirmId))
            {
                throw new DuplicateMemberFirmException(dto.MemberFirmId);
            }

            const string sql = @"
            INSERT INTO dbo.BDO_GlobalLocation_Custom
                (ItemGUID, MetadataLocationPinColour, GlobalLocationTitle, GlobalLocationAddress1,
                 GlobalLocationAddress2, GlobalLocationCity, GlobalLocationState, GlobalLocationCountry,
                 GlobalLocationPostalCode, GlobalLocationRegion, GlobalLocationLatitude, GlobalLocationLongitude,
                 GlobalLocationUrl, MemberFirmID, ChannelID, ItemCreatedWhen, ItemModifiedWhen, ItemOrder)
            OUTPUT
                INSERTED.ItemID, INSERTED.ItemGUID, INSERTED.MetadataLocationPinColour, INSERTED.GlobalLocationTitle,
                INSERTED.GlobalLocationAddress1, INSERTED.GlobalLocationAddress2, INSERTED.GlobalLocationCity,
                INSERTED.GlobalLocationState, INSERTED.GlobalLocationCountry, INSERTED.GlobalLocationPostalCode,
                INSERTED.GlobalLocationRegion, INSERTED.GlobalLocationLatitude, INSERTED.GlobalLocationLongitude,
                INSERTED.GlobalLocationUrl, INSERTED.MemberFirmID, INSERTED.ChannelID, INSERTED.ItemCreatedBy,
                INSERTED.ItemCreatedWhen, INSERTED.ItemModifiedBy, INSERTED.ItemModifiedWhen, INSERTED.ItemOrder
            VALUES
                (@ItemGuid, @MetadataLocationPinColour, @GlobalLocationTitle, @GlobalLocationAddress1,
                 @GlobalLocationAddress2, @GlobalLocationCity, @GlobalLocationState, @GlobalLocationCountry,
                 @GlobalLocationPostalCode, @GlobalLocationRegion, @GlobalLocationLatitude, @GlobalLocationLongitude,
                 @GlobalLocationUrl, @MemberFirmID, @ChannelID, @ItemCreatedWhen, @ItemModifiedWhen, @ItemOrder)";

            var newGuid = Guid.NewGuid();
            var now = DateTime.UtcNow;

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ItemGuid", SqlDbType.UniqueIdentifier).Value = newGuid;
            command.Parameters.Add("@MetadataLocationPinColour", SqlDbType.NVarChar).Value = (object?)dto.MetadataLocationPinColour ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationTitle", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationTitle ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationAddress1", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationAddress1 ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationAddress2", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationAddress2 ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationCity", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationCity ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationState", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationState ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationCountry", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationCountry ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationPostalCode", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationPostalCode ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationRegion", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationRegion ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationLatitude", SqlDbType.Decimal).Value = (object?)dto.GlobalLocationLatitude ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationLongitude", SqlDbType.Decimal).Value = (object?)dto.GlobalLocationLongitude ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationUrl", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationUrl ?? DBNull.Value;
            command.Parameters.Add(MemberFirmIdParameter, SqlDbType.Int).Value = dto.MemberFirmId;
            command.Parameters.Add("@ChannelID", SqlDbType.Int).Value = dto.ChannelId;
            command.Parameters.Add("@ItemCreatedWhen", SqlDbType.DateTime2).Value = now;
            command.Parameters.Add("@ItemModifiedWhen", SqlDbType.DateTime2).Value = now;
            command.Parameters.Add("@ItemOrder", SqlDbType.Int).Value = (object?)dto.ItemOrder ?? DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();

            return MapReaderToDto(reader);
        }
        catch (InvalidGlobalLocationRegionException)
        {
            throw;
        }
        catch (DuplicateMemberFirmException)
        {
            // Known business rule violation - let it bubble up as-is so the
            // controller can map it to 409 Conflict, don't wrap/log as an error.
            throw;
        }
        catch (Exception ex)
        {
            eventLogService.LogException("CustomGlobalLocationsService", "CreateAsync", ex, ex.Message);
            throw new GlobalLocationPersistenceException("Failed to create global location.", ex);
        }
    }

    public async Task<GlobalLocationCustomTableApiDto?> UpdateAsync(int memberFirmId, GlobalLocationCustomTableUpsertDto dto)
    {
        try
        {
            ValidateRegion(dto.GlobalLocationRegion);
            string connectionString = _configuration.GetConnectionString("CMSConnectionString")!;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            if (dto.MemberFirmId != memberFirmId &&
                await MemberFirmIdExistsAsync(connection, dto.MemberFirmId))
            {
                throw new DuplicateMemberFirmException(dto.MemberFirmId);
            }

            const string sql = @"
            UPDATE dbo.BDO_GlobalLocation_Custom
            SET
                MetadataLocationPinColour = @MetadataLocationPinColour,
                GlobalLocationTitle = @GlobalLocationTitle,
                GlobalLocationAddress1 = @GlobalLocationAddress1,
                GlobalLocationAddress2 = @GlobalLocationAddress2,
                GlobalLocationCity = @GlobalLocationCity,
                GlobalLocationState = @GlobalLocationState,
                GlobalLocationCountry = @GlobalLocationCountry,
                GlobalLocationPostalCode = @GlobalLocationPostalCode,
                GlobalLocationRegion = @GlobalLocationRegion,
                GlobalLocationLatitude = @GlobalLocationLatitude,
                GlobalLocationLongitude = @GlobalLocationLongitude,
                GlobalLocationUrl = @GlobalLocationUrl,
                MemberFirmID = @NewMemberFirmID,
                ChannelID = @ChannelID,
                ItemModifiedWhen = @ItemModifiedWhen,
                ItemOrder = @ItemOrder
            OUTPUT
                INSERTED.ItemID, INSERTED.ItemGUID, INSERTED.MetadataLocationPinColour, INSERTED.GlobalLocationTitle,
                INSERTED.GlobalLocationAddress1, INSERTED.GlobalLocationAddress2, INSERTED.GlobalLocationCity,
                INSERTED.GlobalLocationState, INSERTED.GlobalLocationCountry, INSERTED.GlobalLocationPostalCode,
                INSERTED.GlobalLocationRegion, INSERTED.GlobalLocationLatitude, INSERTED.GlobalLocationLongitude,
                INSERTED.GlobalLocationUrl, INSERTED.MemberFirmID, INSERTED.ChannelID, INSERTED.ItemCreatedBy,
                INSERTED.ItemCreatedWhen, INSERTED.ItemModifiedBy, INSERTED.ItemModifiedWhen, INSERTED.ItemOrder
            WHERE MemberFirmID = @MemberFirmID";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(MemberFirmIdParameter, SqlDbType.Int).Value = memberFirmId;
            command.Parameters.Add("@NewMemberFirmID", SqlDbType.Int).Value = dto.MemberFirmId;
            command.Parameters.Add("@MetadataLocationPinColour", SqlDbType.NVarChar).Value = (object?)dto.MetadataLocationPinColour ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationTitle", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationTitle ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationAddress1", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationAddress1 ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationAddress2", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationAddress2 ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationCity", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationCity ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationState", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationState ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationCountry", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationCountry ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationPostalCode", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationPostalCode ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationRegion", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationRegion ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationLatitude", SqlDbType.Decimal).Value = (object?)dto.GlobalLocationLatitude ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationLongitude", SqlDbType.Decimal).Value = (object?)dto.GlobalLocationLongitude ?? DBNull.Value;
            command.Parameters.Add("@GlobalLocationUrl", SqlDbType.NVarChar).Value = (object?)dto.GlobalLocationUrl ?? DBNull.Value;
            command.Parameters.Add("@ChannelID", SqlDbType.Int).Value = dto.ChannelId;
            command.Parameters.Add("@ItemModifiedWhen", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            command.Parameters.Add("@ItemOrder", SqlDbType.Int).Value = (object?)dto.ItemOrder ?? DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToDto(reader);
            }
            return null;
        }
        catch (InvalidGlobalLocationRegionException)
        {
            throw;
        }
        catch (DuplicateMemberFirmException)
        {
            throw;
        }
        catch (Exception ex)
        {
            eventLogService.LogException("CustomGlobalLocationsService", "UpdateAsync", ex, ex.Message);
            throw new GlobalLocationPersistenceException($"Failed to update global location for MemberFirmID '{memberFirmId}'.", ex);
        }
    }

    private static void ValidateRegion(string? region)
    {
        if (!AllowedRegions.Contains(region.Trim()))
        {
            throw new InvalidGlobalLocationRegionException(region);
        }
    }
    private static async Task<bool> MemberFirmIdExistsAsync(SqlConnection connection, int memberFirmId)
    {
        const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.BDO_GlobalLocation_Custom
            WHERE MemberFirmID = @MemberFirmID
        ) THEN 1 ELSE 0 END";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(MemberFirmIdParameter, SqlDbType.Int).Value = memberFirmId;

        var result = await command.ExecuteScalarAsync();
        return result is int exists && exists == 1;
    }

    private static GlobalLocationCustomTableApiDto MapReaderToDto(SqlDataReader reader)
    {
        return new GlobalLocationCustomTableApiDto
        {
            ItemId = (int)reader["ItemID"],
            ItemGuid = (Guid)reader["ItemGUID"],
            MetadataLocationPinColour = reader["MetadataLocationPinColour"]?.ToString(),
            GlobalLocationTitle = reader["GlobalLocationTitle"]?.ToString(),
            GlobalLocationAddress1 = reader["GlobalLocationAddress1"]?.ToString(),
            GlobalLocationAddress2 = reader["GlobalLocationAddress2"]?.ToString(),
            GlobalLocationCity = reader["GlobalLocationCity"]?.ToString(),
            GlobalLocationState = reader["GlobalLocationState"]?.ToString(),
            GlobalLocationCountry = reader["GlobalLocationCountry"]?.ToString(),
            GlobalLocationPostalCode = reader["GlobalLocationPostalCode"]?.ToString(),
            GlobalLocationRegion = reader["GlobalLocationRegion"]?.ToString(),
            GlobalLocationLatitude =
                reader["GlobalLocationLatitude"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["GlobalLocationLatitude"]),
            GlobalLocationLongitude =
                reader["GlobalLocationLongitude"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["GlobalLocationLongitude"]),
            GlobalLocationUrl = reader["GlobalLocationUrl"]?.ToString(),
            MemberFirmId = (int)reader["MemberFirmID"],
            ChannelId = (int)reader["ChannelID"],
            ItemCreatedBy =
                reader["ItemCreatedBy"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["ItemCreatedBy"]),
            ItemCreatedWhen =
                reader["ItemCreatedWhen"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["ItemCreatedWhen"]),
            ItemModifiedBy =
                reader["ItemModifiedBy"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["ItemModifiedBy"]),
            ItemModifiedWhen =
                reader["ItemModifiedWhen"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["ItemModifiedWhen"]),
            ItemOrder =
                reader["ItemOrder"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["ItemOrder"]),
        };
    }
}