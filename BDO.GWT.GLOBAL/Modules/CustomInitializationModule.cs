using CMS;
using CMS.DataEngine;
using CMS.IO;

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
       var assetsProvider = AzureStorageProvider.Create();
       assetsProvider.CustomRootPath = "cmslibrary";
       assetsProvider.PublicExternalFolderObject = false;
       StorageHelper.MapStoragePath("~/assets/contentitems", assetsProvider);
    }
}