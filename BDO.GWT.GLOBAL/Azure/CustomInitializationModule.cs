using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.Websites.Routing;
using Kentico.Xperience.AzureStorage;


[assembly: RegisterModule(typeof(CustomInitializationModule))]

public class CustomInitializationModule : Module
{
    public CustomInitializationModule()
        : base("CustomInit")
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        var configuration = CMS.Core.Service.Resolve<Microsoft.Extensions.Configuration.IConfiguration>();
        var assetsProvider = AzureStorageProvider.Create();
        assetsProvider.CustomRootPath = configuration["CMSAzureRootContainer"];
        MapPathsToNewAzureStorageProviderForAllSites(assetsProvider.CustomRootPath, new[]
         {
                "~/<siteCodeName>/media/",
                "~/<siteCodeName>/files/",
                "~/<siteCodeName>/metafiles/",
                "~/<siteCodeName>/BizFormFiles/",
            });

        assetsProvider.PublicExternalFolderObject = false;
        StorageHelper.MapStoragePath("~/assets/contentitems", assetsProvider);
    }
    public static void MapPathsToNewAzureStorageProviderForAllSites(string customRootPath, string[] pathsToMap)
    {
        var channelContext = Service.Resolve<IWebsiteChannelContext>();
        var channel = channelContext.WebsiteChannelName;

        foreach (var pathToMap in pathsToMap)
        {
            MapPathToNewAzureStorageProvider(customRootPath, pathToMap.Replace("<siteCodeName>", channel));
        }

    }

    private static void MapPathToNewAzureStorageProvider(string customRootPath, string pathToMap)
    {
        bool sharedstorage = false;
        var storageProvider = new StorageProvider("KitAzure", "BDO.GWT.AzureStorageProvider", sharedstorage)
        {
            CustomRootPath = customRootPath,
            PublicExternalFolderObject = true,
        };

        StorageHelper.MapStoragePath(pathToMap, storageProvider);
    }
}


