namespace BDO.GWT.Core.DTOs
{
    public class GlobalLocationCustomTableDto
    {

        public int ItemId { get; set; }

        public Guid ItemGuid { get; set; }

        public int MemberFirmId { get; set; }

        public int ChannelId { get; set; }
        public string? GlobalLocationTitle { get; set; }
        public string? GlobalLocationAddress1 { get; set; }
        public string? GlobalLocationCity { get; set; }
        public string? GlobalLocationCountry { get; set; }
        public decimal? GlobalLocationLatitude { get; set; }
        public decimal? GlobalLocationLongitude { get; set; }
        public string? GlobalLocationUrl { get; set; }


    }
}
