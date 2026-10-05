using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Filters;

[TestFixture]
public class ChannelNameParameterFilterTests
{
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private ChannelNameParameterFilter _filter = null!;

    private static readonly string[] ChannelNames = { "Ind", "Global" };

    [SetUp]
    public void SetUp()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _filter = new ChannelNameParameterFilter(_httpContextAccessorMock.Object);
    }

    private static ParameterFilterContext CreateContext() =>
        (ParameterFilterContext)RuntimeHelpers.GetUninitializedObject(typeof(ParameterFilterContext));

    private static OpenApiParameter CreateParameter(string name) =>
        new() { Name = name, Schema = new OpenApiSchema() };

    [Test]
    public void Apply_WhenParameterNameIsNotChannelcodeName_DoesNothing()
    {
        var parameter = CreateParameter("someOtherParameter");

        _filter.Apply(parameter, CreateContext());

        Assert.Multiple(() =>
        {
            Assert.That(parameter.Schema.Enum, Is.Empty);
            Assert.That(parameter.Schema.Type, Is.Null);
        });
        _httpContextAccessorMock.Verify(x => x.HttpContext, Times.Never);
    }

    [Test]
    public void Apply_WhenHttpContextIsNull_DoesNotModifySchema()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var parameter = CreateParameter("channelcodeName");

        _filter.Apply(parameter, CreateContext());

        Assert.That(parameter.Schema.Enum, Is.Empty);
    }

    [Test]
    public void Apply_WhenRoleServiceNotRegistered_DoesNotModifySchema()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IRoleCrudService))).Returns(null!);
        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProviderMock.Object });
        var parameter = CreateParameter("channelcodeName");

        _filter.Apply(parameter, CreateContext());

        Assert.That(parameter.Schema.Enum, Is.Empty);
    }

    [Test]
    public void Apply_WhenNamesAvailable_PopulatesEnumWithStringValues()
    {
        var roleServiceMock = new Mock<IRoleCrudService>();
        roleServiceMock
            .Setup(x => x.GetWebsiteChannelNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChannelNames);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IRoleCrudService))).Returns(roleServiceMock.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProviderMock.Object });

        var parameter = CreateParameter("channelcodeName");

        _filter.Apply(parameter, CreateContext());

        Assert.Multiple(() =>
        {
            Assert.That(parameter.Schema.Type, Is.EqualTo("string"));
            Assert.That(parameter.Schema.Enum, Has.Count.EqualTo(2));
            Assert.That(parameter.Schema.Enum.Cast<OpenApiString>().Select(s => s.Value),
                Is.EqualTo(ChannelNames));
        });
    }

    [Test]
    public void Apply_WhenNamesEmpty_SetsStringTypeWithEmptyEnum()
    {
        var roleServiceMock = new Mock<IRoleCrudService>();
        roleServiceMock
            .Setup(x => x.GetWebsiteChannelNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IRoleCrudService))).Returns(roleServiceMock.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { RequestServices = serviceProviderMock.Object });

        var parameter = CreateParameter("channelcodeName");

        _filter.Apply(parameter, CreateContext());

        Assert.Multiple(() =>
        {
            Assert.That(parameter.Schema.Type, Is.EqualTo("string"));
            Assert.That(parameter.Schema.Enum, Is.Empty);
        });
    }
}
