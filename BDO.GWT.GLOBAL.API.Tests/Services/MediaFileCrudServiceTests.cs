using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
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

        var service = new MediaFileCrudService(
            Mock.Of<IContentQueryExecutor>(),
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IWebHostEnvironment>(),
            Mock.Of<IContentItemManagerFactory>(),
            Mock.Of<CMS.Membership.IUserInfoProvider>(),
            languageProviderMock.Object);

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

        var service = new MediaFileCrudService(
            Mock.Of<IContentQueryExecutor>(),
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IWebHostEnvironment>(),
            Mock.Of<IContentItemManagerFactory>(),
            Mock.Of<CMS.Membership.IUserInfoProvider>(),
            languageProviderMock.Object);

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
            await service.DeleteMediaFileAsync("Demo", Guid.NewGuid(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Does.Contain("does not support automatic mapping"));
        });
    }
}
