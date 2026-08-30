using BDO;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Service for retrieving media files from the Content Hub.
/// </summary>
public class MediaFileCrudService : IMediaFileCrudService
{
    private const string DevelopmentLiveSiteBaseUrl = "http://localhost:12393";

    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private const string DefaultLanguageName = "en";

    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IContentItemManagerFactory _contentItemManagerFactory;
    private readonly IUserInfoProvider _userInfoProvider;

    public MediaFileCrudService(
        IContentQueryExecutor contentQueryExecutor,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment,
        IContentItemManagerFactory contentItemManagerFactory,
        IUserInfoProvider userInfoProvider)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _hostEnvironment = hostEnvironment;
        _contentItemManagerFactory = contentItemManagerFactory;
        _userInfoProvider = userInfoProvider;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MediaFileOutput>> GetMediaFilesAsync(string workspaceName, CancellationToken cancellationToken)
    {
        var results = new List<MediaFileOutput>();

        // Collect from BDO.ContentAttachment (primary content type for images)
        var contentAttachments = await GetAllMediaFilesFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME,
            item => item.ContentAttachmentAsset,
            cancellationToken);
        results.AddRange(contentAttachments);

        // Collect from Legacy.MediaFile
        var legacyMediaFiles = await GetAllMediaFilesFromContentTypeAsync<MediaFile>(
            MediaFile.CONTENT_TYPE_NAME,
            item => item.LegacyMediaFileAsset,
            cancellationToken);
        results.AddRange(legacyMediaFiles);

        // Collect from MigratedMediaFile
        var migratedMediaFiles = await GetAllMediaFilesFromContentTypeAsync<MigratedMediaFile>(
            MigratedMediaFile.CONTENT_TYPE_NAME,
            item => item.MediaFile,
            cancellationToken);
        results.AddRange(migratedMediaFiles);

