using BDO.GWT.GLOBAL.API.Data;
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
    private const string DevelopmentLiveSiteHost = "localhost";
    private const int DevelopmentLiveSitePort = 12393;

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

    private Uri? BuildAssetUri(string assetUrl)
    {
        if (Uri.TryCreate(assetUrl.Trim(), UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        var normalizedPath = NormalizePath(assetUrl);

        if (_hostEnvironment.IsDevelopment())
        {
            return new UriBuilder(Uri.UriSchemeHttp, DevelopmentLiveSiteHost, DevelopmentLiveSitePort, normalizedPath).Uri;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return null;
        }

        return new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? -1, normalizedPath).Uri;
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

    private static readonly SearchValues<char> InvalidFileNameChars =
        SearchValues.Create(System.IO.Path.GetInvalidFileNameChars());

    private static string GetSafeFileName(string? fileName)
    {
        var value = fileName ?? string.Empty;
        var separatorIndex = value.LastIndexOfAny(['/', '\\']);
        return separatorIndex >= 0 ? value[(separatorIndex + 1)..] : value;
    }

    private static string GetSafeExtension(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        if (extension.AsSpan().IndexOfAny(UnsafeExtensionChars) >= 0)
        {
            return string.Empty;
        }

        return extension;
    }


    public async Task<MediaFileOutput> CreateMediaFileAsync(string workspaceName, MediaFileInput input, CancellationToken cancellationToken)
    {
        // Sanitize user-supplied filename: take only the base file name to prevent path traversal
        var fileName = GetSafeFileName(input.File.FileName);
        var displayName = input.DisplayName ?? System.IO.Path.GetFileNameWithoutExtension(fileName);

        var languageName = await GetDefaultLanguageNameAsync(cancellationToken);
        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        var newItemId = await WithUploadedAssetMetadataAsync(input.File, async assetMetadata =>
        {
            var itemData = CreateContentItemData(assetMetadata);
            var createParameters = new CreateContentItemParameters(
                ContentAttachment.CONTENT_TYPE_NAME,
                null,
                displayName,
                languageName,
                workspaceName);

            return await manager.Create(createParameters, itemData, cancellationToken);
        }, cancellationToken);

        await manager.TryPublish(newItemId, languageName, cancellationToken);

        var created = await GetMediaFileByIdAsync(newItemId, cancellationToken);
        return created!;
    }


    public async Task<MediaFileOutput?> UpdateMediaFileAsync(string workspaceName, Guid attachmentGuid, MediaFileInput input, CancellationToken cancellationToken)
    {
        var target = await GetMediaFileTargetAsync(attachmentGuid, cancellationToken);
        if (target is null)
        {
            return null;
        }

        var languageName = await GetDefaultLanguageNameAsync(cancellationToken);
        var user = await _userInfoProvider.GetAsync("administrator", cancellationToken);
        var manager = _contentItemManagerFactory.Create(user.UserID);

        var itemData = await WithUploadedAssetMetadataAsync(input.File, assetMetadata =>
            Task.FromResult(CreateContentItemData(assetMetadata)), cancellationToken);

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

    private static string CreateSafeTempUploadPath()
    {
        var tempDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "App_Data", "Uploads", "Temp");
        System.IO.Directory.CreateDirectory(tempDirectory);

        var rootPath = System.IO.Path.GetFullPath(tempDirectory);
        var randomFileName = System.IO.Path.GetRandomFileName();
        if (randomFileName.AsSpan().IndexOfAny(InvalidFileNameChars) >= 0)
        {
            throw new InvalidOperationException("Invalid temporary file name.");
        }

        var tempFileName = System.IO.Path.ChangeExtension(randomFileName, ".tmp");
        var candidatePath = System.IO.Path.Combine(rootPath, tempFileName);
        var fullPath = System.IO.Path.GetFullPath(candidatePath);

        if (!fullPath.StartsWith(rootPath + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid temporary file path.");
        }

        return fullPath;
    }

    private static async Task<T> WithUploadedAssetMetadataAsync<T>(
        IFormFile file,
        Func<ContentItemAssetMetadataWithSource, Task<T>> action,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        // Sanitize user-supplied filename: take only the base file name to prevent path traversal
        var fileName = GetSafeFileName(file.FileName);
        var extension = GetSafeExtension(fileName);
        // Derive display name without extension using string trimming — avoids Path API on user data
        var nameWithoutExtension = extension.Length > 0 && fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? fileName[..^extension.Length]
            : fileName;

        var tempPath = CreateSafeTempUploadPath();

        try
        {
            await using (var fs = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(fs, cancellationToken);
            }

            var assetSource = new ContentItemAssetFileSource(tempPath, canMove: true);
            var assetMetadata = new ContentItemAssetMetadataWithSource(
                assetSource,
                nameWithoutExtension,
                extension,
                file.Length,
                DateTime.UtcNow);

            return await action(assetMetadata);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
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

