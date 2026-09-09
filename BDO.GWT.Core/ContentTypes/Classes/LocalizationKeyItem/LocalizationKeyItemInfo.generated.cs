using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(LocalizationKeyItemInfo), LocalizationKeyItemInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="LocalizationKeyItemInfo"/>.
    /// </summary>
    public partial class LocalizationKeyItemInfo : AbstractInfo<LocalizationKeyItemInfo, IInfoProvider<LocalizationKeyItemInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "nittinlocalization.localizationkeyitem";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<LocalizationKeyItemInfo>), OBJECT_TYPE, "NittinLocalization.LocalizationKeyItem", "LocalizationKeyItemId", null, "LocalizationKeyItemGuid", "LocalizationKeyItemName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Localization key item id.
        /// </summary>
        [DatabaseField]
        public virtual int LocalizationKeyItemId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LocalizationKeyItemId)), 0);
            set => SetValue(nameof(LocalizationKeyItemId), value);
        }


        /// <summary>
        /// Localization key item guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid LocalizationKeyItemGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(LocalizationKeyItemGuid)), Guid.Empty);
            set => SetValue(nameof(LocalizationKeyItemGuid), value);
        }


        /// <summary>
        /// Localization key item name.
        /// </summary>
        [DatabaseField]
        public virtual string LocalizationKeyItemName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LocalizationKeyItemName)), String.Empty);
            set => SetValue(nameof(LocalizationKeyItemName), value);
        }


        /// <summary>
        /// Localization key item description.
        /// </summary>
        [DatabaseField]
        public virtual string LocalizationKeyItemDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(LocalizationKeyItemDescription)), String.Empty);
            set => SetValue(nameof(LocalizationKeyItemDescription), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="LocalizationKeyItemInfo"/> class.
        /// </summary>
        public LocalizationKeyItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="LocalizationKeyItemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public LocalizationKeyItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}