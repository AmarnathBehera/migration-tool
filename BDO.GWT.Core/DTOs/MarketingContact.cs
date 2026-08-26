using System;

namespace BDO.GWT.Domain.Core.Entities;

public class MarketingContact : DomainEntity, ISiteEntity
{
    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateTime Birthday { get; set; }

    public int Gender { get; set; }

    public string? CompanyName { get; set; }

    public string? JobTitle { get; set; }

    public DateTime Created { get; set; }

    public int StatusID { get; set; }

    public bool Monitored { get; set; }

    public int OwnerUserID { get; set; }

    public string? Campaign { get; set; }

    public int PersonaID { get; set; }

    public string? Address1 { get; set; }

    public string? City { get; set; }

    public string? ZIP { get; set; }

    public int StateID { get; set; }

    public int CountryID { get; set; }

    public string? MobilePhone { get; set; }

    public string? BusinessPhone { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }

    public int Bounces { get; set; }

    public int SiteID { get; set; }
}
