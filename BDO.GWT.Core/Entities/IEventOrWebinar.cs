using BDO;
using CMS.ContentEngine;
using CMS.Websites;
using System;
using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Entities;

public interface IEventOrWebinar : IWebPageFieldsSource
{
    string MetadataTitle { get; set; }

    DateTime EventDateFrom { get; set; }

    DateTime EventDateTo { get; set; }

    bool EventIsPrivate { get; set; }

    string? EventLocationCity { get; set; }

    string? MetadataDescription { get; set; }

    IEnumerable<Service> MetadataServices { get; set; }

    IEnumerable<BusinessLine> MetadataBusinessLines { get; set; }

    IEnumerable<ServiceArea> MetadataServiceAreas { get; set; }

    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesCategories { get; set; }

    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesAreas { get; set; }

    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesPages { get; set; }

    IEnumerable<MicrositePlaceholder> MicrositeMasterPages { get; set; }

    IEnumerable<MicrositePlaceholder> MicrositePages { get; set; }

    IEnumerable<Industry> MetadataIndustries { get; set; }

    IEnumerable<IndustryCategory> MetadataIndustryCategories { get; set; }

    IEnumerable<LocationOffice> MetadataOfficeLocation { get; set; }

    // Taxonomy tag references
    IEnumerable<EventType> EventType { get; set; }

    IEnumerable<Businessissues> MetadataBusinessIssues { get; set; }

    bool ShowPastEvent { get; set; }
}
