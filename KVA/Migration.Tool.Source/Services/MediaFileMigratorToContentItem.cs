using System.Collections.Concurrent;
using CMS.ContentEngine.Internal;
using CMS.Core;
using Kentico.Xperience.UMT.Model;
using Kentico.Xperience.UMT.Services;
using Microsoft.Extensions.Logging;
using Migration.Tool.Common;
using Migration.Tool.Common.Abstractions;
using Migration.Tool.Common.Services;
using Migration.Tool.Source.Handlers;
using Migration.Tool.Source.Mappers.ContentItemMapperDirectives;
using Migration.Tool.Source.Model;

namespace Migration.Tool.Source.Services;

public class MediaFileMigratorToContentItem(
    ILogger<MigrateMediaLibrariesCommandHandler> logger,
    ModelFacade modelFacade,
    IAssetFacade assetFacade,
    IImporter importer,
    UserService userService,
    WorkspaceService workspaceService,
    ContentFolderService contentFolderService,
    IEnumerable<ContentItemDirectorBase> directors
    ) : IMediaFileMigrator
{
    public async Task<CommandResult> Handle(MigrateMediaLibrariesCommand request, CancellationToken cancellationToken)
    {
        await MigrateToAssets();
        return new GenericCommandResult();
    }

    private async Task MigrateToAssets()
    {
        var migratedSiteIds = modelFacade.GetMigratedSites().Select(site => site.SiteID).ToHashSet();
        var ksMediaFiles = modelFacade.SelectAll<IMediaFile>(" ORDER BY FileLibraryID")
            .Where(mediaFile => migratedSiteIds.Contains(mediaFile.FileSiteID));
        var ksMediaLibraries = new ConcurrentDictionary<int, IMediaLibrary?>();
        var ksSites = new ConcurrentDictionary<int, ICmsSite?>();
        var contentLanguageRetriever = Service.Resolve<IContentLanguageRetriever>();
        var defaultContentLanguage = await contentLanguageRetriever.GetDefaultContentLanguage();

        await assetFacade.PreparePrerequisites();

        var importedCount = 0;
        var missingFileCount = 0;
        var failedCount = 0;
        foreach (var ksMediaFile in ksMediaFiles)
        {
            if (ksSites.GetOrAdd(ksMediaFile.FileSiteID, siteId => modelFacade.SelectById<ICmsSite>(siteId)) is not { } ksSite)
            {
                logger.LogError("Media file '{File}' site not found", ksMediaFile);
                failedCount++;
                continue;
            }
            if (ksMediaLibraries.GetOrAdd(ksMediaFile.FileLibraryID, libraryId => modelFacade.SelectById<IMediaLibrary>(libraryId)) is not { } ksMediaLibrary)
            {
                logger.LogError("Media file '{File}' library not found", ksMediaFile);
                failedCount++;
                continue;
            }

            var directive = GetDirective(new(ksSite, ksMediaLibrary, ksMediaFile));

            var workspaceGuid = workspaceService.EnsureWorkspace(directive.WorkspaceOptions);
            ContentItemSimplifiedModel umtContentItem;
            try
            {
                umtContentItem = await assetFacade.FromMediaFile(ksMediaFile, ksMediaLibrary, ksSite, [defaultContentLanguage.ContentLanguageName], workspaceGuid, directive.ContentFolderOptions);
            }
            catch (FileNotFoundException exception)
            {
                missingFileCount++;
                logger.LogWarning("Skipping missing source media file '{FileGuid}' in site '{SiteName}', library '{LibraryName}': {FilePath}",
                    ksMediaFile.FileGUID, ksSite.SiteName, ksMediaLibrary.LibraryName, exception.FileName);
                continue;
            }

            umtContentItem.ContentItemWorkspaceGUID = workspaceGuid;

            foreach (var item in umtContentItem.LanguageData)
            {
                item.UserGuid = (item.UserGuid.HasValue && userService.UserExists(item.UserGuid.Value))
                    ? item.UserGuid
                    : userService.DefaultAdminUser?.UserGUID;
            }

            switch (await importer.ImportAsync(umtContentItem))
            {
                case { Success: true }:
                {
                    importedCount++;
                    logger.LogInformation("Media file '{File}' imported", ksMediaFile.FileGUID);
                    break;
                }
                case { Success: false, Exception: { } exception }:
                {
                    failedCount++;
                    logger.LogError("Media file '{File}' not migrated: {Error}", ksMediaFile.FileGUID, exception);
                    break;
                }
                case { Success: false, ModelValidationResults: { } validation }:
                {
                    failedCount++;
                    foreach (var validationResult in validation)
                    {
                        logger.LogError("Media file '{File}' not migrated: {Members}: {Error}", ksMediaFile.FileGUID, string.Join(",", validationResult.MemberNames), validationResult.ErrorMessage);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        logger.LogInformation("Media migration summary: {ImportedCount} imported, {MissingFileCount} skipped because source files are missing, {FailedCount} failed",
            importedCount, missingFileCount, failedCount);
    }

    private ContentItemDirectiveBase GetDirective(MediaContentItemSource contentItemSource)
    {
        var directiveFacade = new ContentItemActionProvider();
        foreach (var director in directors)
        {
            director.Direct(contentItemSource, directiveFacade);
            if (directiveFacade.Directive is DropDirective)
            {
                break;
            }
        }
        return directiveFacade.Directive!;
    }
}
