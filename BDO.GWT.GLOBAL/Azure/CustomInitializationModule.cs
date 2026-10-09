using BDO;
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using Kentico.Xperience.AzureStorage;
using Microsoft.Extensions.Configuration;
using System;
using CmsService = CMS.Core.Service;

[assembly: RegisterModule(typeof(CustomInitializationModule))]

namespace BDO;

public class CustomInitializationModule : Module
{
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

        // Storage path mapping only depends on IConfiguration (no DB access required),
        // so it can run directly in OnInit. This avoids the obsolete
        // ApplicationEvents.PostStart hook.
        InitializeAzureStorageMapping();
    }

    private static void InitializeAzureStorageMapping()
    {
        try
        {
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

            // Map global paths only
            foreach (var path in GlobalStoragePaths)
            {
                MapPathToAzureStorageProvider(customRootPath, path, isPublicContainer);
            }

            CmsService.Resolve<IEventLogService>().LogInformation(
                "CustomInitializationModule",
                "INIT",
                "Azure Storage mapping completed successfully."
            );
        }
        catch (Exception ex)
        {
            CmsService.Resolve<IEventLogService>().LogException(
                "CustomInitializationModule",
                "INIT",
                ex,
                "Failed to initialize Azure Storage mapping."
            );
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