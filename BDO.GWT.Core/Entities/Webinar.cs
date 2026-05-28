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

    public IEnumerable<TagReference> Insights
    {
        get => [];
        set => _ = value;
    }

    // ---- IEventOrWebinar explicit implementations ----

    // Webinar.generated.cs uses MetadataBusinessLines (plural, IEnumerable<BusinessLine>)
    IEnumerable<BusinessLine> IEventOrWebinar.MetadataBusinessLines
    {
        get => MetadataBusinessLines ?? [];
        set => MetadataBusinessLines = value;
    }

    // Webinar.generated.cs stores these as IEnumerable<AboutPage> — expose as IWebPageFieldsSource
    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesCategories
    {
        get => MetadataSpecialtiesCategories?.Cast<IWebPageFieldsSource>() ?? [];
        set => MetadataSpecialtiesCategories = value?.Cast<AboutPage>() ?? [];
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesAreas
    {
        get => MetadataSpecialtiesAreas?.Cast<IWebPageFieldsSource>() ?? [];
        set => MetadataSpecialtiesAreas = value?.Cast<AboutPage>() ?? [];
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataSpecialtiesPages
    {
        get => MetadataSpecialtiesPages?.Cast<IWebPageFieldsSource>() ?? [];
        set => MetadataSpecialtiesPages = value?.Cast<AboutPage>() ?? [];
    }

    IEnumerable<IWebPageFieldsSource> IEventOrWebinar.MetadataBusinessIssues
    {
        get => MetadataBusinessIssues?.Cast<IWebPageFieldsSource>() ?? [];
        set => MetadataBusinessIssues = value?.Cast<AboutPage>() ?? [];
    }

    IEnumerable<LocationOffice> IEventOrWebinar.MetadataOfficeLocation
    {
        get => MetadataOfficeLocation ?? [];
        set => MetadataOfficeLocation = value;
    }
}
