using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class MediaFileCrudServiceTests
{
    [Test]
    public void MediaFileCrudService_ImplementsIMediaFileCrudService()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));

        Assert.Multiple(() =>
        {
            Assert.That(service, Is.InstanceOf<IMediaFileCrudService>());
        });
    }

    [Test]
    public void IMediaFileCrudService_DefinesExpectedMethods()
    {
        var interfaceType = typeof(IMediaFileCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.GetMediaFilesAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.GetMediaFileAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.DownloadMediaFileAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.CreateMediaFileAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.UpdateMediaFileAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IMediaFileCrudService.DeleteMediaFileAsync)), Is.Not.Null);
        });
    }

    [Test]
    public void GetMediaFilesAsync_WhenDependenciesAreUnavailable_ThrowsInvalidOperationException()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.GetMediaFilesAsync("Demo", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }

    [Test]
    public void GetMediaFileAsync_WhenDependenciesAreUnavailable_ThrowsInvalidOperationException()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.GetMediaFileAsync("Demo", Guid.NewGuid(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }

    [Test]
    public void DownloadMediaFileAsync_WhenDependenciesAreUnavailable_ThrowsInvalidOperationException()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DownloadMediaFileAsync("Demo", Guid.NewGuid(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }

    [Test]
    public void CreateMediaFileAsync_WhenLanguageQueryFails_PropagatesException_AndCallsProviderGet()
    {
        var languageProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
        var marker = new InvalidOperationException("Get() called");
        languageProviderMock
            .Setup(x => x.Get())
            .Throws(marker);

        var service = CreateSut(
            httpContextAccessor: Mock.Of<IHttpContextAccessor>(),
            hostEnvironment: Mock.Of<IWebHostEnvironment>(),
            languageInfoProvider: languageProviderMock.Object);

        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "File", "new.jpg");

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateMediaFileAsync("Demo", file, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.SameAs(marker));
        });

        languageProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public void CreateMediaFileAsync_WhenDisplayNameIsNull_StillCallsLanguageProviderGet()
    {
        var languageProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
        var marker = new InvalidOperationException("Get() called");
        languageProviderMock
            .Setup(x => x.Get())
            .Throws(marker);

        var service = CreateSut(
            httpContextAccessor: Mock.Of<IHttpContextAccessor>(),
            hostEnvironment: Mock.Of<IWebHostEnvironment>(),
            languageInfoProvider: languageProviderMock.Object);

        var file = new FormFile(new MemoryStream([4, 5, 6]), 0, 3, "File", "fallback-name.jpg");

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateMediaFileAsync("Demo", file, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.SameAs(marker));
        });

        languageProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public void UpdateMediaFileAsync_WhenDependenciesAreUnavailable_ThrowsInvalidOperationException()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));
        var file = new FormFile(new MemoryStream([1, 2]), 0, 2, "File", "replace.jpg");

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateMediaFileAsync("Demo", Guid.NewGuid(), file, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }

    [Test]
    public void DeleteMediaFileAsync_WhenDependenciesAreUnavailable_ThrowsInvalidOperationException()
    {
        var service = (MediaFileCrudService)RuntimeHelpers.GetUninitializedObject(typeof(MediaFileCrudService));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteMediaFileAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }

    [Test]
    public void NormalizePath_WhenTildePrefixProvided_ReturnsNormalizedRootPath()
    {
        var result = (string)InvokePrivateStatic("NormalizePath", "~/getmedia/a.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("/getmedia/a.jpg"));
        });
    }

    [Test]
    public void NormalizePath_WhenMissingLeadingSlash_AddsSlash()
    {
        var result = (string)InvokePrivateStatic("NormalizePath", "getmedia/a.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("/getmedia/a.jpg"));
        });
    }

    [Test]
    public void GetFileNameFromUrl_WhenUrlContainsQuery_RemovesQueryAndReturnsFileName()
    {
        var result = (string)InvokePrivateStatic("GetFileNameFromUrl", "/path/to/file.webp?ver=1");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("file.webp"));
        });
    }

    [Test]
    public void GetFileNameFromUrl_WhenUrlIsEmpty_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetFileNameFromUrl", string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [TestCase(".png", "image/png")]
    [TestCase(".jpg", "image/jpeg")]
    [TestCase(".jpeg", "image/jpeg")]
    [TestCase(".webp", "image/webp")]
    [TestCase(".pdf", "application/pdf")]
    [TestCase(".gif", "image/gif")]
    [TestCase(".svg", "image/svg+xml")]
    [TestCase(".doc", "application/msword")]
    [TestCase(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [TestCase(".xls", "application/vnd.ms-excel")]
    [TestCase(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [TestCase(".mp4", "video/mp4")]
    [TestCase(".mp3", "audio/mpeg")]
    [TestCase(".unknown", "application/octet-stream")]
    public void GetMimeTypeFromExtension_ReturnsExpectedValue(string extension, string expected)
    {
        var result = (string)InvokePrivateStatic("GetMimeTypeFromExtension", extension);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
        });
    }

    [Test]
    public void GetSafeExtension_WhenValidExtension_ReturnsExtension()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "photo.JPG");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(".JPG"));
        });
    }

    [Test]
    public void GetSafeExtension_WhenNoExtension_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "photo");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void GetSafeExtension_WhenContainsInvalidCharacter_Throws()
    {
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivateStatic("GetSafeExtension", "photo.jp|g"));

        Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetSafeExtension_WhenContainsDirectorySeparator_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "photo.jpg/abc");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void GetSafeFileName_WhenPathContainsSeparators_ReturnsTerminalFileName()
    {
        var result = (string)InvokePrivateStatic("GetSafeFileName", "folder\\nested/image.png");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("image.png"));
        });
    }

    [Test]
    public void CreateSafeTempUploadPath_ReturnsPathInExpectedTempDirectory()
    {
        var result = (string)InvokePrivateStatic("CreateSafeTempUploadPath");
        var expectedRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "App_Data", "Uploads", "Temp"));
        var fullPath = Path.GetFullPath(result);

        Assert.Multiple(() =>
        {
            Assert.That(fullPath, Does.StartWith(expectedRoot + Path.DirectorySeparatorChar));
            Assert.That(Path.GetExtension(fullPath), Is.EqualTo(".tmp"));
        });
    }

    [Test]
    public void MapToMediaFileOutput_WhenMimeTypeIsUnknown_UsesFallbackMimeAndMapsFields()
    {
        var attachmentGuid = Guid.NewGuid();
        var itemMock = new Mock<IContentItemFieldsSource>();
        itemMock.SetupGet(x => x.SystemFields).Returns(new ContentItemFields
        {
            ContentItemID = 123,
            ContentItemGUID = attachmentGuid
        });

        var asset = CreateAsset("mystery.weird", ".weird", 128, "/assets/mystery.weird");

        var output = (MediaFileOutput)InvokePrivateStatic("MapToMediaFileOutput", itemMock.Object, asset);

        Assert.Multiple(() =>
        {
            Assert.That(output.Id, Is.EqualTo(123));
            Assert.That(output.Guid, Is.EqualTo(attachmentGuid));
            Assert.That(output.Path, Is.EqualTo("mystery.weird"));
            Assert.That(output.Extension, Is.EqualTo(".weird"));
            Assert.That(output.MimeType, Is.EqualTo("application/octet-stream"));
            Assert.That(output.Link, Is.EqualTo($"~/getmedia/{attachmentGuid}/mystery.weird"));
        });
    }

    [Test]
    public void BuildAssetUri_WhenAbsoluteUrl_ReturnsAbsoluteUri()
    {
        var service = CreateSut();

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "https://cdn.example.com/a.png");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("https://cdn.example.com/a.png"));
        });
    }

    [Test]
    public void BuildAssetUri_WhenDevelopmentAndRelativePath_UsesDevelopmentBaseUrl()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

        var service = CreateSut(hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "~/getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("http://localhost:12393/getmedia/img.jpg"));
        });
    }

    [Test]
    public void BuildAssetUri_WhenDevelopmentAndRelativePathHasQuery_PreservesQuery()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

        var service = CreateSut(hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "~/getmedia/img.jpg?language=en-gb");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("http://localhost:12393/getmedia/img.jpg?language=en-gb"));
        });
    }

    [Test]
    public void BuildAssetUri_WhenProductionAndNoHttpContext_ReturnsNull()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Production");

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = CreateSut(httpContextAccessor: httpContextAccessorMock.Object, hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "~/getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
        });
    }

    [Test]
    public void BuildAssetUri_WhenProductionAndHostIsMissing_ReturnsNull()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Production");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = default;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = CreateSut(httpContextAccessor: httpContextAccessorMock.Object, hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "~/getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
        });
    }

    [Test]
    public void BuildAssetUri_WhenProductionAndHttpContextPresent_UsesRequestSchemeAndHost()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Production");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = CreateSut(httpContextAccessor: httpContextAccessorMock.Object, hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("https://example.com/getmedia/img.jpg"));
        });
    }

    [Test]
    public void BuildAssetUri_WhenProductionAndHttpContextPresentAndPathHasQuery_PreservesQuery()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Production");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = CreateSut(httpContextAccessor: httpContextAccessorMock.Object, hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "getmedia/img.jpg?language=en-gb");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("https://example.com/getmedia/img.jpg?language=en-gb"));
        });
    }

    [Test]
    public void GetMimeType_WhenUnknownExtension_ReturnsApplicationOctetStream()
    {
        var result = (string)InvokePrivateStatic("GetMimeType", ".doesnotexist");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("application/octet-stream"));
        });
    }

    [Test]
    public void GetMimeType_WhenKnownExtension_ReturnsExpectedContentType()
    {
        var result = (string)InvokePrivateStatic("GetMimeType", ".png");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("image/png"));
        });
    }

    [Test]
    public void GetSafeFileName_WhenNull_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeFileName", new object?[] { null });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void GetSafeFileName_WhenOnlyBackslashes_ReturnsFileName()
    {
        var result = (string)InvokePrivateStatic("GetSafeFileName", "folder\\subfolder\\file.txt");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("file.txt"));
        });
    }

    [Test]
    public void GetSafeFileName_WhenOnlyForwardSlashes_ReturnsFileName()
    {
        var result = (string)InvokePrivateStatic("GetSafeFileName", "folder/subfolder/file.txt");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("file.txt"));
        });
    }

    [Test]
    public void GetSafeFileName_WhenNoSeparators_ReturnsOriginal()
    {
        var result = (string)InvokePrivateStatic("GetSafeFileName", "simple.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("simple.jpg"));
        });
    }

    [Test]
    public void GetSafeExtension_WhenNullFileName_ReturnsNull()
    {
        var result = (string?)InvokePrivateStatic("GetSafeExtension", new object?[] { null });

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetSafeExtension_WhenWhitespaceOnly_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "   ");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void GetSafeExtension_WhenContainsBackslash_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "photo.jp\\g");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetSafeExtension_WhenContainsNullChar_Throws()
    {
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivateStatic("GetSafeExtension", "photo.jp\0g"));

        Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetFileNameFromUrl_WhenUrlHasPathButNoQuery_ReturnsFileName()
    {
        var result = (string)InvokePrivateStatic("GetFileNameFromUrl", "/path/to/document.pdf");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("document.pdf"));
        });
    }

    [Test]
    public void GetFileNameFromUrl_WhenUrlIsNull_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetFileNameFromUrl", (string)null!);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void BuildAssetUri_WhenDevelopmentEnvironment_ReturnsLocalhostUri()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

        var service = CreateSut(hostEnvironment: hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "~/getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Host, Is.EqualTo("localhost"));
            Assert.That(result.Port, Is.EqualTo(12393));
            Assert.That(result.AbsolutePath, Is.EqualTo("/getmedia/img.jpg"));
        });
    }

    [Test]
    public void MapToMediaFileOutput_WhenKnownMimeType_ReturnsCorrectMimeType()
    {
        var attachmentGuid = Guid.NewGuid();
        var itemMock = new Mock<IContentItemFieldsSource>();
        itemMock.SetupGet(x => x.SystemFields).Returns(new ContentItemFields
        {
            ContentItemID = 456,
            ContentItemGUID = attachmentGuid
        });

        var asset = CreateAsset("photo.png", ".png", 256, "/assets/photo.png");

        var output = (MediaFileOutput)InvokePrivateStatic("MapToMediaFileOutput", itemMock.Object, asset);

        Assert.Multiple(() =>
        {
            Assert.That(output.Id, Is.EqualTo(456));
            Assert.That(output.Guid, Is.EqualTo(attachmentGuid));
            Assert.That(output.Path, Is.EqualTo("photo.png"));
            Assert.That(output.Extension, Is.EqualTo(".png"));
            Assert.That(output.MimeType, Is.EqualTo("image/png"));
            Assert.That(output.Link, Is.EqualTo($"~/getmedia/{attachmentGuid}/photo.png"));
        });
    }

    [Test]
    public void MapToMediaFileOutput_WhenUrlIsNull_HandlesGracefully()
    {
        var attachmentGuid = Guid.NewGuid();
        var itemMock = new Mock<IContentItemFieldsSource>();
        itemMock.SetupGet(x => x.SystemFields).Returns(new ContentItemFields
        {
            ContentItemID = 789,
            ContentItemGUID = attachmentGuid
        });

        var asset = new ContentItemAsset { Url = null };

        var output = (MediaFileOutput)InvokePrivateStatic("MapToMediaFileOutput", itemMock.Object, asset);

        Assert.Multiple(() =>
        {
            Assert.That(output.Id, Is.EqualTo(789));
            Assert.That(output.Path, Is.EqualTo(string.Empty));
            Assert.That(output.MimeType, Is.EqualTo("application/octet-stream"));
        });
    }

    [Test]
    public void WithUploadedAssetMetadataAsync_WhenFileIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await InvokePrivateStaticAsync<bool>(
                "WithUploadedAssetMetadataAsync",
                [typeof(bool)],
                null!,
                new Func<ContentItemAssetMetadataWithSource, Task<bool>>(_ => Task.FromResult(true)),
                CancellationToken.None));
    }

    private static ContentItemAsset CreateAsset(string name, string extension, long size, string url)
    {
        var asset = new ContentItemAsset { Url = url };
        var metadata = (ContentItemAssetMetadata)RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAssetMetadata));

        SetMetadataProperty(metadata, "Name", name);
        SetMetadataProperty(metadata, "Extension", extension);
        SetMetadataProperty(metadata, "Size", size);

        typeof(ContentItemAsset)
            .GetProperty("Metadata")!
            .SetValue(asset, metadata);

        return asset;
    }

    private static void SetMetadataProperty(object target, string propertyName, object? value)
    {
        var type = target.GetType();
        PropertyInfo? property = null;

        while (type != null && property == null)
        {
            property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            type = type.BaseType;
        }

        property!.SetValue(target, value);
    }

    private static object InvokePrivateStatic(string methodName, params object?[] args)
    {
        var method = typeof(MediaFileCrudService).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;
        return method.Invoke(null, args)!;
    }

    private static async Task<T> InvokePrivateStaticAsync<T>(string methodName, Type[] genericTypes, params object[] args)
    {
        var method = typeof(MediaFileCrudService).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;
        var genericMethod = method.MakeGenericMethod(genericTypes);
        var task = (Task<T>)genericMethod.Invoke(null, args)!;
        return await task;
    }

    private static object? InvokePrivateInstance(MediaFileCrudService instance, string methodName, params object[] args)
    {
        var method = typeof(MediaFileCrudService).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return method.Invoke(instance, args);
    }

    private static MediaFileCrudService CreateSut(
        IContentQueryExecutor? queryExecutor = null,
        IHttpClientFactory? httpClientFactory = null,
        IHttpContextAccessor? httpContextAccessor = null,
        IWebHostEnvironment? hostEnvironment = null,
        IInfoProvider<ContentLanguageInfo>? languageInfoProvider = null)
    {
        return new MediaFileCrudService(
            queryExecutor ?? Mock.Of<IContentQueryExecutor>(),
            httpClientFactory ?? Mock.Of<IHttpClientFactory>(),
            httpContextAccessor ?? Mock.Of<IHttpContextAccessor>(),
            hostEnvironment ?? Mock.Of<IWebHostEnvironment>(),
            Mock.Of<IContentItemManagerFactory>(),
            Mock.Of<CMS.Membership.IUserInfoProvider>(),
            languageInfoProvider ?? Mock.Of<IInfoProvider<ContentLanguageInfo>>());
    }

}
