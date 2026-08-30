using BDO;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using Microsoft.AspNetCore.StaticFiles;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Service for retrieving media files from the Content Hub.
/// </summary>
public class MediaFileCrudService : IMediaFileCrudService
{
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private readonly IContentQueryExecutor _contentQueryExecutor;

    public MediaFileCrudService(IContentQueryExecutor contentQueryExecutor)
    {
        _contentQueryExecutor = contentQueryExecutor;
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
}

