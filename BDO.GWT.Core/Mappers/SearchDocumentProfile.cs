using AutoMapper;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;

namespace BDO.GWT.Core.Mappers
{
    public class SearchDocumentProfile : Profile
    {
        public SearchDocumentProfile()
        {
            CreateMap<SearchDocument, AzureSearchDocument>()
                .ForMember(d => d.DocumentPageTitle,
                    o => o.MapFrom(s => s.Title))
                .ForMember(d => d.MetadataDescription,
                    o => o.MapFrom(s => s.MetadataDescription))
                .ForMember(d => d.Url,
                    o => o.MapFrom(s => s.NodeAliasPath))
                .ForMember(
                    d => d.PublishFrom,
                    o => o.MapFrom(s =>
                    s.PublishFrom.HasValue &&
                    s.PublishFrom.Value > DateTimeOffset.MinValue
                        ? s.PublishFrom.Value.UtcDateTime
                        : (DateTime?)null))
                .ForMember(
                    d => d.PublishTo,
                    o => o.MapFrom(s =>
                    s.PublishTo.HasValue &&
                    s.PublishTo.Value > DateTimeOffset.MinValue
                        ? s.PublishTo.Value.UtcDateTime
                        : (DateTime?)null))
                .ForMember(d => d.ClassName,
                    o => o.MapFrom(s => s.ClassName))
                .ForMember(d => d.DocumentID,
                    o => o.MapFrom(s => s.DocumentId))
                .ForMember(d => d.NodeID,
                    o => o.MapFrom(s => s.NodeId))
                .ForMember(d => d.Content,
                    o => o.MapFrom(s => s.Content))
                .ForMember(d => d.MetadataTitle,
                    o => o.MapFrom(s => s.MetadataTitle))
                .ForMember(d => d.Image,
                    o => o.MapFrom(s => s.InsightImage))
                .ForMember(d => d.ImageAltText,
                    o => o.MapFrom(s => s.InsightImageAltText))
                .ForMember(d => d.SortDate,
                    o => o.MapFrom(s => s.SortDate))
               .ForMember(
                    d => d.EventDateFrom,
                    o => o.MapFrom(s =>
                    s.EventDateFrom.HasValue &&
                    s.EventDateFrom.Value > DateTimeOffset.MinValue
                        ? s.EventDateFrom.Value
                        : (DateTime?)null))
                .ForMember(d => d.EventDateTo,
                    o => o.MapFrom(s =>
                         s.EventDateTo.HasValue &&
                         s.EventDateTo.Value > DateTimeOffset.MinValue
                            ? s.EventDateTo.Value
                            : (DateTime?)null))
                .ForMember(d => d.EventLocationTitle,
                    o => o.MapFrom(s => s.EventLocationTitle))
                .ForMember(d => d.EventLocationCity,
                    o => o.MapFrom(s => s.EventLocationCity))
                .ForMember(d => d.EventIsPrivate,
                    o => o.MapFrom(s => s.EventIsPrivate))
                .ForMember(d => d.EventType,
                    o => o.MapFrom(s => s.EventType))
                .ForMember(d => d.SysCulture,
                    o => o.MapFrom(s => s.LanguageTag));

            CreateMap<AzureSearchDocument, SearchDocument>()
                .ForMember(d => d.Title,
                    o => o.MapFrom(s => FirstNotEmpty(s.DocumentPageTitle, s.MetadataTitle, s.DocumentName)))
                .ForMember(d => d.MetadataDescription,
                    o => o.MapFrom(s => FirstNotEmpty(s.MetadataDescription, s.DocumentPageDescription, s.DescriptionBody, s.Content)))
                .ForMember(d => d.NodeAliasPath,
                    o => o.MapFrom(s => s.Url))
                .ForMember(
                  d => d.PublishFrom,
                  o => o.MapFrom(s =>
                  s.PublishFrom.HasValue &&
                  s.PublishFrom.Value > DateTimeOffset.MinValue
                  ? s.PublishFrom.Value.UtcDateTime
                   : (DateTime?)null))
                .ForMember(
                 d => d.PublishTo,
                 o => o.MapFrom(s =>
                 s.PublishTo.HasValue &&
                 s.PublishTo.Value > DateTimeOffset.MinValue
                 ? s.PublishTo.Value.UtcDateTime
                    : (DateTime?)null))
                .ForMember(d => d.ClassName,
                    o => o.MapFrom(s => s.ClassName))
                .ForMember(d => d.DocumentId,
                    o => o.MapFrom(s => s.DocumentID ?? 0))
                .ForMember(d => d.NodeId,
                    o => o.MapFrom(s => s.DocumentNodeID ?? s.NodeID ?? 0))
                .ForMember(d => d.Content,
                    o => o.MapFrom(s => s.Content))
                .ForMember(d => d.MetadataTitle,
                    o => o.MapFrom(s => FirstNotEmpty(s.MetadataTitle, s.DocumentPageTitle, s.DocumentName)))
                 .ForMember(d => d.InsightImage,
                    o => o.MapFrom(s => s.Image == null ? null : s.Image))
                .ForMember(d => d.InsightImageAltText,
                    o => o.MapFrom(s => s.ImageAltText ?? string.Empty))
                .ForMember(d => d.SortDate,
                    o => o.MapFrom(s => s.SortDate))
                .ForMember(d => d.CreatedWhen,
                    o => o.MapFrom(s => s.DocumentCreatedWhen.HasValue ? s.DocumentCreatedWhen.Value.UtcDateTime : DateTime.MinValue))
               .ForMember(
                  d => d.EventDateFrom,
                  o => o.MapFrom(s =>
                  s.EventDateFrom.HasValue &&
                  s.EventDateFrom.Value > DateTimeOffset.MinValue
                  ? s.EventDateFrom.Value.UtcDateTime
                   : (DateTime?)null))
                .ForMember(d => d.EventDateTo,
                    o => o.MapFrom(s =>
                         s.EventDateTo.HasValue &&
                         s.EventDateTo.Value > DateTimeOffset.MinValue
                         ? s.EventDateTo.Value.UtcDateTime
                            : (DateTime?)null))
                .ForMember(d => d.EventLocationTitle,
                    o => o.MapFrom(s => s.EventLocationTitle))
                .ForMember(d => d.EventLocationCity,
                    o => o.MapFrom(s => s.EventLocationCity))
                .ForMember(d => d.EventIsPrivate,
                    o => o.MapFrom(s => s.EventIsPrivate))
                .ForMember(d => d.EventType,
                    o => o.MapFrom(s => s.EventType))
                .ForMember(d => d.LanguageTag,
                    o => o.MapFrom(s => s.SysCulture));
        }

        private static string? FirstNotEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

    }
}