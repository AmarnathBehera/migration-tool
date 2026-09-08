using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class LicenseKeysControllerTests
{
    private Mock<ILicenseKeysCrudService> _licenseKeysCrudServiceMock = null!;
    private LicenseKeysController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _licenseKeysCrudServiceMock = new Mock<ILicenseKeysCrudService>();
        _controller = new LicenseKeysController(_licenseKeysCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static LicenseKeyOutput CreateLicenseKeyOutput(int id = 1, string? key = "test-key") =>
        new() { ID = id, Key = key };

}