using CMS.ContentEngine;

namespace BDO.GWT.Core.Data;

public sealed record EditorsPickWidgetState(
    HashSet<ContentItemReference> FeaturedGuids,
    HashSet<ContentItemReference> SelectedGuids);
