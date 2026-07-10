using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using CMS.Core;


namespace BDO.GWT.Core.Services;
{
    public interface IGlobalLocationCustomTableDataService
    {
        Task<GlobalLocationCustomTableDto> GetLocationsAsync(int? memberFirmId);
    }
    public class GlobalLocationCustomTableDataService : IGlobalLocationCustomTableDataService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventLogService eventLogService;

        public GlobalLocationCustomTableDataService(IConfiguration configuration, IEventLogService eventLogService)
        {
            _configuration = configuration;
            this.eventLogService = eventLogService;
        }


        public async Task<GlobalLocationCustomTableDto?> GetLocationsAsync(int? memberFirmId)
        {
            var locations = new List<GlobalLocationCustomTableDto>();

            try
            {
                string connectionString =
                       _configuration.GetConnectionString("CMSConnectionString")!;


                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync();

                const string sql = @"
            SELECT
                ItemID,
                ItemGuid,
                MemberFirmID,
                ChannelID,
                GlobalLocationTitle,
                GlobalLocationAddress1,
                GlobalLocationCity,
                GlobalLocationCountry,
                GlobalLocationUrl,
                GlobalLocationLatitude,
                GlobalLocationLongitude
            FROM dbo.BDO_GlobalLocation_Custom
            WHERE MemberFirmID = @MemberFirmID
            ORDER BY GlobalLocationTitle";

                await using var command = new SqlCommand(sql, connection);

                command.Parameters.Add(
                    "@MemberFirmID",
                    SqlDbType.Int).Value = memberFirmId;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    locations.Add(new GlobalLocationCustomTableDto
                    {
                        ItemId = (int)reader["ItemID"],
                        ItemGuid = (Guid)reader["ItemGuid"],
                        MemberFirmId = (int)reader["MemberFirmID"],
                        ChannelId = (int)reader["ChannelID"],
                        GlobalLocationTitle = reader["GlobalLocationTitle"]?.ToString(),
                        GlobalLocationAddress1 = reader["GlobalLocationAddress1"]?.ToString(),
                        GlobalLocationCity = reader["GlobalLocationCity"]?.ToString(),
                        GlobalLocationCountry = reader["GlobalLocationCountry"]?.ToString(),
                        GlobalLocationUrl = reader["GlobalLocationUrl"]?.ToString(),
                        GlobalLocationLatitude =
                            reader["GlobalLocationLatitude"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["GlobalLocationLatitude"]),
                        GlobalLocationLongitude =
                            reader["GlobalLocationLongitude"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["GlobalLocationLongitude"])
                    });
                }

            }
            catch (Exception ex) 
            {
                eventLogService.LogException("GlobalLocationCustomTableDataService", "GetLocationsAsync",ex, ex.Message);
            }

           
            if(locations.Count > 0)            
                return locations.First();
            else
                return new GlobalLocationCustomTableDto();
        }

    }
}
