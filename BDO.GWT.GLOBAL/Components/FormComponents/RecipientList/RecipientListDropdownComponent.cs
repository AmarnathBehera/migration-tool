using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.EmailMarketing;
using Kentico.Forms.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: RegisterFormComponent(
    BDO.GWT.GLOBAL.Components.FormComponents.RecipientList.RecipientListDropdownComponent.IDENTIFIER,
    typeof(BDO.GWT.GLOBAL.Components.FormComponents.RecipientList.RecipientListDropdownComponent),
    "Recipient lists (dropdown)",
    Description = "Displays all recipient lists as a dropdown for subscription forms.",
    IconClass = "icon-l-list",
    ViewName = "~/Components/FormComponents/RecipientList/RecipientListDropdownComponent.cshtml",
    IsAvailableInFormBuilderEditor = true)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.RecipientList
{
    public class RecipientListDropdownComponent : FormComponent<RecipientListDropdownProperties, string>
    {
        public const string IDENTIFIER = "BDO.GWT.RecipientListDropdown";

        private readonly IInfoProvider<ContactGroupInfo> recipientListInfoProvider;

        public RecipientListDropdownComponent(IInfoProvider<ContactGroupInfo> recipientListInfoProvider)
        {
            this.recipientListInfoProvider = recipientListInfoProvider;
        }

        [BindableProperty]
        public string Value { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Options => recipientListInfoProvider.Get()
            .Columns(nameof(ContactGroupInfo.ContactGroupName), nameof(ContactGroupInfo.ContactGroupDisplayName))
            .OrderBy(nameof(ContactGroupInfo.ContactGroupDisplayName))
            .GetEnumerableTypedResult()
            .Where(list => !string.IsNullOrWhiteSpace(list.ContactGroupName))
            .Select(list => new SelectListItem
            {
                Value = list.ContactGroupName,
                Text = string.IsNullOrWhiteSpace(list.ContactGroupDisplayName)
                    ? list.ContactGroupName
                    : list.ContactGroupDisplayName
            });

        public override string GetValue() => Value ?? string.Empty;

        public override void SetValue(string value)
        {
            Value = string.IsNullOrWhiteSpace(value)
                ? (Properties.DefaultValue ?? string.Empty)
                : value;
        }
    }
}