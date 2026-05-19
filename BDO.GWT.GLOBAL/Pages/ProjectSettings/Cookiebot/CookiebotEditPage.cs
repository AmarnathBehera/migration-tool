using BDO.GWT.Core.DTOs.Cookieboot;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
[UIEvaluatePermission("Administrator")]
 public class CookiebootEditPage : ModelEditPage<CookiebotDTO>
    {
        private readonly IInfoProvider<CookiebotInfo> provider;
        private CookiebotDTO model;

        public CookiebootEditPage(
            IFormItemCollectionProvider formItemCollectionProvider,
            IFormDataBinder formDataBinder,
            IInfoProvider<CookiebotInfo> provider)
            : base(formItemCollectionProvider, formDataBinder)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected override CookiebotDTO Model
        {
            get
            {
                if (model != null)
                {
                    return model;
                }

                try
                {
                    var data = provider?.Get()?.FirstOrDefault();

                    if (data != null)
                    {
                        model = new CookiebotDTO
                        {
                            CookieID = data.CookieID ?? string.Empty,
                            EnableCookiebot = data.EnableCookiebot,
                            CustomCookiebotScript = data.CustomCookiebotScript ?? string.Empty
                        };
                    }
                    else
                    {
                        model = new CookiebotDTO();
                    }
                }
                catch (Exception)
                {
                    // Optional: log error here
                    model = new CookiebotDTO();
                }

                return model;
            }
        }

        public override Task ConfigurePage()
        {
            return base.ConfigurePage();
        }

        // ✅ SAVE DATA
        protected override async Task<ICommandResponse> ProcessFormData(
            CookiebotDTO model,
            ICollection<IFormItem> formItems)
        {
            bool success = false;

            try
            {
                var data = provider?.Get()?.FirstOrDefault();

                if (data == null)
                {
                    data = new CookiebotInfo();
                }

                // ✅ Null-safe mapping
                data.CookieID = model?.CookieID ?? string.Empty;
                data.EnableCookiebot = model?.EnableCookiebot ?? false;
                data.CustomCookiebotScript = model?.CustomCookiebotScript ?? string.Empty;

                provider.Set(data);

                success = true;
            }
            catch (Exception)
            {
                success = false;
                // 👉 You can log exception here if needed
            }

            // ✅ Response handling
            var response = ResponseFrom(new FormSubmissionResult(
                success ? FormSubmissionStatus.ValidationSuccess : FormSubmissionStatus.ValidationFailure)
            {
                Items = await formItems.OnlyVisible().GetClientProperties(),
            });

            if (success)
            {
                response.AddSuccessMessage("Cookiebot settings saved successfully.");
            }
            else
            {
                response.AddErrorMessage("Something went wrong while saving settings.");
            }

            return response;
        }
    }
