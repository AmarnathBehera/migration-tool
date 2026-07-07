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

    /// <summary>
    /// Gets or sets the full name of the user, which is a combination of the first name, middle name, and last name. This property is used to display the user's complete name in the application and can be useful for identification and personalization purposes.
    /// </summary>
    public string? FullName { get; set; }

    public string? Email { get; set; }

    public LanguageTag? PreferredCultureCode { get; set; }

    /// <summary>
    /// Gets or sets the preferred user interface culture code for the user. This property is used to determine the language and regional settings for the user interface, allowing the application to display content in the user's preferred language and format.
    /// </summary>
    public LanguageTag? PreferredUICultureCode { get; set; }

    public bool Enabled { get; set; }

    public bool? IsExternal { get; set; }

    public string? PasswordFormat { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? LastLogon { get; set; }

    public string? StartingAliasPath { get; set; }

    /// <summary>
    /// Gets or sets the globally unique identifier (GUID) for the user. This property is used to uniquely identify the user across different systems and databases, ensuring that each user can be referenced without ambiguity.
    /// </summary>
    public Guid GUID { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last modified. This property is used to track changes to the user's information and can be useful for auditing and synchronization purposes.
    /// </summary>
    public DateTime LastModified { get; set; }

    public string? LastLogonInfo { get; set; }

    public bool? IsHidden { get; set; }

    public bool? IsDomain { get; set; }

    public bool? HasAllowedCultures { get; set; }

    public bool? MFRequired { get; set; }

    /// <summary>
    /// Gets or sets the privilege level of the user. This property indicates the user's access rights and permissions within the system, which can be used to control what actions the user is allowed to perform.
    /// </summary>
    public UserPrivilegeLevel PrivilegeLevel { get; set; }

    public string? SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the multi-factor authentication (MFA) time step in seconds. This property is used to configure the time interval for generating MFA codes, which are typically valid for a short duration to enhance security.
    /// </summary>
    public long? MFTimestep { get; set; }
}
