using BDO.GWT.Core.ValueObjects;
using System;

namespace BDO.GWT.Domain.Core.Entities.BDOMetadata;

public class EventType : IEntity, IMetadataEntity, ISiteEntity
{
    public virtual int ID { get; set; }
    public string Name { get; set; } = null!;

    public string CodeName { get; set; } = null!;

    public LanguageTag LanguageTag { get; set; } = null!;

    public string? CallToActionText { get; set; } // This field is required but nullable because the field was not available in K12 and old entries might not have the value.

    public int SiteID { get; set; }

    public Guid GUID { get; set; }

    public DateTime LastModified { get; set; }

}
