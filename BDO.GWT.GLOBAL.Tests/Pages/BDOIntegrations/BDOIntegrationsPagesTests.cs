using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BDO;
using BDO.GWT.Xperience;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations
{
    [TestFixture]
    public class BDOIntegrationsPagesTests
    {
        [Test]
        public void BDOIntegrationsApplication_InheritsFromApplicationPage()
        {
            Assert.That(typeof(BDOIntegrationsApplication).BaseType, Is.EqualTo(typeof(ApplicationPage)));
        }

        [Test]
        public void BDOIntegrationsApplication_Identifier_IsExpectedValue()
        {
            Assert.That(BDOIntegrationsApplication.IDENTIFIER, Is.EqualTo("BDO.BDOIntegrations"));
        }

        [Test]
        public void BDOIntegrationsApplication_HasExpectedUiPermissions()
        {
            var permissions = typeof(BDOIntegrationsApplication)
                .GetCustomAttributesData()
                .Where(a => a.AttributeType.Name == "UIPermissionAttribute")
                .Select(a => a.ConstructorArguments.FirstOrDefault().Value?.ToString())
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(permissions, Does.Contain(SystemPermissions.VIEW));
                Assert.That(permissions, Does.Contain(SystemPermissions.CREATE));
                Assert.That(permissions, Does.Contain(SystemPermissions.UPDATE));
                Assert.That(permissions, Does.Contain(SystemPermissions.DELETE));
            });
        }

        [Test]
        public void BDOIntegrationListingPage_InheritsFromListingPage()
        {
            Assert.That(typeof(BDOIntegrationListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void BDOIntegrationListingPage_ObjectType_ReturnsFormSubmissionSubscriptionsObjectType()
        {
            var page = new TestableBDOIntegrationListingPage();

            Assert.That(page.ExposedObjectType, Is.EqualTo(FormSubmissionSubscriptionsInfo.OBJECT_TYPE));
        }

        [Test]
        public void BDOIntegrationListingPage_ConfigurePage_IsOverridden()
        {
            var method = typeof(BDOIntegrationListingPage).GetMethod(
                nameof(BDOIntegrationListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BDOIntegrationListingPage)));
        }

        [Test]
        public void BDOIntegrationListingPage_Delete_HasDeletePermissionPageCommand()
        {
            var deleteMethod = typeof(BDOIntegrationListingPage).GetMethod(
                nameof(BDOIntegrationListingPage.Delete),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(deleteMethod, Is.Not.Null);

            var pageCommandAttribute = deleteMethod!
                .GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.Name == "PageCommandAttribute");

            Assert.That(pageCommandAttribute, Is.Not.Null);

            var permissionArgument = pageCommandAttribute!
                .NamedArguments
                .FirstOrDefault(a => a.MemberName == "Permission");

            Assert.That(permissionArgument.TypedValue.Value?.ToString(), Is.EqualTo(SystemPermissions.DELETE));
        }

        [Test]
        public void BDOIntegrationsCreatePage_InheritsFromCreatePage()
        {
            var baseType = typeof(BDOIntegrationsCreatePage).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
                Assert.That(baseType.GetGenericArguments()[1], Is.EqualTo(typeof(BDOIntegrationsEditSection)));
            });
        }

        [Test]
        public void BDOIntegrationsCreatePage_ExposesExpectedConstructor()
        {
            var ctor = typeof(BDOIntegrationsCreatePage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IPageLinkGenerator),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void BDOIntegrationsCreatePage_ConfigurePage_IsOverridden()
        {
            var method = typeof(BDOIntegrationsCreatePage).GetMethod(
                nameof(BDOIntegrationsCreatePage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BDOIntegrationsCreatePage)));
        }

        [Test]
        public void BDOIntegrationsEditSection_InheritsFromEditSectionPage()
        {
            var baseType = typeof(BDOIntegrationsEditSection).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
            });
        }

        [Test]
        public void BDOIntegrationEditPage_InheritsFromInfoEditPage()
        {
            var baseType = typeof(BDOIntegrationEditPage).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
            });
        }

        [Test]
        public void BDOIntegrationEditPage_ExposesExpectedConstructor()
        {
            var ctor = typeof(BDOIntegrationEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void BDOIntegrationEditPage_ObjectId_HasPageParameterAttributeWithIntModelBinder()
        {
            var objectIdProperty = typeof(BDOIntegrationEditPage).GetProperty(
                nameof(BDOIntegrationEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(objectIdProperty, Is.Not.Null);

            var attributeData = objectIdProperty!
                .GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType == typeof(PageParameterAttribute));

            Assert.That(attributeData, Is.Not.Null);
            Assert.That(attributeData!.ConstructorArguments[0].Value, Is.EqualTo(typeof(IntPageModelBinder)));
        }

        [Test]
        public void BDOIntegrationEditPage_ConfigurePage_IsOverridden()
        {
            var method = typeof(BDOIntegrationEditPage).GetMethod(
                nameof(BDOIntegrationEditPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BDOIntegrationEditPage)));
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationListingPage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BDOIntegrationListingPage),
                parentType: typeof(BDOIntegrationsApplication),
                slug: "BDO-Integrations",
                name: "BDO Integrations",
                templateName: TemplateNames.LISTING,
                order: 0);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationsCreatePage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BDOIntegrationsCreatePage),
                parentType: typeof(BDOIntegrationListingPage),
                slug: "create",
                name: "Create Form Submission Integration",
                templateName: TemplateNames.EDIT,
                order: 20);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationsEditSection()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BDOIntegrationsEditSection),
                parentType: typeof(BDOIntegrationListingPage),
                slug: PageParameterConstants.PARAMETERIZED_SLUG,
                name: "Edit",
                templateName: TemplateNames.SECTION_LAYOUT,
                order: 10);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationEditPage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BDOIntegrationEditPage),
                parentType: typeof(BDOIntegrationsEditSection),
                slug: "edit",
                name: "Edit Form Submission subscription",
                templateName: TemplateNames.EDIT,
                order: 0);
        }

        [Test]
        public void BDOIntegrationListingPage_ConfigurePage_ContainsAllExpectedColumnsAndActions()
        {
            var configureMethod = typeof(BDOIntegrationListingPage).GetMethod(
                nameof(BDOIntegrationListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(configureMethod, Is.Not.Null);

            var literals = GetStringLiterals(configureMethod!).ToArray();

            Assert.Multiple(() =>
            {
                // Column keys (nameof(...))
                Assert.That(literals, Does.Contain(nameof(FormSubmissionSubscriptionsInfo.Displayname)));
                Assert.That(literals, Does.Contain(nameof(FormSubmissionSubscriptionsInfo.Codename)));
                Assert.That(literals, Does.Contain(nameof(FormSubmissionSubscriptionsInfo.Form)));
                Assert.That(literals, Does.Contain(nameof(FormSubmissionSubscriptionsInfo.Method)));
                Assert.That(literals, Does.Contain(nameof(FormSubmissionSubscriptionsInfo.Endpoint)));

                // Column titles + actions
                Assert.That(literals, Does.Contain("Display name"));
                Assert.That(literals, Does.Contain("Code name"));
                Assert.That(literals, Does.Contain("Form"));
                Assert.That(literals, Does.Contain("Method"));
                Assert.That(literals, Does.Contain("Endpoint"));
                Assert.That(literals, Does.Contain("New setting"));
                Assert.That(literals, Does.Contain(nameof(BDOIntegrationListingPage.Delete)));
            });
        }

        [Test]
        public void BDOIntegrationsCreatePage_ConfigurePage_UsesExpectedUiFormName()
        {
            var configureMethod = typeof(BDOIntegrationsCreatePage).GetMethod(
                nameof(BDOIntegrationsCreatePage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(configureMethod, Is.Not.Null);

            var literals = GetStringLiterals(configureMethod!).ToArray();

            Assert.That(literals, Does.Contain("FormSubmissionSubscriptions"));
        }

        [Test]
        public void BDOIntegrationEditPage_ConfigurePage_UsesExpectedUiFormName()
        {
            var configureMethod = typeof(BDOIntegrationEditPage).GetMethod(
                nameof(BDOIntegrationEditPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(configureMethod, Is.Not.Null);

            var literals = GetStringLiterals(configureMethod!).ToArray();

            Assert.That(literals, Does.Contain("FormSubmissionSubscriptions"));
        }

        private static void AssertUiPageAttribute(
            Type uiPageType,
            Type parentType,
            string slug,
            string name,
            string templateName,
            int order)
        {
            var uiPageAttribute = uiPageType.Assembly
                .GetCustomAttributesData()
                .Where(a => a.AttributeType.Name == "UIPageAttribute")
                .FirstOrDefault(a =>
                    a.ConstructorArguments.Count >= 3 &&
                    Equals(a.ConstructorArguments[2].Value, uiPageType));

            Assert.That(uiPageAttribute, Is.Not.Null, $"UIPage attribute not found for {uiPageType.Name}.");

            Assert.Multiple(() =>
            {
                Assert.That(uiPageAttribute!.ConstructorArguments[0].Value, Is.EqualTo(parentType));
                Assert.That(uiPageAttribute.ConstructorArguments[1].Value?.ToString(), Is.EqualTo(slug));
                Assert.That(uiPageAttribute.ConstructorArguments[2].Value, Is.EqualTo(uiPageType));
                Assert.That(uiPageAttribute.ConstructorArguments[3].Value?.ToString(), Is.EqualTo(name));
                Assert.That(uiPageAttribute.ConstructorArguments[4].Value?.ToString(), Is.EqualTo(templateName));
                Assert.That((int)uiPageAttribute.ConstructorArguments[5].Value!, Is.EqualTo(order));
            });
        }

        private static IEnumerable<string> GetStringLiterals(MethodInfo method)
        {
            foreach (var literal in GetStringLiteralsFromMethodBody(method))
            {
                yield return literal;
            }

            var asyncAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
            var moveNext = asyncAttribute?.StateMachineType.GetMethod(
                "MoveNext",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (moveNext is null)
            {
                yield break;
            }

            foreach (var literal in GetStringLiteralsFromMethodBody(moveNext))
            {
                yield return literal;
            }
        }

        private static IEnumerable<string> GetStringLiteralsFromMethodBody(MethodInfo method)
        {
            var body = method.GetMethodBody();
            if (body is null)
            {
                yield break;
            }

            var il = body.GetILAsByteArray();
            if (il is null || il.Length == 0)
            {
                yield break;
            }

            for (var i = 0; i < il.Length - 4; i++)
            {
                // 0x72 = ldstr <metadata token>
                if (il[i] != OpCodes.Ldstr.Value)
                {
                    continue;
                }

                var token = BitConverter.ToInt32(il, i + 1);
                string? value = null;

                try
                {
                    value = method.Module.ResolveString(token);
                }
                catch
                {
                    // Ignore non-resolvable tokens and continue scanning.
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }

                i += 4;
            }
        }

        private sealed class TestableBDOIntegrationListingPage : BDOIntegrationListingPage
        {
            public string ExposedObjectType => ObjectType;
        }
    }
}
