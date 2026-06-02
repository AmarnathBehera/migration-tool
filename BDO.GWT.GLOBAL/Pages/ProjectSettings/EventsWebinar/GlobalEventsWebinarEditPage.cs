using BDO.GWT.Core.DTOs;
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
public class GlobalEventsWebinarEditPage : ModelEditPage<GlobalEventsWebinarDto>
{
    private readonly IInfoProvider<GlobalEventsnWebinarsInfo> provider;
    private GlobalEventsWebinarDto model;

    public GlobalEventsWebinarEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<GlobalEventsnWebinarsInfo> provider)
        : base(formItemCollectionProvider, formDataBinder)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    protected override GlobalEventsWebinarDto Model
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
                    model = new GlobalEventsWebinarDto
                    {
                        Displaylocationtitleinasidebar = data.Displaylocationtitleinasidebar,
                        Minutestotriggercalendarreminder = data.Minutestotriggercalendarreminder,
                    };
                }
                else
                {
                    model = new GlobalEventsWebinarDto();
                }
            }
            catch (Exception)
            {
                model = new GlobalEventsWebinarDto();
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
        GlobalEventsWebinarDto model,
        ICollection<IFormItem> formItems)
    {
        bool success = false;

        try
        {
            var data = provider?.Get()?.FirstOrDefault();

            if (data == null)
            {
                data = new GlobalEventsnWebinarsInfo();
            }

            // ✅ Null-safe mapping
            data.Displaylocationtitleinasidebar = model?.Displaylocationtitleinasidebar ?? false;
            data.Minutestotriggercalendarreminder = model?.Minutestotriggercalendarreminder ?? 0;
            provider!.Set(data);

            success = true;
        }
        catch (Exception)
        {
            success = false;
        }

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
