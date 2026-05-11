using CMS.DataEngine;
using CMS.DataProtection;
using Kentico.Forms.Web.Mvc;
using System;
using System.Linq;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ConsentAgreement : FormComponent<ConsentAgreementProperties, string?>
{
    public new const string IDENTIFIER = "BDO.FormComponent.ConsentAgreement";

    private readonly IInfoProvider<ConsentInfo> consentInfoProvider;

    // ✅ Use dependency injection instead of static providers
    public ConsentAgreement(IInfoProvider<ConsentInfo> consentInfoProvider)
    {
        this.consentInfoProvider = consentInfoProvider;
    }

    [BindableProperty]
    public bool Value { get; set; }

    public string Label => GetConsentText() ?? "";

    public override string GetValue() => Value ? "True" : null;

    public override void SetValue(string value) =>
        Value = "True".Equals(value, StringComparison.OrdinalIgnoreCase);

    private string GetConsentText()
    {
        return consentInfoProvider
            .Get()
            .TopN(1)
            .FirstOrDefault()
            ?.ConsentDisplayName;
    }
}