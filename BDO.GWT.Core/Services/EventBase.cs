// EventBase.cs
using System;
using System.Collections.Generic;
using CMS.ContentEngine;

namespace BDO;

public partial class EventBase : IEventBase
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
}