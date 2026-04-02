using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalLocationInfo), GlobalLocationInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalLocationInfo"/>.
    /// </summary>
    public partial class GlobalLocationInfo : AbstractInfo<GlobalLocationInfo, IInfoProvider<GlobalLocationInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.globallocation";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalLocationInfo>), OBJECT_TYPE, "BDO.GlobalLocation", "ItemID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Item ID.
        /// </summary>
        [DatabaseField]
        public virtual int ItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ItemID)), 0);
            set => SetValue(nameof(ItemID), value);
        }


        /// <summary>
        /// Metadata location pin colour.
        /// </summary>
        [DatabaseField]
        public virtual string MetadataLocationPinColour
        {
            get => ValidationHelper.GetString(GetValue(nameof(MetadataLocationPinColour)), String.Empty);
            set => SetValue(nameof(MetadataLocationPinColour), value);
        }


        /// <summary>
        /// Global location title.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationTitle
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationTitle)), String.Empty);
            set => SetValue(nameof(GlobalLocationTitle), value, String.Empty);
        }


        /// <summary>
        /// Global location address 1.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationAddress1
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationAddress1)), String.Empty);
            set => SetValue(nameof(GlobalLocationAddress1), value, String.Empty);
        }


        /// <summary>
        /// Global location address 2.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationAddress2
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationAddress2)), String.Empty);
            set => SetValue(nameof(GlobalLocationAddress2), value, String.Empty);
        }


        /// <summary>
        /// Global location city.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationCity
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationCity)), String.Empty);
            set => SetValue(nameof(GlobalLocationCity), value, String.Empty);
        }


        /// <summary>
        /// Global location state.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationState
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationState)), String.Empty);
            set => SetValue(nameof(GlobalLocationState), value, String.Empty);
        }


        /// <summary>
        /// Global location country.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationCountry
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationCountry)), String.Empty);
            set => SetValue(nameof(GlobalLocationCountry), value, String.Empty);
        }


        /// <summary>
        /// Global location postal code.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationPostalCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationPostalCode)), String.Empty);
            set => SetValue(nameof(GlobalLocationPostalCode), value, String.Empty);
        }


        /// <summary>
        /// Global location region.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationRegion
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationRegion)), String.Empty);
            set => SetValue(nameof(GlobalLocationRegion), value, String.Empty);
        }


        /// <summary>
        /// Global location latitude.
        /// </summary>
        [DatabaseField]
        public virtual decimal GlobalLocationLatitude
        {
            get => ValidationHelper.GetDecimal(GetValue(nameof(GlobalLocationLatitude)), 0m);
            set => SetValue(nameof(GlobalLocationLatitude), value, 0m);
        }


        /// <summary>
        /// Global location longitude.
        /// </summary>
        [DatabaseField]
        public virtual decimal GlobalLocationLongitude
        {
            get => ValidationHelper.GetDecimal(GetValue(nameof(GlobalLocationLongitude)), 0m);
            set => SetValue(nameof(GlobalLocationLongitude), value, 0m);
        }


        /// <summary>
        /// Global location url.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationUrl)), String.Empty);
            set => SetValue(nameof(GlobalLocationUrl), value, String.Empty);
        }


        /// <summary>
        /// Member firm ID.
        /// </summary>
        [DatabaseField]
        public virtual int MemberFirmID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(MemberFirmID)), 0);
            set => SetValue(nameof(MemberFirmID), value, 0);
        }


        /// <summary>
        /// Item created by.
        /// </summary>
        [DatabaseField]
        public virtual int ItemCreatedBy
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ItemCreatedBy)), 0);
            set => SetValue(nameof(ItemCreatedBy), value, 0);
        }


        /// <summary>
        /// Item created when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ItemCreatedWhen
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ItemCreatedWhen)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ItemCreatedWhen), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Item modified by.
        /// </summary>
        [DatabaseField]
        public virtual int ItemModifiedBy
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ItemModifiedBy)), 0);
            set => SetValue(nameof(ItemModifiedBy), value, 0);
        }


        /// <summary>
        /// Item modified when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ItemModifiedWhen
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ItemModifiedWhen)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ItemModifiedWhen), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Item order.
        /// </summary>
        [DatabaseField]
        public virtual int ItemOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ItemOrder)), 0);
            set => SetValue(nameof(ItemOrder), value, 0);
        }


        /// <summary>
        /// Item GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ItemGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ItemGUID)), Guid.Empty);
            set => SetValue(nameof(ItemGUID), value);
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
        /// Creates an empty instance of the <see cref="GlobalLocationInfo"/> class.
        /// </summary>
        public GlobalLocationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalLocationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalLocationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}