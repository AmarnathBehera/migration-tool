using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.Dto;
public class GlobalLocationCustomTableApiDto
{
    public int ItemId { get; set; }
    public Guid ItemGuid { get; set; }
    public string? MetadataLocationPinColour { get; set; }
    public string? GlobalLocationTitle { get; set; }
    public string? GlobalLocationAddress1 { get; set; }
    public string? GlobalLocationAddress2 { get; set; }
    public string? GlobalLocationCity { get; set; }
    public string? GlobalLocationState { get; set; }
    public string? GlobalLocationCountry { get; set; }
    public string? GlobalLocationPostalCode { get; set; }
    public string? GlobalLocationRegion { get; set; }
    public decimal? GlobalLocationLatitude { get; set; }
    public decimal? GlobalLocationLongitude { get; set; }
    public string? GlobalLocationUrl { get; set; }
    public int MemberFirmId { get; set; }
    public int ChannelId { get; set; }
    public int? ItemCreatedBy { get; set; }
    public DateTime? ItemCreatedWhen { get; set; }
    public int? ItemModifiedBy { get; set; }
    public DateTime? ItemModifiedWhen { get; set; }
    public int? ItemOrder { get; set; }
}

public class GlobalLocationCustomTableUpsertDto
{
    public string? MetadataLocationPinColour { get; set; }
    public string? GlobalLocationTitle { get; set; }
    public string? GlobalLocationAddress1 { get; set; }
    public string? GlobalLocationAddress2 { get; set; }
    public string? GlobalLocationCity { get; set; }
    public string? GlobalLocationState { get; set; }
    public string? GlobalLocationCountry { get; set; }
    public string? GlobalLocationPostalCode { get; set; }
    public string? GlobalLocationRegion { get; set; }
    public decimal? GlobalLocationLatitude { get; set; }
    public decimal? GlobalLocationLongitude { get; set; }
    public string? GlobalLocationUrl { get; set; }
    [JsonRequired]
    public int MemberFirmId { get; set; }
    [JsonRequired]
    public int ChannelId { get; set; }
    public int? ItemOrder { get; set; }
}