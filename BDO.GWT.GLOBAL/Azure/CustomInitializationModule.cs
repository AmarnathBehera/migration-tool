using BDO;
using CMS;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using Kentico.Xperience.AzureStorage;
using Microsoft.Extensions.Configuration;
using System.Linq;
using CmsService = CMS.Core.Service;

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

    private static readonly string[] GlobalStoragePaths =
    [
        "~/assets/contentitems/",
        "~/assets/temp/",
        "~/temp/",
    ];

    public CustomInitializationModule() : base("CustomInit") { }

    protected override void OnInit()
    {
        base.OnInit();

        var configuration = CmsService.Resolve<IConfiguration>();

        var legacyAssembly = configuration["AzureStorageProviderSettings:CMSStorageProviderAssembly"];
        if (!string.IsNullOrEmpty(legacyAssembly))
            return;

        var cdnEndpoint = configuration["AzureStorageProviderSettings:CMSAzureCDNEndpoint"];
        if (string.IsNullOrWhiteSpace(cdnEndpoint))
        {
            CmsService.Resolve<IEventLogService>().LogWarning(
                "CustomInitializationModule",
                "INIT",
                "Azure CDN endpoint is not configured. Skipping Azure Storage mapping."
            );
            return;
        }

        var customRootPath = configuration["AzureStorageProviderSettings:CMSAzureRootContainer"];
        var isPublicContainer = bool.TryParse(
            configuration["AzureStorageProviderSettings:CMSAzurePublicContainer"],
            out var parsed) && parsed;

        MapPathsForAllChannels(customRootPath, ChannelStoragePathTemplates, isPublicContainer);

        foreach (var path in GlobalStoragePaths)
        {
            MapPathToAzureStorageProvider(customRootPath, path, isPublicContainer);
        }
    }

    private static void MapPathsForAllChannels(
        string customRootPath,
        string[] pathsToMap,
        bool isPublicContainer)
    {
        var channels = ChannelInfo.Provider.Get()
            .WhereEquals("ChannelType", "Website")
            .ToList();

        if (channels.Count == 0)
        {
            CmsService.Resolve<IEventLogService>().LogWarning(
                "CustomInitializationModule",
                "INIT",
                "No website channels found during Azure Storage path mapping."
            );
            return;
        }

        foreach (var channel in channels)
        {
            foreach (var path in pathsToMap)
            {
                var resolvedPath = path.Replace(
                    "<siteCodeName>",
                    channel.ChannelName.ToLower()
                );
                MapPathToAzureStorageProvider(customRootPath, resolvedPath, isPublicContainer);
            }
        }
    }

    private static void MapPathToAzureStorageProvider(
        string customRootPath,
        string pathToMap,
        bool isPublicContainer)
    {
        var storageProvider = AzureStorageProvider.Create();
        storageProvider.CustomRootPath = customRootPath;
        storageProvider.PublicExternalFolderObject = isPublicContainer;
        StorageHelper.MapStoragePath(pathToMap, storageProvider);
    }
}