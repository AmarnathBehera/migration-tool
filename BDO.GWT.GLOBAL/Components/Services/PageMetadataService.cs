using BDO.GWT.Xperience;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface IPageMetadataService
{
    //public WebPageDataContext GetPageTitle();

    //public Task<string> GetPageDescriptionAsync();

    //public string GetPageKeywords();
}

public class PageMetadataService : IPageMetadataService
{


    private readonly IInfoProvider<GlobalsettingskeyInfo> _infoprovider;
    private const string MetadataDescriptionLength = "BDO_MetadataDescriptionLength";
    private const string MetaTrimDescriptionToOneSentence = "BDO_MetadataDescriptionFirstSentence";
    public PageMetadataService( IInfoProvider<GlobalsettingskeyInfo> infoProvider)
    {

        _infoprovider = infoProvider;
    }


    public async Task<string> GetPageDescriptionAsync()
    {
        var metadataDescLengthObj = _infoprovider.Get()
            .WhereEquals(
                nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                MetadataDescriptionLength)
            .TopN(1)
            .GetEnumerableTypedResult()
            ?.FirstOrDefault();

        var trimDescToOneSentenceObj = _infoprovider.Get()
            .WhereEquals(
                nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                MetaTrimDescriptionToOneSentence)
            .TopN(1)
            .GetEnumerableTypedResult()
            ?.FirstOrDefault();

        //var description = PageMetadataUtility.GetFallbackMetadataDescription(_pageContext);
        var description = "This is a temp placeholder";

        if (metadataDescLengthObj != null &&
            int.TryParse(metadataDescLengthObj.GlobalSettingsKeyValue, out int metadataDescriptionLengthInt) &&
            metadataDescriptionLengthInt > 0 &&
            metadataDescriptionLengthInt < description.Length)
        {
            description = description[..metadataDescriptionLengthInt];
        }

        if (trimDescToOneSentenceObj != null &&
            bool.TryParse(trimDescToOneSentenceObj.GlobalSettingsKeyValue, out bool trimDescriptionToOneSentence) &&
            trimDescriptionToOneSentence)
        {
            var indexOfFirstPeriod = description.IndexOf('.');
            if (indexOfFirstPeriod > 0)
            {
                description = description[..indexOfFirstPeriod];
            }
        }

        return await Task.FromResult(description);
    }


}
