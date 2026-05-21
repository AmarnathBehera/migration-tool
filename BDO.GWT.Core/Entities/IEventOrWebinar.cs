using System;
using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Entities;

public interface IEventOrWebinar : IOfficeTaggablePage
{
    public string MetadataTitle { get; set; }
    
    public DateTime EventDateFrom { get; set; }

    public DateTime EventDateTo { get; set; }

    public bool EventIsPrivate { get; set; }

    public string? EventLocationCity { get; set; }

    public string? MetadataDescription { get; set; }

    public List<int> MetadataServices { get; set; }

    public List<int> MetadataBusinessLines { get; set; }

    public List<int> MetadataServiceAreas { get; set; }

    public List<int> MetadataSpecialtiesCategories { get; set; }

    public List<int> MetadataSpecialtiesAreas { get; set; }

    public List<int> MetadataSpecialtiesPages { get; set; }

    public List<int> MicrositeMasterPages { get; set; }

    public List<int> MicrositePages { get; set; }

    public List<int> MetadataIndustries { get; set; }

    public List<int> MetadataIndustryCategories { get; set; }

    public new List<int> MetadataOfficeLocation { get; set; }

    public List<int> EventType { get; set; }

    public List<int> MetadataBusinessIssues { get; set; }

    public bool ShowPastEvent { get; set; }
}
