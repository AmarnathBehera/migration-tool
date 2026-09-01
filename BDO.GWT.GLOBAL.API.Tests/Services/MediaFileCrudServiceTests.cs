using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "File", "new.jpg"),
            DisplayName = "New file"
        };

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateMediaFileAsync("Demo", input, CancellationToken.None));

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

        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([4, 5, 6]), 0, 3, "File", "fallback-name.jpg"),
            DisplayName = null
        };

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateMediaFileAsync("Demo", input, CancellationToken.None));

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
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([1, 2]), 0, 2, "File", "replace.jpg")
        };

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateMediaFileAsync("Demo", Guid.NewGuid(), input, CancellationToken.None));

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
    public void GetSafeExtension_WhenContainsInvalidCharacter_ReturnsEmpty()
    {
        var result = (string)InvokePrivateStatic("GetSafeExtension", "photo.jp|g");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(string.Empty));
        });
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
    public void BuildAssetUri_WhenProductionAndNoHttpContext_ReturnsNull()
    {
        var hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Production");

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = CreateSut(httpContextAccessorMock.Object, hostEnvironmentMock.Object);

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

        var service = CreateSut(httpContextAccessorMock.Object, hostEnvironmentMock.Object);

        var result = (Uri?)InvokePrivateInstance(service, "BuildAssetUri", "getmedia/img.jpg");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AbsoluteUri, Is.EqualTo("https://example.com/getmedia/img.jpg"));
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

    private static object InvokePrivateStatic(string methodName, params object[] args)
    {
        var method = typeof(MediaFileCrudService).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;
        return method.Invoke(null, args)!;
    }

    private static object? InvokePrivateInstance(MediaFileCrudService instance, string methodName, params object[] args)
    {
        var method = typeof(MediaFileCrudService).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return method.Invoke(instance, args);
    }

    private static MediaFileCrudService CreateSut(
        IHttpContextAccessor? httpContextAccessor = null,
        IWebHostEnvironment? hostEnvironment = null,
        IInfoProvider<ContentLanguageInfo>? languageInfoProvider = null)
    {
        return new MediaFileCrudService(
            Mock.Of<IContentQueryExecutor>(),
            Mock.Of<IHttpClientFactory>(),
            httpContextAccessor ?? Mock.Of<IHttpContextAccessor>(),
            hostEnvironment ?? Mock.Of<IWebHostEnvironment>(),
            Mock.Of<IContentItemManagerFactory>(),
            Mock.Of<CMS.Membership.IUserInfoProvider>(),
            languageInfoProvider ?? Mock.Of<IInfoProvider<ContentLanguageInfo>>());
    }
}
