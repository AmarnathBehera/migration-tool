using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Components.VisibilityConditions;
using CMS.Membership;
using Kentico.Membership;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.VisibilityConditions;

[TestFixture]
public class NonEnglishOrAdminConditionTests
{
    private Mock<IAuthenticatedUserAccessor> _userAccessor = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
    private Mock<IFormFieldValueProvider> _formFieldValueProvider = null!;
    private Mock<ILogger<NonEnglishOrAdminCondition>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _userAccessor = new Mock<IAuthenticatedUserAccessor>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _formFieldValueProvider = new Mock<IFormFieldValueProvider>();
        _logger = new Mock<ILogger<NonEnglishOrAdminCondition>>();
    }

    private NonEnglishOrAdminCondition CreateCondition() =>
        new(_userAccessor.Object, _httpContextAccessor.Object, _logger.Object);

    private void SetupUser(bool isAdmin) =>
        _userAccessor.Setup(a => a.Get()).ReturnsAsync(MakeUser(isAdmin));

    private void SetupReferer(string? referer)
    {
        var httpContext = new DefaultHttpContext();
        if (referer is not null)
        {
            httpContext.Request.Headers.Referer = referer;
        }

        _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContext);
    }

    private static TestAdminApplicationUser MakeUser(bool isAdmin)
    {
        var user = (TestAdminApplicationUser)RuntimeHelpers.GetUninitializedObject(typeof(TestAdminApplicationUser));
        user.Admin = isAdmin;
        return user;
    }

    [Test]
    public void Evaluate_AdminUser_NoReferer_ReturnsTrue()
    {
        SetupUser(isAdmin: true);
        SetupReferer(null);

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_AdminUser_EnglishReferer_ReturnsTrue()
    {
        SetupUser(isAdmin: true);
        SetupReferer("https://example.com/en_1/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_NonAdminUser_EnglishReferer_ReturnsFalse()
    {
        SetupUser(isAdmin: false);
        SetupReferer("https://example.com/en_1/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_NonAdminUser_EnglishRegionalReferer_ReturnsFalse()
    {
        SetupUser(isAdmin: false);
        SetupReferer("https://example.com/en-us_1/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_NonAdminUser_EnglishRefererDifferentCasing_ReturnsFalse()
    {
        SetupUser(isAdmin: false);
        SetupReferer("https://example.com/EN_1/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_NonAdminUser_NonEnglishReferer_ReturnsTrue()
    {
        SetupUser(isAdmin: false);
        SetupReferer("https://example.com/fr_2/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_NonAdminUser_NoReferer_ReturnsTrue()
    {
        SetupUser(isAdmin: false);
        SetupReferer(null);

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_NonAdminUser_RefererWithoutLanguageSegment_ReturnsTrue()
    {
        SetupUser(isAdmin: false);
        SetupReferer("https://example.com/global-locations");

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_NonAdminUser_NoHttpContext_ReturnsTrue()
    {
        SetupUser(isAdmin: false);
        _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_WhenUserAccessorThrows_ReturnsTrueAndLogsError()
    {
        _userAccessor.Setup(a => a.Get()).ThrowsAsync(new InvalidOperationException("boom"));
        SetupReferer(null);

        bool result = CreateCondition().Evaluate(_formFieldValueProvider.Object);

        Assert.That(result, Is.True);
        _logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Test subclass overrides virtual members so reads/writes bypass Kentico's
    // AbstractInfoBase.GetValue/SetValue, which require an initialised CMS IoC container.
    private sealed class TestAdminApplicationUser : AdminApplicationUser
    {
        public bool Admin;

        public override bool UserAdministrationAccess { get => true; set { } }

        public override bool IsInRole(string roleName) => Admin;
    }
}
