using BDO.GWT.GLOBAL.Sync.Attributes;
using BDO.GWT.GLOBAL.Sync.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.Sync.Tests.Attributes;

[TestFixture]
public class AuthorizeApiKeyAttributeTests
{
    [Test]
    public void Attribute_DerivesFromTypeFilterAttribute()
    {
        var attribute = new AuthorizeApiKeyAttribute();

        Assert.That(attribute, Is.InstanceOf<TypeFilterAttribute>());
    }

    [Test]
    public void Attribute_UsesApiKeyAuthorizationFilterImplementationType()
    {
        var attribute = new AuthorizeApiKeyAttribute();

        Assert.That(attribute.ImplementationType, Is.EqualTo(typeof(ApiKeyAuthorizationFilter)));
    }

    [Test]
    public void Attribute_DefaultConstructor_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _ = new AuthorizeApiKeyAttribute());
    }
}
