using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SocialSharingInfo), SocialSharingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SocialSharingInfo"/>.
    /// </summary>
    public partial class SocialSharingInfo : AbstractInfo<SocialSharingInfo, IInfoProvider<SocialSharingInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.socialsharing";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SocialSharingInfo>), OBJECT_TYPE, "BDO.SocialSharing", "SocialSharingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Social sharing ID.
        /// </summary>
        [DatabaseField]
        public virtual int SocialSharingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SocialSharingID)), 0);
            set => SetValue(nameof(SocialSharingID), value);
        }


        /// <summary>
        /// Twitter handleofthe twittershare.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterHandleoftheTwittershare
        {
            get => ValidationHelper.GetString(GetValue(nameof(TwitterHandleoftheTwittershare)), String.Empty);
            set => SetValue(nameof(TwitterHandleoftheTwittershare), value);
        }


        /// <summary>
        /// Facebook app ID.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookAppID
        {
            get => ValidationHelper.GetString(GetValue(nameof(FacebookAppID)), String.Empty);
            set => SetValue(nameof(FacebookAppID), value);
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
        /// Creates an empty instance of the <see cref="SocialSharingInfo"/> class.
        /// </summary>
        public SocialSharingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SocialSharingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SocialSharingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}