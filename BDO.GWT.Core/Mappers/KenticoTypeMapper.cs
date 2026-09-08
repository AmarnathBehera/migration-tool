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

        
        static KenticoTypeMapper()
        {
            // Scan all loaded assemblies for non-abstract, non-interface types that implement IWebPageFieldsSource
            // (i.e., generated Kentico Xperience page type classes).
            // Some third-party assemblies (e.g. Azure.AI.OpenAI when version-mismatched) fail to load all
            // of their types. Catch ReflectionTypeLoadException and use the types that did load so a single
            // bad assembly cannot break the whole mapper's type initializer.
            var pageTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t is not null).ToArray()!;
                }
                catch (Exception)
                {
                    // Dynamic / unloadable assemblies - skip entirely.
                    continue;
                }

                pageTypes.AddRange(types.Where(t =>
                    typeof(IWebPageFieldsSource).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface));
            }

            foreach (var pageType in pageTypes)
            {
                // Each generated page type class exposes a public static CONTENT_TYPE_NAME field
                // containing the Kentico content type's class name.
                var classNameField = pageType.GetField("CONTENT_TYPE_NAME", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (classNameField?.GetValue(null) is string className)
                {
                    // Build lookups in both directions so the mapper can resolve
                    // a page type from a class name and vice versa.
                    PageClassNameByPageType[pageType] = className;
                    PageTypeByPageClassName[className] = pageType;
                }
            }
        }
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
   }
