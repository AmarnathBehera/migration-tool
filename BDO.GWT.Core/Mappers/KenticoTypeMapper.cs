using AutoMapper;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Domain.Core.Entities;
using CMS.ContactManagement;
using CMS.ContentEngine;
using CMS.Globalization;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.Scheduler;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Mappers
{
    public interface IKenticoTypeMapper
    {        
        string GetClassNameFromPageType<TPage>() where TPage : IWebPageFieldsSource;             
   
        Type GetTypeFromPageClassName(string pageClassName);
               
        Type GetTypeFromPageClassNameInvariant(string pageClassName);

        string GetClassNameFromPageType(Type pageType);

    }
    public class KenticoTypeMapper : IKenticoTypeMapper
    {
        private static readonly IDictionary<Type, string> PageClassNameByPageType = new Dictionary<Type, string>();
        private static readonly IDictionary<string, Type> PageTypeByPageClassName = new Dictionary<string, Type>();

        //static KenticoTypeMapper()
        //{
        //    foreach (var (pageType, data) in DataByPageType)
        //    {
        //        if (data.TreeNodeType != null)
        //        {
        //            PageTypeByTreeNodeType[data.TreeNodeType] = pageType;
        //        }

        //        if (data.ClassName != null)
        //        {
        //            PageClassNameByPageType[pageType] = data.ClassName;
        //            PageTypeByPageClassName[data.ClassName] = pageType;
        //        }
        //    }
        //}
        public string GetClassNameFromPageType(Type pageType)
        {
            if (PageClassNameByPageType.TryGetValue(pageType, out var className))
            {
                return className;
            }

            throw new NotSupportedException($"'{pageType.Name}' -> TTreeNode.CLASS_NAME map is not supported.");
        }

        public string GetClassNameFromPageType<TPage>() where TPage : IWebPageFieldsSource
        {
            return GetClassNameFromPageType(typeof(TPage));
        }

        public Type GetTypeFromPageClassName(string pageClassName)
        {
            if (PageTypeByPageClassName.TryGetValue(pageClassName, out var pageType))
            {
                return pageType;
            }

            throw new NotSupportedException($"'TTreeNode.{pageClassName}' -> TPage map is not supported.");
        }

        public Type GetTypeFromPageClassNameInvariant(string pageClassName)
        {
            var pageType = PageTypeByPageClassName.FirstOrDefault(x => x.Key.Equals(pageClassName, StringComparison.InvariantCultureIgnoreCase)).Value;

            if (pageType is null)
            {
                throw new NotSupportedException($"'TTreeNode.{pageClassName}' -> TPage map is not supported.");
            }
            return pageType;
        }
        
    }

     

        //public class SearchDocumentProfile : Profile
        //{
        //    public SearchDocumentProfile()
        //    {
        //        CreateMap<SearchDocument, AzureSearchDocument>()
        //            // Same-named properties (ClassName, Content, EventDateFrom, EventDateTo,
        //            // EventIsPrivate, ...) are mapped automatically by convention.
        //            // Map only the members whose names differ between the two types:
        //            .ForMember(dest => dest.DocumentID, opt => opt.MapFrom(src => src.DocumentId))
        //            .ForMember(dest => dest.DocumentNodeID, opt => opt.MapFrom(src => src.NodeId))
        //            .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Title))
        //            .ForMember(dest => dest.DocumentPageTitle, opt => opt.MapFrom(src => src.MetadataTitle))
        //            .ForMember(dest => dest.DocumentPageDescription, opt => opt.MapFrom(src => src.MetadataDescription))
        //            .ForMember(dest => dest.DocumentPublishFrom, opt => opt.MapFrom(src => src.PublishFrom))
        //            .ForMember(dest => dest.DocumentPublishTo, opt => opt.MapFrom(src => src.PublishTo))
        //            .ForMember(dest => dest.DocumentCreatedWhen, opt => opt.MapFrom(src => src.CreatedWhen));
        //        // Add/adjust ForMember lines for every SearchDocument property that your
        //        // queries reference (Where/OrderBy/Select/facets), e.g. ChannelName,
        //        // LanguageTag, NodeAliasPath, EventType, EventLocationTitle, InsightContentType,
        //        // SortDate, EventLocationCity — each mapped to its AzureSearchDocument counterpart.
        //    }
        //}
    }
