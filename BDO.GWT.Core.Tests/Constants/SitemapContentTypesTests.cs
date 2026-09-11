using BDO.GWT.Core.Constants;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Constants;

[TestFixture]
public class SitemapContentTypesTests
{
    [Test]
    public void SitemapPages_ShouldContainAllPageTypesThatImplementISearchAndSEO()
    {
        // Arrange – discover every concrete class in the BDO.GWT.Core assembly
        // that implements ISearchAndSEO and exposes a CONTENT_TYPE_NAME constant.
        var coreAssembly = typeof(SitemapContentTypes).Assembly;

        var missingTypes = coreAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && typeof(ISearchAndSEO).IsAssignableFrom(t))
            .Select(t => new
            {
                Type = t,
                ContentTypeName = t.GetField(
                    "CONTENT_TYPE_NAME",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.GetValue(null) as string
            })
            .Where(x => x.ContentTypeName is not null)
            .Where(x => !SitemapContentTypes.SitemapPages.Contains(x.ContentTypeName))
            .Select(x => $"{x.Type.Name} (CONTENT_TYPE_NAME = \"{x.ContentTypeName}\")")
            .ToList();

        // Assert
        Assert.That(
            missingTypes,
            Is.Empty,
            $"The following page types implement ISearchAndSEO but are missing from SitemapContentTypes.SitemapPages:\n"
            + string.Join("\n", missingTypes));
    }
}
