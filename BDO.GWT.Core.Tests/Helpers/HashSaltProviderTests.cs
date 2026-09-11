using BDO.GWT.Xperience.Core.Services;
using Microsoft.Extensions.Configuration;

namespace BDO.GWT.Core.Tests.Helpers;

public class HashSaltProviderTests
{
    [Test]
    public void HashSalt_ReturnsConfiguredValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CMSHashStringSalt"] = "test-salt"
            })
            .Build();

        var provider = new HashSaltProvider(configuration);

        Assert.That(provider.HashSalt, Is.EqualTo("test-salt"));
    }

    [Test]
    public void HashSalt_WhenMissingSetting_ReturnsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var provider = new HashSaltProvider(configuration);

        Assert.That(provider.HashSalt, Is.Null);
    }
}
