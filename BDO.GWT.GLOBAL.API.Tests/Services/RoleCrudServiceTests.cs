using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Workspaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class RoleCrudServiceTests
{
    private static RoleCrudService CreateUninitializedService() =>
        (RoleCrudService)RuntimeHelpers.GetUninitializedObject(typeof(RoleCrudService));

    private static RoleCrudService CreateService()
    {
        return new RoleCrudService(
            new Mock<IInfoProvider<UserInfo>>().Object,
            new Mock<IInfoProvider<UserRoleInfo>>().Object,
            new Mock<IInfoProvider<RoleInfo>>().Object,
            new Mock<IInfoProvider<ApplicationPermissionInfo>>().Object,
            new Mock<IInfoProvider<WorkspaceDataPermissionInfo>>().Object,
            new Mock<IInfoProvider<WorkspaceInfo>>().Object,
            new Mock<IInfoProvider<EmailChannelInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclRoleInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclRolePermissionInfo>>().Object,
            new Mock<IInfoProvider<WebsiteChannelInfo>>().Object,
            new Mock<IInfoProvider<ChannelInfo>>().Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<CMS.Core.IEventLogService>().Object);
    }

    [Test]
    public void ImplementsRoleCrudServiceContract()
    {
        Assert.That(CreateUninitializedService(), Is.InstanceOf<IRoleCrudService>());
    }

    [Test]
    public void ContractContainsAllPublicOperations()
    {
        var contract = typeof(IRoleCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetRolesAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.UpdateRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.DeleteRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.AssignRolePermissionsAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateChannelRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateChannelRolesForAllChannelsAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetWebsiteChannelNamesAsync)), Is.Not.Null);
        });
    }

    [Test]
    public void ContractOperationsReturnExpectedTaskTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetRolesAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<RoleOutput>>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput?>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.UpdateRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.DeleteRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.AssignRolePermissionsAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateChannelRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateChannelRolesForAllChannelsAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<RoleOutput>>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetWebsiteChannelNamesAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<string>>)));
        });
    }

    [Test]
    public void EveryContractOperationAcceptsCancellationToken()
    {
        foreach (var method in typeof(IRoleCrudService).GetMethods())
        {
            Assert.That(method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken)),
                Is.True, $"{method.Name} should accept a CancellationToken");
        }
    }

    [Test]
    public void ConstructorWithAllDependenciesCreatesService()
    {
        Assert.That(CreateService(), Is.Not.Null);
    }

    [Test]
    public void ConstructorAssignsAllInstanceDependencies()
    {
        var service = CreateService();
        var dependencyFields = typeof(RoleCrudService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(field => field.Name.StartsWith('_'));

        Assert.That(dependencyFields, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            foreach (var field in dependencyFields)
            {
                Assert.That(field.GetValue(service), Is.Not.Null, $"{field.Name} should be assigned");
            }
        });
    }

    [TestCase("Ind", "Channel 'Ind' was not found.")]
    [TestCase("", "Channel '' was not found.")]
    public void ChannelNotFoundExceptionContainsChannelName(string channelName, string expectedMessage)
    {
        var exception = new ChannelNotFoundException(channelName);

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void ChannelRoleExistExceptionContainsRoleName()
    {
        var exception = new ChannelRoleExistException("Ind-Editor");

        Assert.That(exception.Message, Is.EqualTo("Role 'Ind-Editor' already exists for this channel."));
    }

    [Test]
    public void RolePersistenceExceptionPreservesMessageAndInnerException()
    {
        var innerException = new InvalidOperationException("database failure");

        var exception = new RolePersistenceException("Could not save role.", innerException);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("Could not save role."));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        });
    }
}
