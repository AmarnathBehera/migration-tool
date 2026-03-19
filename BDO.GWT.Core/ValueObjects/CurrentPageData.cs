using System;

namespace BDO.GWT.Core.ValueObjects;

public interface ICurrentPageData
{
    int PageID { get; }

    Guid PageGUID { get; }

    int LanguageID { get; }

    string LanguageName { get; }

    int ContentTypeID { get; }

    string ContentTypeName { get; }

    int WebsiteID { get; }

    string WebsiteChannelName { get; }
}

public record CurrentPageData : ICurrentPageData
{
    public required int PageID { get; init; }

    public required Guid PageGUID { get; init; }

    public required int LanguageID { get; init; }

    public required string LanguageName { get; init; }

    public required int ContentTypeID { get; init; }

    public required string ContentTypeName { get; init; }

    public required int WebsiteID { get; init; }

    public required string WebsiteChannelName { get; init; }
}
