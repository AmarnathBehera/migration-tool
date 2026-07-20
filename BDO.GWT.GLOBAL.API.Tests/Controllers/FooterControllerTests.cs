using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.Tests;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class FooterControllerUnitTests
{
    private readonly Mock<IFooterService> _mockFooterService = new();
    private readonly Mock<ICultureValidationService> _mockCultureValidationService = new();
    private readonly Mock<IInfoProvider<ChannelInfo>> _mockChannelInfoProvider = new();

    private FooterController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 1;

        var routeData = new RouteData();

        var actionDescriptor = new ControllerActionDescriptor();

        var actionContext = new ActionContext(
            httpContext,
            routeData,
            actionDescriptor);

        _controller = new FooterController(_mockFooterService.Object, _mockCultureValidationService.Object, _mockChannelInfoProvider.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _mockFooterService.Reset();
        _mockCultureValidationService.Reset();
        _mockChannelInfoProvider.Reset();
        _controller.Dispose();
    }

    private void SetRouteValues(object? memberFirmID, object? culture)
    {
        _controller.HttpContext.Request.RouteValues["memberFirmID"] = memberFirmID;
        _controller.HttpContext.Request.RouteValues["culture"] = culture;
    }

    [Test]
    public async Task Get_InvalidMemberFirmID_ReturnsBadRequestAndEmptyFooterDto()
    {
        // Arrange
        SetRouteValues("notANumber", "en-GB");

        // Act
        var result = await _controller.Get(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(
                _controller.HttpContext.Response.StatusCode,
                Is.EqualTo(StatusCodes.Status400BadRequest));

            Assert.That(result.Value, Is.Not.Null);
        }

        _mockFooterService.Verify(
            x => x.GetAsyncForAPI(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

   
    [Test]
    public async Task Get_InvalidCulture_ReturnsBadRequestAndEmptyFooterDto()
    {
        // Arrange
        SetRouteValues("1", "xx-INVALID");

        _mockCultureValidationService
            .Setup(x => x.IsValidCulture("xx-INVALID"))
            .Returns(false);

        // Act
        var result = await _controller.Get(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(
                _controller.HttpContext.Response.StatusCode,
                Is.EqualTo(StatusCodes.Status400BadRequest));

            Assert.That(result.Value, Is.Not.Null);
        }

        _mockFooterService.Verify(
            x => x.GetAsyncForAPI(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    

    
}