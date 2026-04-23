using BDO;
using CMS;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.Websites.Routing;
using Kentico.Xperience.AzureStorage;
using Microsoft.Extensions.Configuration;
using System.Linq;

[assembly: RegisterModule(typeof(CustomInitializationModule))]
namespace BDO;

public class CustomInitializationModule : Module
{
    private static readonly string[] ChannelStoragePathTemplates =
    [
        "~/<siteCodeName>/media/",
        "~/<siteCodeName>/files/",
        "~/<siteCodeName>/metafiles/",
        "~/<siteCodeName>/BizFormFiles/",
    ];

    public CustomInitializationModule()
        : base("CustomInit")
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        var configuration = Service.Resolve<IConfiguration>();

        var cdnEndpoint = configuration["AzureStorageProviderSettings:CMSAzureCDNEndpoint"];
        var legacyAssembly = configuration["AzureStorageProviderSettings:CMSStorageProviderAssembly"];
        if (!string.IsNullOrEmpty(legacyAssembly) || string.IsNullOrWhiteSpace(cdnEndpoint))
            return;

        var customRootPath = configuration["AzureStorageProviderSettings:CMSAzureRootContainer"];
        MapPathsForAllChannels(customRootPath, ChannelStoragePathTemplates);
        MapPathToAzureStorageProvider(customRootPath, "~/assets/contentitems");
    }

    private static void MapPathsForAllChannels(string customRootPath, string[] pathsToMap)
    {
        var channels = ChannelInfo.Provider.Get()
            .WhereEquals("ChannelType", "Website").ToList();

        if (channels.Count == 0)
        {
            Service.Resolve<IEventLogService>().LogWarning(
                "AzureInitializationModule",
                "INIT",
                "No website channels found during Azure Storage path mapping."
            );
            return;
        }

        foreach (var channel in channels)
        {
            foreach (var path in pathsToMap)
            {
                MapPathToAzureStorageProvider(
                    customRootPath,
                    path.Replace("<siteCodeName>", channel.ChannelName.ToLower())
                );
            }
        }
    }

    private static void MapPathToAzureStorageProvider(string customRootPath, string pathToMap)
    {
        var storageProvider = AzureStorageProvider.Create();
        storageProvider.CustomRootPath = customRootPath;
        storageProvider.PublicExternalFolderObject = false;

        StorageHelper.MapStoragePath(pathToMap, storageProvider);
    }
}