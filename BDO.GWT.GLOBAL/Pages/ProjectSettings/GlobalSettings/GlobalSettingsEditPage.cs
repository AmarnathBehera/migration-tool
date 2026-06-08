using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

[UIEvaluatePermission("Administrator")]
public class GlobalSettingsEditPage : InfoEditPage<GlobalsettingskeyInfo>
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;

    public override int ObjectId
    {
        get => _provider.Get()
                .TopN(1)
                .GetEnumerableTypedResult()
                .FirstOrDefault()
                ?.GlobalsettingskeyID ?? 0;
        set { _ = value; }
    }

    public GlobalSettingsEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GlobalsettingskeyInfo> provider)
        : base(formComponentMapper, formDataBinder)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public override async Task ConfigurePage()
    {
        // Auto-create the record if it doesn't exist yet
        var existing = _provider.Get()
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        if (existing == null)
        {
            var newRecord = new GlobalsettingskeyInfo();
            _provider.Set(newRecord);
        }

        PageConfiguration.UIFormName = "globalsettingskeyedit"; // ← your form name
        PageConfiguration.Headline = "Global Settings";

        await base.ConfigurePage();
    }

    protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
        GlobalsettingskeyInfo savedInfoObject,
        ICollection<IFormItem> items)
    {
        var result = new EditPageSuccessFormSubmissionResult()
        {
            Items = await items.OnlyVisible().GetClientProperties(),
            ObjectDisplayName = "Global Settings",
            ObjectId = savedInfoObject.GlobalsettingskeyID,
            RefetchAll = RefetchAll
        };

        return ResponseFrom(result)
            .AddSuccessMessage(LocalizationService?.GetString("base.forms.saved"));
    }
}