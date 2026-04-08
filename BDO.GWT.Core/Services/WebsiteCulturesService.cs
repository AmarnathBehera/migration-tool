using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services
{
    public interface IWebsiteCulturesService
    {
        public Task<IEnumerable<CultureDto?>> GetWebsiteCulturesAsync();
        public string GetDisplayName(IWebsiteChannelContext websiteChannel);


    }

    public class WebsiteCulturesService : IWebsiteCulturesService
    {
        public Task<IEnumerable<CultureDto?>> GetWebsiteCulturesAsync()
        {
            var provider = new ContentLanguageInfoProvider();

            var languages = provider.Get().ToList();

            var cultureDtos = languages.Select(cl =>
            {
                var languageTag = cl.ContentLanguageName; 
                var name = cl.ContentLanguageDisplayName;
                var shortName = languageTag.Split('-')[0];

                // Get URL if exists in dictionary, otherwise fallback
                //var url = linkByLanguageTag.TryGetValue(languageTag, out var link)
                //    ? link
                //    : $"/{languageTag}".ToLowerInvariant();

                return new CultureDto
                {
                    Name = name,
                    ShortName = shortName,
                    //Url =linkByLanguageTag.TryGetValue(websiteCulture.LanguageTag, out var link) ? link.ToHtmlCompatibleString() : $"/{websiteCulture.LanguageTag}".ToLowerInvariant(),
                };
            }).ToList();

            return Task.FromResult<IEnumerable<CultureDto?>>(cultureDtos);
        }
        public string GetDisplayName(IWebsiteChannelContext websiteChannel)
        {
            return websiteChannel.WebsiteChannelName ?? string.Empty;
        }
    }
}

