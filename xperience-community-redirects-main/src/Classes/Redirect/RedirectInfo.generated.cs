using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.Redirects;

[assembly: RegisterObjectType(typeof(RedirectInfo), RedirectInfo.OBJECT_TYPE)]

namespace XperienceCommunity.Redirects
{
    /// <summary>
    /// Data container class for <see cref="RedirectInfo"/>.
    /// </summary>
    [Serializable]
    public partial class RedirectInfo : AbstractInfo<RedirectInfo, IInfoProvider<RedirectInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.redirect";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RedirectInfo>), OBJECT_TYPE, "XperienceCommunity.Redirect", "RedirectID", null, "RedirectGUID", null, "RedirectSourceUrl", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Redirect ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectID)), 0);
            set => SetValue(nameof(RedirectID), value);
        }


        /// <summary>
        /// Redirect GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid RedirectGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RedirectGUID)), Guid.Empty);
            set => SetValue(nameof(RedirectGUID), value);
        }


        /// <summary>
        /// Redirect source url.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectSourceUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectSourceUrl)), String.Empty);
            set => SetValue(nameof(RedirectSourceUrl), value);
        }


        /// <summary>
        /// Redirect target web page item GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid RedirectTargetWebPageItemGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RedirectTargetWebPageItemGUID)), Guid.Empty);
            set => SetValue(nameof(RedirectTargetWebPageItemGUID), value);
        }

        /// <summary>
        /// Redirect channel name.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectChannelName
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectChannelName)), String.Empty);
            set => SetValue(nameof(RedirectChannelName), value);
        }

        /// <summary>
        /// Redirect channel ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectChannelID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectChannelID)), 0);
            set => SetValue(nameof(RedirectChannelID), value);
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
        /// Creates an empty instance of the <see cref="RedirectInfo"/> class.
        /// </summary>
        public RedirectInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RedirectInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RedirectInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}