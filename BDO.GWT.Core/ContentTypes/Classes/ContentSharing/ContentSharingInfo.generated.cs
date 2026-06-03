using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ContentSharingInfo), ContentSharingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ContentSharingInfo"/>.
    /// </summary>
    public partial class ContentSharingInfo : AbstractInfo<ContentSharingInfo, IInfoProvider<ContentSharingInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.contentsharing";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ContentSharingInfo>), OBJECT_TYPE, "BDO.ContentSharing", "ContentSharingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Content sharing ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentSharingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentSharingID)), 0);
            set => SetValue(nameof(ContentSharingID), value);
        }


        /// <summary>
        /// Memberfirm ID.
        /// </summary>
        [DatabaseField]
        public virtual string MemberfirmID
        {
            get => ValidationHelper.GetString(GetValue(nameof(MemberfirmID)), String.Empty);
            set => SetValue(nameof(MemberfirmID), value);
        }


        /// <summary>
        /// Last content sharing mail.
        /// </summary>
        [DatabaseField]
        public virtual string LastContentSharingMail
        {
            get => ValidationHelper.GetString(GetValue(nameof(LastContentSharingMail)), String.Empty);
            set => SetValue(nameof(LastContentSharingMail), value);
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
        /// Creates an empty instance of the <see cref="ContentSharingInfo"/> class.
        /// </summary>
        public ContentSharingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ContentSharingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentSharingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}