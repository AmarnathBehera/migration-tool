using BDO.GWT.Domain.Core.Entities;
using CMS.ContentEngine;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO;

public partial class Webinar : IEventBase, IEventOrWebinar
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
        get => [];
        set { }
    }

    // Webinar.generated.cs stores these as IEnumerable<AboutPage> — expose as IWebPageFieldsSource
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

    IEnumerable<Businessissues> IEventOrWebinar.MetadataBusinessIssues
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
