using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GoogleInfo), GoogleInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GoogleInfo"/>.
    /// </summary>
    public partial class GoogleInfo : AbstractInfo<GoogleInfo, IInfoProvider<GoogleInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.google";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GoogleInfo>), OBJECT_TYPE, "BDO.Google", "GoogleID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Google ID.
        /// </summary>
        [DatabaseField]
        public virtual int GoogleID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GoogleID)), 0);
            set => SetValue(nameof(GoogleID), value);
        }


        /// <summary>
        /// Google analytics tracking ID.
        /// </summary>
        [DatabaseField]
        public virtual string GoogleAnalyticsTrackingID
        {
            get => ValidationHelper.GetString(GetValue(nameof(GoogleAnalyticsTrackingID)), String.Empty);
            set => SetValue(nameof(GoogleAnalyticsTrackingID), value);
        }


        /// <summary>
        /// Enableglobal googleanalyticsforsite.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableglobalGoogleanalyticsforsite
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableglobalGoogleanalyticsforsite)), false);
            set => SetValue(nameof(EnableglobalGoogleanalyticsforsite), value);
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
        /// Creates an empty instance of the <see cref="GoogleInfo"/> class.
        /// </summary>
        public GoogleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GoogleInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GoogleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}