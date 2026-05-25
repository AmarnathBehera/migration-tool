using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FootersettingsInfo), FootersettingsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FootersettingsInfo"/>.
    /// </summary>
    public partial class FootersettingsInfo : AbstractInfo<FootersettingsInfo, IInfoProvider<FootersettingsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.footersettings";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FootersettingsInfo>), OBJECT_TYPE, "BDO.Footersettings", "FootersettingsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Footersettings ID.
        /// </summary>
        [DatabaseField]
        public virtual int FootersettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FootersettingsID)), 0);
            set => SetValue(nameof(FootersettingsID), value);
        }


        /// <summary>
        /// Display subscriptionssection.
        /// </summary>
        [DatabaseField]
        public virtual bool DisplaySubscriptionssection
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisplaySubscriptionssection)), false);
            set => SetValue(nameof(DisplaySubscriptionssection), value);
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
        /// Creates an empty instance of the <see cref="FootersettingsInfo"/> class.
        /// </summary>
        public FootersettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FootersettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FootersettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}