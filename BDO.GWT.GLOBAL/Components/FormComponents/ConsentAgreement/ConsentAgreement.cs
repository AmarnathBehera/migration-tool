using CMS.DataProtection;
using Kentico.Forms.Web.Mvc;
using Kentico.OnlineMarketing.Web.Mvc;
using System;
using System.Linq;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ConsentAgreement : FormComponent<ConsentAgreementProperties, string?>
{
    public const string IDENTIFIER = "BDO.FormComponent.ConsentAgreement";

    private readonly IConsentInfoProvider consentInfoProvider;

    // ✅ Use dependency injection instead of static providers
    public ConsentAgreement(IConsentInfoProvider consentInfoProvider)
    {
        this.consentInfoProvider = consentInfoProvider;
    }

    [BindableProperty]
    public bool Value { get; set; }

    public string Label => GetConsentText() ?? "";

    public override string? GetValue() => Value == false ? null : "True";

    public override void SetValue(string? value) =>
        Value = value != null && value.Equals("True", StringComparison.OrdinalIgnoreCase);

    private string? GetConsentText()
    {
        // ✅ XbyK: consents are global (no SiteID), get by code name configured in properties
        return consentInfoProvider
            .Get()
            .TopN(1)
            .FirstOrDefault()
            ?.ConsentDisplayName;
    }
}