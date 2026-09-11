using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(HeaderInfo), HeaderInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="HeaderInfo"/>.
    /// </summary>
    public partial class HeaderInfo : AbstractInfo<HeaderInfo, IInfoProvider<HeaderInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.header";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<HeaderInfo>), OBJECT_TYPE, "BDO.Header", "HeaderID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Header ID.
        /// </summary>
        [DatabaseField]
        public virtual int HeaderID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(HeaderID)), 0);
            set => SetValue(nameof(HeaderID), value);
        }


        /// <summary>
        /// Site logo URL.
        /// </summary>
        [DatabaseField]
        public virtual string SiteLogoURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteLogoURL)), String.Empty);
            set => SetValue(nameof(SiteLogoURL), value);
        }


        /// <summary>
        /// Site display namesby culture.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDisplayNamesbyCulture
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteDisplayNamesbyCulture)), String.Empty);
            set => SetValue(nameof(SiteDisplayNamesbyCulture), value);
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
        /// Creates an empty instance of the <see cref="HeaderInfo"/> class.
        /// </summary>
        public HeaderInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="HeaderInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public HeaderInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}