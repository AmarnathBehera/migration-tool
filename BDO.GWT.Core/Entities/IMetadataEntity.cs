using BDO.GWT.Core.ValueObjects;
using System;

namespace BDO.GWT.Domain.Core.Entities;

public interface IMetadataEntity
{
    string Name { get; set; }

    string CodeName { get; set; }

    LanguageTag LanguageTag { get; set; }

    int SiteID { get; set; }

    Guid GUID { get; set; }

    DateTime LastModified { get; set; }
}
