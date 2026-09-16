using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Microsoft.AspNetCore.StaticFiles;
using System.Buffers;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Service for retrieving media files from the Content Hub.
/// </summary>
public class MediaFileCrudService : IMediaFileCrudService
{
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private const string FallbackLanguageName = "en-US";

    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IContentItemManagerFactory _contentItemManagerFactory;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;

    private sealed record MediaFileTarget(int ContentItemId, ContentItemAsset Asset);

    public MediaFileCrudService(
        IContentQueryExecutor contentQueryExecutor,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment,
        IContentItemManagerFactory contentItemManagerFactory,
        IUserInfoProvider userInfoProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _hostEnvironment = hostEnvironment;
        _contentItemManagerFactory = contentItemManagerFactory;
        _userInfoProvider = userInfoProvider;
        _contentLanguageInfoProvider = contentLanguageInfoProvider;
    }

    public async Task<IEnumerable<MediaFileOutput>> GetMediaFilesAsync(string workspaceName, CancellationToken cancellationToken)
    {
        return await GetAllMediaFilesFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME,
            item => item.ContentAttachmentAsset,
            cancellationToken);
    }


    public async Task<MediaFileOutput?> GetMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        return await GetMediaFileFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME,
            attachmentGuid,
            item => item.ContentAttachmentAsset,
            cancellationToken);
    }

    public async Task<FileStreamData?> DownloadMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        var target = await GetMediaFileTargetAsync(attachmentGuid, cancellationToken);

        if (target?.Asset.Url is null)
        {
            return null;
        }

        var assetUri = BuildAssetUri(target.Asset.Url);
        if (assetUri is null)
        {
            return null;
        }

        var response = await _httpClientFactory.CreateClient().GetAsync(assetUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType
            ?? GetMimeType(target.Asset.Metadata?.Extension ?? string.Empty);
        var fileName = target.Asset.Metadata?.Name ?? "attachment";

        return new FileStreamData
        {
            Stream = new System.IO.MemoryStream(data),
            ContentType = contentType,
            FileName = fileName
        };
    }

    private async Task<MediaFileTarget?> GetMediaFileTargetAsync(Guid attachmentGuid, CancellationToken cancellationToken)
    {
        return await GetMediaFileTargetFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME,
            attachmentGuid,
            item => item.ContentAttachmentAsset,
            cancellationToken);
    }

    private async Task<MediaFileTarget?> GetMediaFileTargetFromContentTypeAsync<T>(
        string contentTypeName,
        Guid attachmentGuid,
        Func<T, ContentItemAsset?> assetSelector,
        CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(
                contentTypeName,
                subQuery => subQuery
                    .Where(w => w.WhereEquals(nameof(ContentItemFields.ContentItemGUID), attachmentGuid))
                    .TopN(1));

        var results = await _contentQueryExecutor.GetMappedResult<T>(query, cancellationToken: cancellationToken);
        var item = results.FirstOrDefault();

        if (item is null)
        {
            return null;
        }

        var asset = assetSelector(item);
        return asset is null
            ? null
            : new MediaFileTarget(item.SystemFields.ContentItemID, asset);
    }

    private Uri? BuildAssetUri(string assetUrl) =>
        AssetUrlHelper.BuildAssetUri(assetUrl, _httpContextAccessor, _hostEnvironment);
    private static string GetMimeType(string extension)
    {
        if (_contentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
        {
            return contentType;
        }

        return MediaTypeNames.Application.Octet;
    }

    private async Task<IEnumerable<MediaFileOutput>> GetAllMediaFilesFromContentTypeAsync<T>(
        string contentTypeName,
        Func<T, ContentItemAsset?> assetSelector,
        CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName);

        var results = await _contentQueryExecutor.GetMappedResult<T>(query, cancellationToken: cancellationToken);

        var outputs = new List<MediaFileOutput>();
        foreach (var item in results)
        {
            var asset = assetSelector(item);
            if (asset == null)
            {
                continue;
            }

            outputs.Add(MapToMediaFileOutput(item, asset));
        }

        return outputs;
    }

    private async Task<MediaFileOutput?> GetMediaFileFromContentTypeAsync<T>(
        string contentTypeName,
        Guid attachmentGuid,
        Func<T, ContentItemAsset?> assetSelector,
        CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(
                contentTypeName,
                subQuery => subQuery
                    .Where(w => w.WhereEquals(nameof(ContentItemFields.ContentItemGUID), attachmentGuid))
                    .TopN(1));

        var results = await _contentQueryExecutor.GetMappedResult<T>(query, cancellationToken: cancellationToken);

        var item = results.FirstOrDefault();
        if (item == null)
        {
            return null;
        }

        var asset = assetSelector(item);
        if (asset == null)
        {
            return null;
        }

        return MapToMediaFileOutput(item, asset);
    }

    private static string GetFileNameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        // Remove query string if present
        var queryIndex = url.IndexOf('?');
        if (queryIndex > 0)
        {
            url = url[..queryIndex];
        }

        return System.IO.Path.GetFileName(url);
    }

    private static string GetMimeTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }

    private static MediaFileOutput MapToMediaFileOutput(IContentItemFieldsSource item, ContentItemAsset asset)
    {
        var assetUrl = asset.Url ?? string.Empty;
        var fileName = GetFileNameFromUrl(assetUrl);
        var extension = System.IO.Path.GetExtension(fileName);

        if (!_contentTypeProvider.TryGetContentType(fileName, out var mimeType))
        {
            mimeType = GetMimeTypeFromExtension(extension);
        }

        return new MediaFileOutput
        {
            Id = item.SystemFields.ContentItemID,
            Path = fileName,
            MimeType = mimeType ?? "application/octet-stream",
            Extension = extension,
            Guid = item.SystemFields.ContentItemGUID,
            Link = $"~/getmedia/{item.SystemFields.ContentItemGUID}/{fileName}"
        };
    }

    // Explicit cross-platform set: null byte, forward/back slashes, and common injection chars
    private static readonly SearchValues<char> UnsafeExtensionChars =
        SearchValues.Create(['\0', '/', '\\', '|', '<', '>', ':', '"', '?', '*']);

    public async Task<MediaFileOutput> CreateMediaFileAsync(string workspaceName, IFormFile file, CancellationToken cancellationToken)
    {
        var languageName = await GetDefaultLanguageNameAsync(cancellationToken);
        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        int createdContentItemId = await WithUploadedAssetMetadataAsync(
            file,
            async assetMetadata =>
            {
                var createItemData = CreateContentItemData(assetMetadata);

                var attachmentParameters = new CreateContentItemParameters(
                    ContentAttachment.CONTENT_TYPE_NAME,
                    assetMetadata.Name,
                    assetMetadata.Name,
                    languageName,
                    workspaceName)
                {
                    VersionStatus = VersionStatus.Published
                };

                return await manager.Create(attachmentParameters, createItemData, cancellationToken);
            },
            cancellationToken);

        await manager.TryPublish(createdContentItemId, languageName, cancellationToken);

        var created = await GetMediaFileByIdAsync(createdContentItemId, cancellationToken);
        if (created is null)
        {
            throw new InvalidOperationException("Media file was created but could not be retrieved.");
        }

        return created;
    }


    public async Task<MediaFileOutput?> UpdateMediaFileAsync(string workspaceName, Guid attachmentGuid, IFormFile file, CancellationToken cancellationToken)
    {
        var target = await GetMediaFileTargetAsync(attachmentGuid, cancellationToken);
        if (target is null)
        {
            return null;
        }

        var languageName = await GetDefaultLanguageNameAsync(cancellationToken);
        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        var itemData = await WithUploadedAssetMetadataAsync(
            file,
            assetMetadata => Task.FromResult(CreateContentItemData(assetMetadata)),
            cancellationToken);

        await manager.TryCreateDraft(target.ContentItemId, languageName, cancellationToken);
        await manager.TryUpdateDraft(target.ContentItemId, languageName, itemData, cancellationToken);
        await manager.TryPublish(target.ContentItemId, languageName, cancellationToken);

        return await GetMediaFileAsync(workspaceName, attachmentGuid, cancellationToken);
    }


    public async Task<bool> DeleteMediaFileAsync(Guid attachmentGuid, CancellationToken cancellationToken)
    {
        var target = await GetMediaFileTargetAsync(attachmentGuid, cancellationToken);
        if (target is null)
        {
            return false;
        }

        var languageName = await GetDefaultLanguageNameAsync(cancellationToken);

        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        await manager.Delete(new DeleteContentItemParameters(target.ContentItemId, languageName), cancellationToken);
        return true;
    }

    private static ContentItemData CreateContentItemData(ContentItemAssetMetadataWithSource assetMetadata)
    {
        return new ContentItemData(new Dictionary<string, object>
        {
            [nameof(ContentAttachment.ContentAttachmentAsset)] = assetMetadata
        });
    }

    private static string GetSafeFileName(string? fileName)
    {
        var value = fileName ?? string.Empty;
        var separatorIndex = value.LastIndexOfAny(['/', '\\']);
        return separatorIndex >= 0 ? value[(separatorIndex + 1)..] : value;
    }

    private static string GetSafeExtension(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.AsSpan().IndexOfAny(UnsafeExtensionChars) >= 0)
        {
            throw new InvalidOperationException("Invalid file extension.");
        }

        return extension;
    }

    private static async Task<T> WithUploadedAssetMetadataAsync<T>(
        IFormFile file,
        Func<ContentItemAssetMetadataWithSource, Task<T>> action,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        var fileName = GetSafeFileName(file.FileName);
        var extension = GetSafeExtension(fileName);

        await using var uploadStream = file.OpenReadStream();
        using var bufferedStream = new MemoryStream();
        await uploadStream.CopyToAsync(bufferedStream, cancellationToken);

        var bufferedContent = bufferedStream.ToArray();
        var assetSource = new ContentItemAssetStreamSource(_ =>
            Task.FromResult<Stream>(new MemoryStream(bufferedContent, writable: false)));
        var assetMetadata = new ContentItemAssetMetadataWithSource(
            assetSource,
            fileName,
            extension,
            file.Length,
            DateTime.UtcNow);

        return await action(assetMetadata);
    }

    private async Task<string> GetDefaultLanguageNameAsync(CancellationToken cancellationToken)
    {
        var language = await _contentLanguageInfoProvider.Get()
            .Columns(nameof(ContentLanguageInfo.ContentLanguageName))
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return language.FirstOrDefault()?.ContentLanguageName ?? FallbackLanguageName;
    }

    private async Task<MediaFileOutput?> GetMediaFileByIdAsync(int contentItemId, CancellationToken cancellationToken)
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(
                ContentAttachment.CONTENT_TYPE_NAME,
                subQuery => subQuery
                    .Where(w => w.WhereEquals(nameof(ContentItemFields.ContentItemID), contentItemId))
                    .TopN(1));

        var results = await _contentQueryExecutor.GetMappedResult<ContentAttachment>(query, cancellationToken: cancellationToken);
        var item = results.FirstOrDefault();
        if (item is null)
        {
            return null;
        }

        var asset = item.ContentAttachmentAsset;
        if (asset is null)
        {
            return null;
        }

        return MapToMediaFileOutput(item, asset);
    }
}

