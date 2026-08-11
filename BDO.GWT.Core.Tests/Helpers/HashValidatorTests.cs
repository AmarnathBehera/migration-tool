using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Xperience.Core.Services;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace BDO.GWT.Core.Tests.Helpers;

public class HashValidatorTests
{
    [Test]
    public void Constructor_WhenSaltIsMissing_ThrowsInvalidOperationException()
    {
        var saltProvider = new Mock<IHashSaltProvider>();
        saltProvider.SetupGet(x => x.HashSalt).Returns((string?)null);

        var ex = Assert.Throws<InvalidOperationException>(() => new HashValidator(saltProvider.Object));

        Assert.That(ex!.Message, Does.Contain("CMSHashStringSalt"));
    }

    [Test]
    public void ValidateEmailHash_WhenHashMatches_ReturnsTrue()
    {
        const string email = "user@example.com";
        const string salt = "salt-123";

        var saltProvider = new Mock<IHashSaltProvider>();
        saltProvider.SetupGet(x => x.HashSalt).Returns(salt);

        var validator = new HashValidator(saltProvider.Object);
        var hash = ComputeExpectedHash($"{email}{salt}");

        var isValid = validator.ValidateEmailHash(email, hash);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ValidateEmailHash_WhenHashDoesNotMatch_ReturnsFalse()
    {
        var saltProvider = new Mock<IHashSaltProvider>();
        saltProvider.SetupGet(x => x.HashSalt).Returns("salt-123");

        var validator = new HashValidator(saltProvider.Object);

        var isValid = validator.ValidateEmailHash("user@example.com", "not-a-real-hash");

        Assert.That(isValid, Is.False);
    }

    private static string ComputeExpectedHash(string value)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(sha256.ComputeHash(bytes)).ToLower();
    }
}
