using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GoogleServicesInfo), GoogleServicesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GoogleServicesInfo"/>.
    /// </summary>
    public partial class GoogleServicesInfo : AbstractInfo<GoogleServicesInfo, IInfoProvider<GoogleServicesInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.googleservices";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GoogleServicesInfo>), OBJECT_TYPE, "BDO.GoogleServices", "GoogleServicesID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Google services ID.
        /// </summary>
        [DatabaseField]
        public virtual int GoogleServicesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GoogleServicesID)), 0);
            set => SetValue(nameof(GoogleServicesID), value);
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
        /// Google tag manager ID.
        /// </summary>
        [DatabaseField]
        public virtual string GoogleTagManagerID
        {
            get => ValidationHelper.GetString(GetValue(nameof(GoogleTagManagerID)), String.Empty);
            set => SetValue(nameof(GoogleTagManagerID), value);
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
        /// Creates an empty instance of the <see cref="GoogleServicesInfo"/> class.
        /// </summary>
        public GoogleServicesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GoogleServicesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GoogleServicesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}