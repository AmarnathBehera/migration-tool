using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CookiebotInfo), CookiebotInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CookiebotInfo"/>.
    /// </summary>
    public partial class CookiebotInfo : AbstractInfo<CookiebotInfo, IInfoProvider<CookiebotInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.cookiebot";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CookiebotInfo>), OBJECT_TYPE, "BDO.Cookiebot", "CookiebotID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Cookiebot ID.
        /// </summary>
        [DatabaseField]
        public virtual int CookiebotID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CookiebotID)), 0);
            set => SetValue(nameof(CookiebotID), value);
        }


        /// <summary>
        /// Enable cookiebot.
        /// </summary>
        [DatabaseField]
        public virtual bool EnableCookiebot
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(EnableCookiebot)), false);
            set => SetValue(nameof(EnableCookiebot), value);
        }


        /// <summary>
        /// Cookie ID.
        /// </summary>
        [DatabaseField]
        public virtual string CookieID
        {
            get => ValidationHelper.GetString(GetValue(nameof(CookieID)), String.Empty);
            set => SetValue(nameof(CookieID), value);
        }


        /// <summary>
        /// Custom cookiebot script.
        /// </summary>
        [DatabaseField]
        public virtual string CustomCookiebotScript
        {
            get => ValidationHelper.GetString(GetValue(nameof(CustomCookiebotScript)), String.Empty);
            set => SetValue(nameof(CustomCookiebotScript), value);
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
        /// Creates an empty instance of the <see cref="CookiebotInfo"/> class.
        /// </summary>
        public CookiebotInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CookiebotInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CookiebotInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}