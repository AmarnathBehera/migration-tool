using BDO.GWT.Core.ValueObjects;

namespace BDO.GWT.GLOBAL.Tests.ValueObjects
{
    public class AbsoluteLinkTests
    {
        #region Constructor – parameterless

        [Test]
        public void ParameterlessConstructor_CreatesInstance_WithoutThrowing()
        {
            Assert.DoesNotThrow(() => _ = new AbsoluteLink());
        }

        [Test]
        public void ParameterlessConstructor_AllOptionalProperties_AreNull()
        {
            var link = new AbsoluteLink();

            Assert.Multiple(() =>
            {
                Assert.That(link.SlashesAfterColon, Is.Null);
                Assert.That(link.UserInfo, Is.Null);
                Assert.That(link.Port, Is.Null);
                Assert.That(link.Path, Is.Null);
                Assert.That(link.Query, Is.Null);
                Assert.That(link.Hash, Is.Null);
            });
        }

        #endregion

        #region Constructor – string parsing

        [Test]
        public void StringConstructor_ParsesScheme()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.Scheme, Is.EqualTo("https"));
        }

        [Test]
        public void StringConstructor_ParsesHostName()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.HostName, Is.EqualTo("example.com"));
        }

        [Test]
        public void StringConstructor_ParsesPath()
        {
            var link = new AbsoluteLink("https://example.com/en/about-us");
            Assert.That(link.Path, Is.EqualTo("en/about-us"));
        }

        [Test]
        public void StringConstructor_PathIsNull_WhenNoPathSegment()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.Path, Is.Null);
        }

        [Test]
        public void StringConstructor_ParsesQuery()
        {
            var link = new AbsoluteLink("https://example.com/page?q=test&lang=en");
            Assert.That(link.Query, Is.EqualTo("q=test&lang=en"));
        }

        [Test]
        public void StringConstructor_QueryIsNull_WhenNoQueryString()
        {
            var link = new AbsoluteLink("https://example.com/page");
            Assert.That(link.Query, Is.Null);
        }

        [Test]
        public void StringConstructor_ParsesFragment_AsHash()
        {
            var link = new AbsoluteLink("https://example.com/page#section-1");
            Assert.That(link.Hash, Is.EqualTo("section-1"));
        }

        [Test]
        public void StringConstructor_HashIsNull_WhenNoFragment()
        {
            var link = new AbsoluteLink("https://example.com/page");
            Assert.That(link.Hash, Is.Null);
        }

        [Test]
        public void StringConstructor_ParsesNonDefaultPort()
        {
            var link = new AbsoluteLink("https://example.com:8080/path");
            Assert.That(link.Port, Is.EqualTo(8080));
        }

        [Test]
        public void StringConstructor_Port_IsNull_WhenDefaultHttpsPort()
        {
            // HTTPS default port (443) must be normalised to null
            var link = new AbsoluteLink("https://example.com:443/path");
            Assert.That(link.Port, Is.Null);
        }

        [Test]
        public void StringConstructor_Port_IsNull_WhenDefaultHttpPort()
        {
            // HTTP default port (80) must be normalised to null
            var link = new AbsoluteLink("http://example.com:80/path");
            Assert.That(link.Port, Is.Null);
        }

        [Test]
        public void StringConstructor_ParsesUserInfo()
        {
            var link = new AbsoluteLink("https://user:pass@example.com");
            Assert.That(link.UserInfo, Is.EqualTo("user:pass"));
        }

        [Test]
        public void StringConstructor_UserInfo_IsNull_WhenAbsent()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.UserInfo, Is.Null);
        }

        [Test]
        public void StringConstructor_SlashesAfterColon_IsDoubleSlash_ForHttps()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.SlashesAfterColon, Is.EqualTo("//"));
        }

        [Test]
        public void StringConstructor_SlashesAfterColon_IsDoubleSlash_ForHttp()
        {
            var link = new AbsoluteLink("http://example.com");
            Assert.That(link.SlashesAfterColon, Is.EqualTo("//"));
        }

        [Test]
        public void StringConstructor_SlashesAfterColon_IsTripleSlash_ForFileUri()
        {
            var link = new AbsoluteLink("file:///path/to/file");
            Assert.That(link.SlashesAfterColon, Is.EqualTo("///"));
        }

        [Test]
        public void StringConstructor_ThrowsArgumentException_ForRelativePath()
        {
            Assert.Throws<ArgumentException>(() => _ = new AbsoluteLink("/relative/path"));
        }

        [Test]
        public void StringConstructor_ThrowsArgumentException_ForBareRelativePath()
        {
            Assert.Throws<ArgumentException>(() => _ = new AbsoluteLink("relative/path"));
        }

        [Test]
        public void StringConstructor_ThrowsArgumentException_ForEmptyString()
        {
            Assert.Throws<ArgumentException>(() => _ = new AbsoluteLink(""));
        }

        [Test]
        public void StringConstructor_ParsesAllComponents_Together()
        {
            var link = new AbsoluteLink("https://user@example.com:9000/en/about?q=1#top");

            Assert.Multiple(() =>
            {
                Assert.That(link.Scheme, Is.EqualTo("https"));
                Assert.That(link.SlashesAfterColon, Is.EqualTo("//"));
                Assert.That(link.UserInfo, Is.EqualTo("user"));
                Assert.That(link.HostName, Is.EqualTo("example.com"));
                Assert.That(link.Port, Is.EqualTo(9000));
                Assert.That(link.Path, Is.EqualTo("en/about"));
                Assert.That(link.Query, Is.EqualTo("q=1"));
                Assert.That(link.Hash, Is.EqualTo("top"));
            });
        }

        #endregion

        #region ToString / ToHtmlCompatibleString

        [Test]
        public void ToString_SimpleUrl_ReturnsSchemeAndHost()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com"));
        }

        [Test]
        public void ToString_WithPath_IncludesPath()
        {
            var link = new AbsoluteLink("https://example.com/en/about");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com/en/about"));
        }

        [Test]
        public void ToString_WithQuery_IncludesQuery()
        {
            var link = new AbsoluteLink("https://example.com/page?q=test");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com/page?q=test"));
        }

        [Test]
        public void ToString_WithFragment_IncludesHash()
        {
            var link = new AbsoluteLink("https://example.com/page#section");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com/page#section"));
        }

        [Test]
        public void ToString_WithPort_IncludesPort()
        {
            var link = new AbsoluteLink("https://example.com:8080/path");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com:8080/path"));
        }

        [Test]
        public void ToString_WithUserInfo_IncludesUserInfo()
        {
            var link = new AbsoluteLink("https://admin@example.com");
            Assert.That(link.ToString(), Is.EqualTo("https://admin@example.com"));
        }

        [Test]
        public void ToString_AllComponents_ProducesCorrectUrl()
        {
            var link = new AbsoluteLink("https://user@example.com:9000/en/about?q=1#top");
            Assert.That(link.ToString(), Is.EqualTo("https://user@example.com:9000/en/about?q=1#top"));
        }

        [Test]
        public void ToString_NullPath_OmitsSlashAndPath()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.ToString(), Is.EqualTo("https://example.com"));
            Assert.That(link.ToString(), Does.Not.Contain("/en"));
        }

        [Test]
        public void ToString_NullQuery_OmitsQueryString()
        {
            var link = new AbsoluteLink("https://example.com/page");
            Assert.That(link.ToString(), Does.Not.Contain("?"));
        }

        [Test]
        public void ToString_NullHash_OmitsFragment()
        {
            var link = new AbsoluteLink("https://example.com/page");
            Assert.That(link.ToString(), Does.Not.Contain("#"));
        }

        [Test]
        public void ToString_FileUri_RoundTrips()
        {
            const string fileUri = "file:///path/to/file";
            var link = new AbsoluteLink(fileUri);
            Assert.That(link.ToString(), Is.EqualTo(fileUri));
        }

        [Test]
        public void ToHtmlCompatibleString_ReturnsSameAs_ToString()
        {
            var link = new AbsoluteLink("https://example.com/path?q=1#section");
            Assert.That(link.ToHtmlCompatibleString(), Is.EqualTo(link.ToString()));
        }

        #endregion

        #region ToUri

        [Test]
        public void ToUri_ReturnsSystemUri()
        {
            var link = new AbsoluteLink("https://example.com/path");
            var uri = link.ToUri();
            Assert.That(uri, Is.InstanceOf<Uri>());
        }

        [Test]
        public void ToUri_Uri_HasCorrectScheme()
        {
            var link = new AbsoluteLink("https://example.com");
            Assert.That(link.ToUri().Scheme, Is.EqualTo("https"));
        }

        [Test]
        public void ToUri_Uri_HasCorrectHost()
        {
            var link = new AbsoluteLink("https://example.com/path");
            Assert.That(link.ToUri().Host, Is.EqualTo("example.com"));
        }

        [Test]
        public void ToUri_Uri_HasCorrectPath()
        {
            var link = new AbsoluteLink("https://example.com/en/about");
            Assert.That(link.ToUri().AbsolutePath, Is.EqualTo("/en/about"));
        }

        #endregion

        #region + operator

        [Test]
        public void PlusOperator_NullBasePathAndRelativePath_YieldsRelativePath()
        {
            var left = new AbsoluteLink("https://example.com");          // Path = null
            var right = (RelativeLink)Link.Parse("/en/about");            // Path = "en/about"

            var result = left + right;

            Assert.That(result.Path, Is.EqualTo("en/about"));
        }

        [Test]
        public void PlusOperator_BasePathAndRelativePath_AreCombined()
        {
            var left = new AbsoluteLink("https://example.com/api");       // Path = "api"
            var right = (RelativeLink)Link.Parse("/v2/users");             // Path = "v2/users"

            var result = left + right;

            Assert.That(result.Path, Is.EqualTo("api/v2/users"));
        }

        [Test]
        public void PlusOperator_BothPathsNull_ResultPathIsNull()
        {
            var left = new AbsoluteLink("https://example.com");           // Path = null
            var right = new RelativeLink();                                // Path = null

            var result = left + right;

            Assert.That(result.Path, Is.Null);
        }

        [Test]
        public void PlusOperator_PreservesScheme_FromLeft()
        {
            var left = new AbsoluteLink("https://example.com");
            var right = (RelativeLink)Link.Parse("/path");

            var result = left + right;

            Assert.That(result.Scheme, Is.EqualTo("https"));
        }

        [Test]
        public void PlusOperator_PreservesHostName_FromLeft()
        {
            var left = new AbsoluteLink("https://example.com");
            var right = (RelativeLink)Link.Parse("/path");

            var result = left + right;

            Assert.That(result.HostName, Is.EqualTo("example.com"));
        }

        [Test]
        public void PlusOperator_PreservesPort_FromLeft()
        {
            var left = new AbsoluteLink("https://example.com:9000");
            var right = (RelativeLink)Link.Parse("/path");

            var result = left + right;

            Assert.That(result.Port, Is.EqualTo(9000));
        }

        [Test]
        public void PlusOperator_TakesQuery_FromRight()
        {
            var left = new AbsoluteLink("https://example.com/page?leftQuery=1");
            var right = (RelativeLink)Link.Parse("/new?rightQuery=2");

            var result = left + right;

            Assert.Multiple(() =>
            {
                Assert.That(result.Query, Is.EqualTo("rightQuery=2"));
                Assert.That(result.ToString(), Does.Not.Contain("leftQuery"));
            });
        }

        [Test]
        public void PlusOperator_LeftQuery_IsDiscarded_WhenRightQueryIsNull()
        {
            // The + operator always copies Query from the right-hand side, even if null.
            var left = new AbsoluteLink("https://example.com/page?keep=false");
            var right = (RelativeLink)Link.Parse("/new");                  // Query = null

            var result = left + right;

            Assert.That(result.Query, Is.Null);
        }

        [Test]
        public void PlusOperator_TakesHash_FromRight()
        {
            var left = new AbsoluteLink("https://example.com/page#leftHash");
            var right = (RelativeLink)Link.Parse("/new#rightHash");

            var result = left + right;

            Assert.Multiple(() =>
            {
                Assert.That(result.Hash, Is.EqualTo("rightHash"));
                Assert.That(result.ToString(), Does.Not.Contain("leftHash"));
            });
        }

        [Test]
        public void PlusOperator_ReturnsNewInstance_LeftNotMutated()
        {
            var left = new AbsoluteLink("https://example.com");
            var right = (RelativeLink)Link.Parse("/en/page");

            var result = left + right;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.SameAs(left));
                Assert.That(left.Path, Is.Null);   // left is untouched
            });
        }

        [Test]
        public void PlusOperator_ResultToString_IsCorrectAbsoluteUrl()
        {
            var left = new AbsoluteLink("https://example.com");
            var right = (RelativeLink)Link.Parse("/en/about");

            var result = left + right;

            Assert.That(result.ToString(), Is.EqualTo("https://example.com/en/about"));
        }

        [Test]
        public void PlusOperator_AcceptsAbsoluteLink_AsRightOperand_UsesOnlyPathFromRight()
        {
            // When an AbsoluteLink is passed as the right operand, scheme/host come from LEFT.
            var left = new AbsoluteLink("https://example.com");
            var right = new AbsoluteLink("https://ignored.com/api/resource?q=1");

            var result = left + right;

            Assert.Multiple(() =>
            {
                Assert.That(result.HostName, Is.EqualTo("example.com"));
                Assert.That(result.Path, Is.EqualTo("api/resource"));
                Assert.That(result.Query, Is.EqualTo("q=1"));
            });
        }

        #endregion

        #region Record equality

        [Test]
        public void Equality_TwoIdenticalInstances_AreEqual()
        {
            var link1 = new AbsoluteLink("https://example.com/path?q=1#top");
            var link2 = new AbsoluteLink("https://example.com/path?q=1#top");

            Assert.That(link1, Is.EqualTo(link2));
        }

        [Test]
        public void Equality_DifferentScheme_AreNotEqual()
        {
            var link1 = new AbsoluteLink("https://example.com");
            var link2 = new AbsoluteLink("http://example.com");

            Assert.That(link1, Is.Not.EqualTo(link2));
        }

        [Test]
        public void Equality_DifferentHostName_AreNotEqual()
        {
            var link1 = new AbsoluteLink("https://example.com");
            var link2 = new AbsoluteLink("https://other.com");

            Assert.That(link1, Is.Not.EqualTo(link2));
        }

        [Test]
        public void Equality_DifferentPath_AreNotEqual()
        {
            var link1 = new AbsoluteLink("https://example.com/en/about");
            var link2 = new AbsoluteLink("https://example.com/fr/about");

            Assert.That(link1, Is.Not.EqualTo(link2));
        }

        [Test]
        public void Equality_DifferentPort_AreNotEqual()
        {
            var link1 = new AbsoluteLink("https://example.com:8080");
            var link2 = new AbsoluteLink("https://example.com:9090");

            Assert.That(link1, Is.Not.EqualTo(link2));
        }

        #endregion

        #region Link.Parse

        [Test]
        public void LinkParse_ReturnsAbsoluteLink_ForAbsoluteUrl()
        {
            var link = Link.Parse("https://example.com/path");
            Assert.That(link, Is.InstanceOf<AbsoluteLink>());
        }

        [Test]
        public void LinkParse_ReturnsRelativeLink_ForRelativePath()
        {
            var link = Link.Parse("/relative/path");
            Assert.That(link, Is.InstanceOf<RelativeLink>());
        }

        [Test]
        public void LinkParse_AbsoluteLink_HasCorrectProperties()
        {
            var link = (AbsoluteLink)Link.Parse("https://example.com:8080/en/page?q=hello#anchor");

            Assert.Multiple(() =>
            {
                Assert.That(link.Scheme, Is.EqualTo("https"));
                Assert.That(link.HostName, Is.EqualTo("example.com"));
                Assert.That(link.Port, Is.EqualTo(8080));
                Assert.That(link.Path, Is.EqualTo("en/page"));
                Assert.That(link.Query, Is.EqualTo("q=hello"));
                Assert.That(link.Hash, Is.EqualTo("anchor"));
            });
        }

        [Test]
        public void LinkParse_StripsLeadingTilde_BeforeParsing()
        {
            // "~" prefix is stripped before parsing; the result is relative since no scheme
            var link = Link.Parse("~/en/about");
            Assert.That(link, Is.InstanceOf<RelativeLink>());
        }

        #endregion

        #region TryParse

        [Test]
        public void TryParse_ReturnsTrueAndAbsoluteLink_ForAbsoluteUrl()
        {
            var success = Link.TryParse("https://example.com/path", out var link);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(link, Is.InstanceOf<AbsoluteLink>());
            });
        }

        [Test]
        public void TryParse_ReturnsFalse_ForNull()
        {
            var success = Link.TryParse(null, out var link);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.False);
                Assert.That(link, Is.Null);
            });
        }

        [Test]
        public void TryParse_ReturnsTrueAndRelativeLink_ForRelativePath()
        {
            var success = Link.TryParse("/relative/path", out var link);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(link, Is.InstanceOf<RelativeLink>());
            });
        }

        #endregion

        #region ToAbsolute (inherited from Link)

        [Test]
        public void ToAbsolute_WithAbsoluteLinkRoot_UsesRootSchemeAndHost()
        {
            // A RelativeLink converted to absolute using a root AbsoluteLink
            var relative = (RelativeLink)Link.Parse("/en/about");
            var root = new AbsoluteLink("https://root.example.com");

            var result = relative.ToAbsolute(root);

            Assert.Multiple(() =>
            {
                Assert.That(result.Scheme, Is.EqualTo("https"));
                Assert.That(result.HostName, Is.EqualTo("root.example.com"));
            });
        }

        [Test]
        public void ToAbsolute_WithAbsoluteLinkRoot_PreservesOwnPath()
        {
            var relative = (RelativeLink)Link.Parse("/en/about?q=1#top");
            var root = new AbsoluteLink("https://root.example.com");

            var result = relative.ToAbsolute(root);

            Assert.Multiple(() =>
            {
                Assert.That(result.Path, Is.EqualTo("en/about"));
                Assert.That(result.Query, Is.EqualTo("q=1"));
                Assert.That(result.Hash, Is.EqualTo("top"));
            });
        }

        [Test]
        public void ToAbsolute_WithStringRoot_ProducesCorrectAbsoluteUrl()
        {
            var relative = (RelativeLink)Link.Parse("/en/contact");
            var result = relative.ToAbsolute("https://example.com");

            Assert.That(result.ToString(), Does.StartWith("https://example.com"));
            Assert.That(result.ToString(), Does.Contain("en/contact"));
        }

        [Test]
        public void ToAbsolute_WithStringRoot_ThrowsArgumentException_ForRelativeRoot()
        {
            var relative = (RelativeLink)Link.Parse("/en/contact");

            Assert.Throws<ArgumentException>(() => relative.ToAbsolute("/not-absolute"));
        }

        #endregion
    }
}