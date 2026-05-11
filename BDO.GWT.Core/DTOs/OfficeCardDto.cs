namespace BDO.GWT.Core.DTOs;

public class OfficeCardDto
{
    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public string Name { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string PostalCode { get; set; } = null!;

    public string PostalCodeDisplayType { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? OpeningHours { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? PoBoxNumber { get; set; }

    public string? PoCity { get; set; }

    public string? PoPostalCode { get; set; }

    public string PoPostalCodeDisplayType { get; set; } = null!;

    public string? PoCountry { get; set; }

    public string? GetDirectionsUrl { get; set; } = null!;
}
