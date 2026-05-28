// Event.cs
using BDO.GWT.Domain.Core.Entities;
using CMS.ContentEngine;
using CMS.Websites;
using System;
using System.Collections.Generic;

namespace BDO;

public partial class Event : IEventBase, IEventOrWebinar
{
    public IEnumerable<ContentAttachment> Image
    {
        get => MetadataTeaserImage;
        set => MetadataTeaserImage = value;
    }

    public string? ImageAltText
    {
        get => MetadataTeaserImageAltText;
        set => MetadataTeaserImageAltText = value;
    }

    public bool ShowInWidget => !EventIsPrivate;

    public DateTime? RelatedContentDate
        => EventDateFrom == default ? null : EventDateFrom;

    public DateTime? RelatedContentDateTo
        => EventDateTo == default ? null : EventDateTo;

    public string? RelatedContentDateOverride => EventDateOverride;

    public IEnumerable<TagReference> Insights
    {
        get => [];
        set => _ = value;
    }

    // ---- IEventOrWebinar explicit implementations ----

    // Event.generated.cs has MetadataBusinessLine (singular IEnumerable<BusinessLine>)
    IEnumerable<BusinessLine> IEventOrWebinar.MetadataBusinessLines
    {
        get => MetadataBusinessLine ?? [];
        set => MetadataBusinessLine = value;
    }

    // Event.generated.cs does not have specialties/business-issue/office fields
    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesCategories
    {
        get => [];
        set { }
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesAreas
    {
        get => [];
        set { }
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesPages
    {
        get => [];
        set { }
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataBusinessIssues
    {
        get => [];
        set { }
    }

    IEnumerable<LocationOffice> IEventOrWebinar.MetadataOfficeLocation
    {
        get => [];
        set { }
    }
}