        return results;
    }

    /// <inheritdoc/>
    public async Task<MediaFileOutput?> GetMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        // Try to get from BDO.ContentAttachment content type first (primary content type for images)
        var contentAttachmentResult = await GetMediaFileFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME,
            attachmentGuid,
            item => item.ContentAttachmentAsset,
            cancellationToken);

        if (contentAttachmentResult != null)
        {
            return contentAttachmentResult;
        }

        // Try Legacy.MediaFile content type
        var mediaFileResult = await GetMediaFileFromContentTypeAsync<MediaFile>(
            MediaFile.CONTENT_TYPE_NAME,
            attachmentGuid,
            item => item.LegacyMediaFileAsset,
            cancellationToken);

        if (mediaFileResult != null)
        {
            return mediaFileResult;
        }

        // Try MigratedMediaFile content type
        var migratedResult = await GetMediaFileFromContentTypeAsync<MigratedMediaFile>(
            MigratedMediaFile.CONTENT_TYPE_NAME,
            attachmentGuid,
            item => item.MediaFile,
            cancellationToken);

        return migratedResult;
    }

    /// <inheritdoc/>
    public async Task<FileStreamData?> DownloadMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        var asset = await GetAssetByGuidAsync(workspaceName, attachmentGuid, cancellationToken);

        if (asset?.Url is null)
        {
            return null;
        }

        var assetUri = BuildAssetUri(asset.Url);
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
            ?? GetMimeType(asset.Metadata?.Extension ?? string.Empty);
        var fileName = asset.Metadata?.Name ?? "attachment";

        return new FileStreamData
        {
            Stream = new System.IO.MemoryStream(data),
            ContentType = contentType,
            FileName = fileName
        };
    }

    private async Task<ContentItemAsset?> GetAssetByGuidAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        var asset = await GetAssetFromContentTypeAsync<ContentAttachment>(
            ContentAttachment.CONTENT_TYPE_NAME, attachmentGuid,
            item => item.ContentAttachmentAsset, cancellationToken);

        if (asset is not null) return asset;

        asset = await GetAssetFromContentTypeAsync<MediaFile>(
            MediaFile.CONTENT_TYPE_NAME, attachmentGuid,
            item => item.LegacyMediaFileAsset, cancellationToken);

        if (asset is not null) return asset;

        return await GetAssetFromContentTypeAsync<MigratedMediaFile>(
            MigratedMediaFile.CONTENT_TYPE_NAME, attachmentGuid,
            item => item.MediaFile, cancellationToken);
    }

    private async Task<ContentItemAsset?> GetAssetFromContentTypeAsync<T>(
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
        return item is null ? null : assetSelector(item);
    }

    private Uri? BuildAssetUri(string assetUrl)
    {
        if (Uri.TryCreate(assetUrl.Trim(), UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        var normalizedPath = NormalizePath(assetUrl);

        if (_hostEnvironment.IsDevelopment())
        {
            return new Uri(new Uri(DevelopmentLiveSiteBaseUrl), normalizedPath);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        return request is null
            ? null
            : new Uri($"{request.Scheme}://{request.Host}{normalizedPath}");
    }

    private static string NormalizePath(string assetUrl)
    {
        var normalizedPath = assetUrl.Trim().TrimStart('~');
        return normalizedPath.StartsWith('/') ? normalizedPath : $"/{normalizedPath}";
    }

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

            var assetUrl = asset.Url ?? string.Empty;
            var fileName = GetFileNameFromUrl(assetUrl);
            var extension = System.IO.Path.GetExtension(fileName);

            if (!_contentTypeProvider.TryGetContentType(fileName, out var mimeType))
            {
                mimeType = GetMimeTypeFromExtension(extension);
            }

            outputs.Add(new MediaFileOutput
            {
                Id = item.SystemFields.ContentItemID,
                Path = fileName,
                MimeType = mimeType ?? "application/octet-stream",
                Extension = extension,
                Guid = item.SystemFields.ContentItemGUID,
                Link = $"~/getmedia/{item.SystemFields.ContentItemGUID}/{fileName}"
            });
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

        // Extract file information from the asset URL
        var assetUrl = asset.Url ?? string.Empty;
        var fileName = GetFileNameFromUrl(assetUrl);
        var extension = System.IO.Path.GetExtension(fileName);

        // Get MIME type from extension
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

    /// <inheritdoc/>
    public async Task<MediaFileOutput> CreateMediaFileAsync(string workspaceName, MediaFileInput input, CancellationToken cancellationToken)
    {
        var fileName = input.File.FileName;
        var extension = System.IO.Path.GetExtension(fileName);
        var displayName = input.DisplayName ?? System.IO.Path.GetFileNameWithoutExtension(fileName);

        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + extension);
        try
        {
            await using (var fs = System.IO.File.Create(tempPath))
            {
                await input.File.CopyToAsync(fs, cancellationToken);
            }

            var assetSource = new ContentItemAssetFileSource(tempPath, canMove: true);
            var assetMetadata = new ContentItemAssetMetadataWithSource(
                assetSource,
                System.IO.Path.GetFileNameWithoutExtension(fileName),
                extension,
                input.File.Length,
                DateTime.UtcNow);

            var itemData = new ContentItemData(new Dictionary<string, object>
            {
                [nameof(ContentAttachment.ContentAttachmentAsset)] = assetMetadata
            });

            var createParameters = new CreateContentItemParameters(
                ContentAttachment.CONTENT_TYPE_NAME,
                null,
                displayName,
                DefaultLanguageName,
                workspaceName);

            var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
            var manager = _contentItemManagerFactory.Create(user.UserID);

            var newItemId = await manager.Create(createParameters, itemData, cancellationToken);
            await manager.TryPublish(newItemId, DefaultLanguageName, cancellationToken);

            var created = await GetMediaFileByIdAsync(newItemId, cancellationToken);
            return created!;
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<MediaFileOutput?> UpdateMediaFileAsync(string workspaceName, Guid attachmentGuid, MediaFileInput input, CancellationToken cancellationToken)
    {
        var existing = await GetMediaFileAsync(workspaceName, attachmentGuid, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var fileName = input.File.FileName;
        var extension = System.IO.Path.GetExtension(fileName);

        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + extension);
        try
        {
            await using (var fs = System.IO.File.Create(tempPath))
            {
                await input.File.CopyToAsync(fs, cancellationToken);
            }

            var assetSource = new ContentItemAssetFileSource(tempPath, canMove: true);
            var assetMetadata = new ContentItemAssetMetadataWithSource(
                assetSource,
                System.IO.Path.GetFileNameWithoutExtension(fileName),
                extension,
                input.File.Length,
                DateTime.UtcNow);

            var itemData = new ContentItemData(new Dictionary<string, object>
            {
                [nameof(ContentAttachment.ContentAttachmentAsset)] = assetMetadata
            });

            var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
            var manager = _contentItemManagerFactory.Create(user.UserID);

            await manager.TryCreateDraft(existing.Id, DefaultLanguageName, cancellationToken);
            await manager.TryUpdateDraft(existing.Id, DefaultLanguageName, itemData, cancellationToken);
            await manager.TryPublish(existing.Id, DefaultLanguageName, cancellationToken);

            return await GetMediaFileByIdAsync(existing.Id, cancellationToken);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken)
    {
        var existing = await GetMediaFileAsync(workspaceName, attachmentGuid, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        await manager.Delete(existing.Id, DefaultLanguageName, cancellationToken);
        return true;
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
}

