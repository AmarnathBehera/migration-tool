// Event.cs
using BDO.GWT.Core.Constants;
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




    IEnumerable<BusinessLine> IEventOrWebinar.MetadataBusinessLines
    {
        get => MetadataBusinessLine ?? [];
        set => MetadataBusinessLine = value;
    }
    IEnumerable<EventType> IEventOrWebinar.EventType
    {
        get => [];
        set => _ = value;
    }
    // Event.generated.cs does not have specialties/business-issue/office fields
    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesCategories
    {
        get => [];
        set => _ = value;
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesAreas
    {
        get => [];
        set => _ = value;
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesPages
    {
        get => [];
        set => _ = value;
    }
    IEnumerable<Businessissues> IEventOrWebinar.MetadataBusinessIssues
    {
        get => [];
        set => _ = value;
    }

    IEnumerable<LocationOffice> IEventOrWebinar.MetadataOfficeLocation
    {
        get => [];
        set => _ = value;
    }

}