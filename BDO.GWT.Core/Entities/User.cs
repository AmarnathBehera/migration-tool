using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.Core.ValueObjects;
using System;

namespace BDO.GWT.Domain.Core.Entities;

public class User : DomainEntity
{
    public string UserName { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public LanguageTag? PreferredCultureCode { get; set; }

    public LanguageTag? PreferredUICultureCode { get; set; }

    public bool Enabled { get; set; }

    public bool? IsExternal { get; set; }

    public string? PasswordFormat { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? LastLogon { get; set; }

    public string? StartingAliasPath { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastLogonInfo { get; set; }

    public bool? IsHidden { get; set; }

    public bool? IsDomain { get; set; }

    public bool? HasAllowedCultures { get; set; }

    public bool? MFRequired { get; set; }

    public UserPrivilegeLevel PrivilegeLevel { get; set; }

    public string? SecurityStamp { get; set; }

    public long? MFTimestep { get; set; }
}
