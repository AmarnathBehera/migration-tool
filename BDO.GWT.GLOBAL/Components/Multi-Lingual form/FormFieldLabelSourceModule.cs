using BDO.Components.Widgets.FormWidget;
using CMS;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Localization;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Forms.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using XperienceCommunity.Localization;

[assembly: RegisterModule(typeof(FormFieldLabelSourceModule))]

namespace BDO.Components.Widgets.FormWidget
{
    /// <summary>
    /// Allows the label of a form field rendered by the Form widget to be resolved in code, based on the
    /// current page's language, instead of the static text entered in the Form Builder's Field editor.
    /// Editors opt a field in by typing a LocalizationKey item's name (e.g. "bdo.form.title") directly
    /// into the field's Label property. Any label that does NOT resolve to a real LocalizationKey item is
    /// left completely untouched, so plain-text labels keep working exactly as before.
    /// </summary>
    internal class FormFieldLabelSourceModule : Module
    {
        private IHttpContextAccessor httpContextAccessor;

        public FormFieldLabelSourceModule()
            : base(nameof(FormFieldLabelSourceModule))
        {
        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit(parameters);

            httpContextAccessor = parameters.Services.GetRequiredService<IHttpContextAccessor>();

            FormFieldRenderingConfiguration.GetConfiguration.Execute += SetLabelFromCode;
        }

        private void SetLabelFromCode(object sender, GetFormFieldRenderingConfigurationEventArgs e)
        {
            var rawLabel = e.FormComponent?.BaseProperties?.Label;

            if (string.IsNullOrWhiteSpace(rawLabel))
            {
                return;
            }

            var preferredLanguageRetriever = httpContextAccessor.HttpContext?.RequestServices.GetService<IPreferredLanguageRetriever>();
            var languageName = preferredLanguageRetriever?.Get();

            var resolved = ResolveResourceString(rawLabel, languageName);

            if (!string.IsNullOrWhiteSpace(resolved))
            {
                e.FormComponent.BaseProperties.Label = resolved;
            }
        }

        /// <summary>
        /// Resolves a localization key name to the translation text for the current culture,
        /// falling back from a specific culture like "en-gb" to its base language "en".
        /// </summary>
        private string ResolveResourceString(string resourceKey, string languageName)
        {
            var requestServices = httpContextAccessor.HttpContext?.RequestServices;
            if (requestServices == null)
            {
                return null;
            }

            var keyProvider = requestServices.GetService<IInfoProvider<LocalizationKeyInfo>>();
            var translationProvider = requestServices.GetService<IInfoProvider<LocalizationTranslationItemInfo>>();
            var languageProvider = requestServices.GetService<IInfoProvider<ContentLanguageInfo>>();

            if (keyProvider == null || translationProvider == null || languageProvider == null)
            {
                return null;
            }

            var candidateCultures = new[] { languageName, languageName?.Split('-', 2)[0] }
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var key = keyProvider.Get()
                .WhereEquals(nameof(LocalizationKeyInfo.LocalizationKeyItemName), resourceKey)
                .TopN(1)
                .FirstOrDefault();

            if (key == null)
            {
                return null;
            }

            foreach (var culture in candidateCultures)
            {
                var language = languageProvider.Get()
                    .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageCultureFormat), culture)
                    .TopN(1)
                    .FirstOrDefault();

                if (language == null)
                {
                    continue;
                }

                var translation = translationProvider.Get()
                    .WhereEquals(nameof(LocalizationTranslationItemInfo.LocalizationTranslationItemLocalizationKeyItemId), key.LocalizationKeyItemId)
                    .WhereEquals(nameof(LocalizationTranslationItemInfo.LocalizationTranslationItemContentLanguageId), language.ContentLanguageID)
                    .TopN(1)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(translation?.LocalizationTranslationItemText))
                {
                    return translation.LocalizationTranslationItemText;
                }
            }

            return null;
        }
    }
}