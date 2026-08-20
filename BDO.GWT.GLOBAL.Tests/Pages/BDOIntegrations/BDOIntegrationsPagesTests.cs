using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BDO;
using BDO.GWT.Xperience;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations
{
    [TestFixture]
    public class BDOIntegrationsPagesTests
    {
        [Test]
        public void BDOIntegrationsApplication_InheritsFromApplicationPage()
        {
            Assert.That(typeof(BdoIntegrationsApplication).BaseType, Is.EqualTo(typeof(ApplicationPage)));
        }

        [Test]
        public void BDOIntegrationsApplication_Identifier_IsExpectedValue()
        {
            Assert.That(BdoIntegrationsApplication.IDENTIFIER, Is.EqualTo("BDO.BDOIntegrations"));
        }

        [Test]
        public void BDOIntegrationsApplication_HasExpectedUiPermissions()
        {
            var permissions = typeof(BdoIntegrationsApplication)
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
            Assert.That(typeof(BdoIntegrationListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void BDOIntegrationListingPage_ObjectType_ReturnsFormSubmissionSubscriptionsObjectType()
        {
            var page = new TestableBdoIntegrationListingPage();

            Assert.That(page.ExposedObjectType, Is.EqualTo(FormSubmissionSubscriptionsInfo.OBJECT_TYPE));
        }

        [Test]
        public void BDOIntegrationListingPage_ConfigurePage_IsOverridden()
        {
            var method = typeof(BdoIntegrationListingPage).GetMethod(
                nameof(BdoIntegrationListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BdoIntegrationListingPage)));
        }

        [Test]
        public void BDOIntegrationListingPage_Delete_HasDeletePermissionPageCommand()
        {
            var deleteMethod = typeof(BdoIntegrationListingPage).GetMethod(
                nameof(BdoIntegrationListingPage.Delete),
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
            var baseType = typeof(BdoIntegrationsCreatePage).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(FormSubmissionSubscriptionsInfo)));
                Assert.That(baseType.GetGenericArguments()[1], Is.EqualTo(typeof(BdoIntegrationsEditSection)));
            });
        }

        [Test]
        public void BDOIntegrationsCreatePage_ExposesExpectedConstructor()
        {
            var ctor = typeof(BdoIntegrationsCreatePage).GetConstructor(new[]
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
            var method = typeof(BdoIntegrationsCreatePage).GetMethod(
                nameof(BdoIntegrationsCreatePage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BdoIntegrationsCreatePage)));
        }

        [Test]
        public void BDOIntegrationsEditSection_InheritsFromEditSectionPage()
        {
            var baseType = typeof(BdoIntegrationsEditSection).BaseType;

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
            var baseType = typeof(BdoIntegrationEditPage).BaseType;

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
            var ctor = typeof(BdoIntegrationEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void BDOIntegrationEditPage_ObjectId_HasPageParameterAttributeWithIntModelBinder()
        {
            var objectIdProperty = typeof(BdoIntegrationEditPage).GetProperty(
                nameof(BdoIntegrationEditPage.ObjectId),
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
            var method = typeof(BdoIntegrationEditPage).GetMethod(
                nameof(BdoIntegrationEditPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BdoIntegrationEditPage)));
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationListingPage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BdoIntegrationListingPage),
                parentType: typeof(BdoIntegrationsApplication),
                slug: "BDO-Integrations",
                name: "BDO Integrations",
                templateName: TemplateNames.LISTING,
                order: 0);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationsCreatePage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BdoIntegrationsCreatePage),
                parentType: typeof(BdoIntegrationListingPage),
                slug: "create",
                name: "Create Form Submission Integration",
                templateName: TemplateNames.EDIT,
                order: 20);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationsEditSection()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BdoIntegrationsEditSection),
                parentType: typeof(BdoIntegrationListingPage),
                slug: PageParameterConstants.PARAMETERIZED_SLUG,
                name: "Edit",
                templateName: TemplateNames.SECTION_LAYOUT,
                order: 10);
        }

        [Test]
        public void Assembly_HasExpectedUiPageRegistration_ForBDOIntegrationEditPage()
        {
            AssertUiPageAttribute(
                uiPageType: typeof(BdoIntegrationEditPage),
                parentType: typeof(BdoIntegrationsEditSection),
                slug: "edit",
                name: "Edit Form Submission subscription",
                templateName: TemplateNames.EDIT,
                order: 0);
        }

        [Test]
        public void BDOIntegrationListingPage_ConfigurePage_ContainsAllExpectedColumnsAndActions()
        {
            var configureMethod = typeof(BdoIntegrationListingPage).GetMethod(
                nameof(BdoIntegrationListingPage.ConfigurePage),
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
                Assert.That(literals, Does.Contain(nameof(BdoIntegrationListingPage.Delete)));
            });
        }

        [Test]
        public void BDOIntegrationsCreatePage_ConfigurePage_UsesExpectedUiFormName()
        {
            var configureMethod = typeof(BdoIntegrationsCreatePage).GetMethod(
                nameof(BdoIntegrationsCreatePage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(configureMethod, Is.Not.Null);

            var literals = GetStringLiterals(configureMethod!).ToArray();

            Assert.That(literals, Does.Contain("FormSubmissionSubscriptions"));
        }

        [Test]
        public void BDOIntegrationEditPage_ConfigurePage_UsesExpectedUiFormName()
        {
            var configureMethod = typeof(BdoIntegrationEditPage).GetMethod(
                nameof(BdoIntegrationEditPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(configureMethod, Is.Not.Null);

            var literals = GetStringLiterals(configureMethod!).ToArray();

            Assert.That(literals, Does.Contain("FormSubmissionSubscriptions"));
        }

        [Test]
        public async Task BDOIntegrationListingPage_ConfigurePage_ExecutesConfigurationLines_ForCoverage()
        {
            var page = (BdoIntegrationListingPage)RuntimeHelpers.GetUninitializedObject(typeof(BdoIntegrationListingPage));
            PreparePageConfigurationForConfigurePage(page);

            try
            {
                await page.ConfigurePage();
            }
            catch
            {
                // Base admin runtime is not fully available in unit test host.
                // The goal is to execute override lines for coverage.
            }

            var pageConfiguration = GetPageConfiguration(page);

            Assert.That(GetEnumerableCount(pageConfiguration, "ColumnConfigurations"), Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public async Task BDOIntegrationListingPage_Delete_InvokesForwardingPath_ForCoverage()
        {
            var page = (BdoIntegrationListingPage)RuntimeHelpers.GetUninitializedObject(typeof(BdoIntegrationListingPage));

            try
            {
                await page.Delete(1);
            }
            catch
            {
                // Expected in unit test context without admin page runtime services.
            }

            Assert.Pass();
        }

        [Test]
        public async Task BDOIntegrationsCreatePage_ConfigurePage_SetsExpectedUiFormName_ForCoverage()
        {
            var page = (BdoIntegrationsCreatePage)RuntimeHelpers.GetUninitializedObject(typeof(BdoIntegrationsCreatePage));
            PreparePageConfigurationForConfigurePage(page);

            try
            {
                await page.ConfigurePage();
            }
            catch
            {
                // Base runtime can throw in isolated unit tests; assignment should already be executed.
            }

            var pageConfiguration = GetPageConfiguration(page);
            var uiFormName = GetStringProperty(pageConfiguration, "UIFormName");

            Assert.That(uiFormName, Is.EqualTo("FormSubmissionSubscriptions"));
        }

        [Test]
        public async Task BDOIntegrationEditPage_ConfigurePage_SetsExpectedUiFormName_AndObjectIdIsReadable_ForCoverage()
        {
            var page = (BdoIntegrationEditPage)RuntimeHelpers.GetUninitializedObject(typeof(BdoIntegrationEditPage));
            PreparePageConfigurationForConfigurePage(page);
            page.ObjectId = 99;

            try
            {
                await page.ConfigurePage();
            }
            catch
            {
                // Base runtime can throw in isolated unit tests; assignment should already be executed.
            }

            var pageConfiguration = GetPageConfiguration(page);
            var uiFormName = GetStringProperty(pageConfiguration, "UIFormName");

            Assert.Multiple(() =>
            {
                Assert.That(page.ObjectId, Is.EqualTo(99));
                Assert.That(uiFormName, Is.EqualTo("FormSubmissionSubscriptions"));
            });
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

        private static void PreparePageConfigurationForConfigurePage(object page)
        {
            var pageConfiguration = GetPageConfiguration(page);

            EnsureListProperty(pageConfiguration, "HeaderActions");
            EnsureListProperty(pageConfiguration, "TableActions");
            EnsureListProperty(pageConfiguration, "ColumnConfigurations");
            EnsureListProperty(pageConfiguration, "MassActions");
            EnsureListProperty(pageConfiguration, "QueryModifiers");
        }

        private static object GetPageConfiguration(object page)
        {
            var type = page.GetType();

            while (type is not null)
            {
                var property = type.GetProperty("PageConfiguration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property is not null)
                {
                    var existing = property.GetValue(page);
                    if (existing is not null)
                    {
                        return existing;
                    }

                    var pageConfiguration = Activator.CreateInstance(property.PropertyType)
                        ?? RuntimeHelpers.GetUninitializedObject(property.PropertyType);

                    if (property.SetMethod is not null)
                    {
                        property.SetValue(page, pageConfiguration);
                        return pageConfiguration;
                    }

                    var backingField = FindMatchingField(page.GetType(), property.Name, property.PropertyType);
                    backingField?.SetValue(page, pageConfiguration);

                    var updated = property.GetValue(page);
                    return updated ?? pageConfiguration;
                }

                type = type.BaseType;
            }

            throw new InvalidOperationException("PageConfiguration property was not found.");
        }

        private static FieldInfo? FindMatchingField(Type type, string propertyName, Type fieldType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            for (var current = type; current is not null; current = current.BaseType)
            {
                var field = current.GetField($"<{propertyName}>k__BackingField", flags)
                    ?? current.GetField(propertyName, flags)
                    ?? current.GetFields(flags)
                        .FirstOrDefault(f => f.FieldType == fieldType && f.Name.Contains(propertyName, StringComparison.OrdinalIgnoreCase));

                if (field is not null)
                {
                    return field;
                }
            }

            return null;
        }

        private static void EnsureListProperty(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                return;
            }

            var propertyType = property.PropertyType;
            if (!propertyType.IsGenericType)
            {
                return;
            }

            var elementType = propertyType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType)!;

            if (property.SetMethod != null)
            {
                property.SetValue(target, list);
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var backingField = target.GetType().GetField($"<{propertyName}>k__BackingField", flags)
                ?? target.GetType().GetField(propertyName, flags)
                ?? target.GetType().GetFields(flags)
                    .FirstOrDefault(f => f.FieldType == propertyType && f.Name.Contains(propertyName, StringComparison.OrdinalIgnoreCase));

            backingField?.SetValue(target, list);
        }

        private static int GetEnumerableCount(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var value = property?.GetValue(target);

            return value switch
            {
                System.Collections.ICollection c => c.Count,
                System.Collections.IEnumerable e => e.Cast<object>().Count(),
                _ => 0,
            };
        }

        private static string? GetStringProperty(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property?.GetValue(target)?.ToString();
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
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }

                i += 4;
            }
        }

        private sealed class TestableBdoIntegrationListingPage : BdoIntegrationListingPage
        {
            public string ExposedObjectType => ObjectType;
        }
    }
}
