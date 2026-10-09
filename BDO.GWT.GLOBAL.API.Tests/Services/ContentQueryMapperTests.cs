using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ContentQueryMapperTests
{
    private static readonly Type[] ExpectedGenericArguments = { typeof(BDO.ContentAttachment) };
    private static readonly int[] ExpectedContentItemIds = { 1, 2 };
    [Test]
    public void Constructor_WhenContentQueryExecutorIsNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ContentQueryMapper(null!, Mock.Of<IContentQueryModelTypeMapper>()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(
                exception!.ParamName,
                Is.EqualTo("contentQueryExecutor"));
        });
    }

    [Test]
    public void Constructor_WhenContentQueryModelTypeMapperIsNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ContentQueryMapper(Mock.Of<IContentQueryExecutor>(), null!));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(
                exception!.ParamName,
                Is.EqualTo("contentQueryModelTypeMapper"));
        });
    }

    [Test]
    public async Task GetMappedResultAsync_DelegatesToContentQueryExecutor_AndReturnsMappedItems()
    {
        // Arrange
        var expected = new[]
        {
            new BDO.ContentAttachment
            {
                SystemFields = new ContentItemFields
                {
                    ContentItemID = 42,
                    ContentItemGUID = Guid.NewGuid()
                }
            }
        };

        var query = new ContentItemQueryBuilder();

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        MethodInfo? invokedMethod = null;
        object?[]? invokedArguments = null;
        var invocationCount = 0;

        var executor = CreateContentQueryExecutorProxy(
            (method, arguments) =>
            {
                invocationCount++;
                invokedMethod = method;
                invokedArguments = arguments;

                return Task.FromResult<IEnumerable<BDO.ContentAttachment>>(
                    expected);
            });

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var result =
            await mapper.GetMappedResultAsync<BDO.ContentAttachment>(
                query,
                cancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(invocationCount, Is.EqualTo(1));

            Assert.That(invokedMethod, Is.Not.Null);
            Assert.That(
                invokedMethod!.Name,
                Is.EqualTo("GetResult"));

            Assert.That(
                invokedMethod.IsGenericMethod,
                Is.True);

            Assert.That(
                invokedMethod.GetGenericArguments(),
                Is.EqualTo(ExpectedGenericArguments));

            Assert.That(invokedArguments, Is.Not.Null);
            Assert.That(invokedArguments, Is.Not.Empty);

            Assert.That(
                invokedArguments![0],
                Is.SameAs(query));
        });

        AssertCancellationTokenWasForwarded(
            invokedMethod!,
            invokedArguments!,
            cancellationToken);
    }

    [Test]
    public async Task GetMappedResultAsync_WhenExecutorReturnsEmptyCollection_ReturnsSameEmptyCollection()
    {
        // Arrange
        IEnumerable<BDO.ContentAttachment> expected =
            Array.Empty<BDO.ContentAttachment>();

        var executor = CreateContentQueryExecutorProxy(
            (_, _) => Task.FromResult(expected));

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var result =
            await mapper.GetMappedResultAsync<BDO.ContentAttachment>(
                new ContentItemQueryBuilder(),
                CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetMappedResultAsync_WhenExecutorReturnsMultipleItems_ReturnsAllItems()
    {
        // Arrange
        var firstItemGuid = Guid.NewGuid();
        var secondItemGuid = Guid.NewGuid();

        var expected = new[]
        {
            new BDO.ContentAttachment
            {
                SystemFields = new ContentItemFields
                {
                    ContentItemID = 1,
                    ContentItemGUID = firstItemGuid
                }
            },
            new BDO.ContentAttachment
            {
                SystemFields = new ContentItemFields
                {
                    ContentItemID = 2,
                    ContentItemGUID = secondItemGuid
                }
            }
        };

        var executor = CreateContentQueryExecutorProxy(
            (_, _) =>
                Task.FromResult<IEnumerable<BDO.ContentAttachment>>(
                    expected));

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var result =
            await mapper.GetMappedResultAsync<BDO.ContentAttachment>(
                new ContentItemQueryBuilder(),
                CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));

            Assert.That(
                result.Select(
                    item => item.SystemFields.ContentItemID),
                Is.EqualTo(ExpectedContentItemIds));

            Assert.That(
                result.Select(
                    item => item.SystemFields.ContentItemGUID),
                Is.EqualTo(
                    new[]
                    {
                        firstItemGuid,
                        secondItemGuid
                    }));
        });
    }

    [Test]
    public void GetMappedResultAsync_WhenExecutorThrowsSynchronously_PropagatesExactException()
    {
        // Arrange
        var expectedException =
            new InvalidOperationException(
                "Executor failed synchronously.");

        var executor = CreateContentQueryExecutorProxy(
            (_, _) => throw expectedException);

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var actualException =
            Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await mapper
                        .GetMappedResultAsync<BDO.ContentAttachment>(
                            new ContentItemQueryBuilder(),
                            CancellationToken.None));

        // Assert
        Assert.That(
            actualException,
            Is.SameAs(expectedException));
    }

    [Test]
    public void GetMappedResultAsync_WhenExecutorReturnsFaultedTask_PropagatesExactException()
    {
        // Arrange
        var expectedException =
            new InvalidOperationException(
                "Executor task failed.");

        var executor = CreateContentQueryExecutorProxy(
            (_, _) =>
                Task.FromException<
                    IEnumerable<BDO.ContentAttachment>>(
                        expectedException));

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var actualException =
            Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await mapper
                        .GetMappedResultAsync<BDO.ContentAttachment>(
                            new ContentItemQueryBuilder(),
                            CancellationToken.None));

        // Assert
        Assert.That(
            actualException,
            Is.SameAs(expectedException));
    }

    [Test]
    public void GetMappedResultAsync_WhenExecutorReturnsCanceledTask_PropagatesCancellation()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var cancellationToken =
            cancellationTokenSource.Token;

        var executor = CreateContentQueryExecutorProxy(
            (_, _) =>
                Task.FromCanceled<
                    IEnumerable<BDO.ContentAttachment>>(
                        cancellationToken));

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        var exception =
            Assert.CatchAsync<OperationCanceledException>(
                async () =>
                    await mapper
                        .GetMappedResultAsync<BDO.ContentAttachment>(
                            new ContentItemQueryBuilder(),
                            cancellationToken));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);

            Assert.That(
                exception!.CancellationToken,
                Is.EqualTo(cancellationToken));
        });
    }

    [Test]
    public void GetMappedResultAsync_WhenTokenIsAlreadyCanceled_ForwardsTokenToExecutor()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var cancellationToken =
            cancellationTokenSource.Token;

        MethodInfo? invokedMethod = null;
        object?[]? invokedArguments = null;

        var executor = CreateContentQueryExecutorProxy(
            (method, arguments) =>
            {
                invokedMethod = method;
                invokedArguments = arguments;

                return Task.FromCanceled<
                    IEnumerable<BDO.ContentAttachment>>(
                        cancellationToken);
            });

        var mapper = new ContentQueryMapper(executor, Mock.Of<IContentQueryModelTypeMapper>());

        // Act
        Assert.CatchAsync<OperationCanceledException>(
            async () =>
                await mapper
                    .GetMappedResultAsync<BDO.ContentAttachment>(
                        new ContentItemQueryBuilder(),
                        cancellationToken));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(invokedMethod, Is.Not.Null);
            Assert.That(invokedArguments, Is.Not.Null);
        });

        AssertCancellationTokenWasForwarded(
            invokedMethod!,
            invokedArguments!,
            cancellationToken);
    }

    private static void AssertCancellationTokenWasForwarded(
        MethodInfo invokedMethod,
        object?[] invokedArguments,
        CancellationToken expectedCancellationToken)
    {
        var parameters = invokedMethod.GetParameters();

        var cancellationTokenParameterIndex =
            Array.FindIndex(
                parameters,
                parameter =>
                    parameter.ParameterType ==
                    typeof(CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(
                cancellationTokenParameterIndex,
                Is.GreaterThanOrEqualTo(0),
                "The GetMappedResult method does not have a CancellationToken parameter.");

            Assert.That(
                invokedArguments,
                Has.Length.EqualTo(parameters.Length),
                "The captured argument count does not match the method parameter count.");

            Assert.That(
                invokedArguments[cancellationTokenParameterIndex],
                Is.EqualTo(expectedCancellationToken));
        });
    }

    private static IContentQueryExecutor CreateContentQueryExecutorProxy(
        Func<MethodInfo, object?[]?, object?> onInvoke)
    {
        ArgumentNullException.ThrowIfNull(onInvoke);

        var executor =
            DispatchProxy.Create<
                IContentQueryExecutor,
                ContentQueryExecutorProxy>();

        var proxy =
            ContentQueryExecutorProxy.GetProxy(executor);

        proxy.OnInvoke = onInvoke;

        return executor;
    }

    private class ContentQueryExecutorProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?>? OnInvoke
        {
            get;
            set;
        }

        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args)
        {
            if (targetMethod is null)
            {
                throw new InvalidOperationException(
                    "The proxy was invoked without a target method.");
            }

            if (OnInvoke is null)
            {
                throw new InvalidOperationException(
                    "The proxy callback has not been configured.");
            }

            return OnInvoke(targetMethod, args);
        }

        public static ContentQueryExecutorProxy GetProxy(
            IContentQueryExecutor proxy)
        {
            return (ContentQueryExecutorProxy)(object)proxy;
        }
    }
}