using System;
using System.Security.Principal;

namespace BDO.GWT.Domain.Entities;

public class Setting : DomainEntity, ICodeNameEntity, IGuidEntity, IOptionalSiteEntity
{
    public string CodeName { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public object? Value { get; set; }

    public FieldType Type { get; set; }

    public int CategoryID { get; set; }

    public int? SiteID { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }

    public int Order { get; set; }

    public object? DefaultValue { get; set; }

    public string? Validation { get; set; }

    public string EditingControlPath { get; set; }

    public bool IsGlobal { get; set; }

    public bool IsCustom { get; set; }

    public bool IsHidden { get; set; }

    public string FormControlSettings { get; set; }

    public string ExplanationText { get; set; }
}

public enum FieldType
{
    Boolean,
    Double,
    Int,
    LongText,
    String,
}
