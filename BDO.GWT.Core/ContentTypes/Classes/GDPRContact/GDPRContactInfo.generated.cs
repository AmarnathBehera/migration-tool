using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GDPRContactInfo), GDPRContactInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GDPRContactInfo"/>.
    /// </summary>
    public partial class GDPRContactInfo : AbstractInfo<GDPRContactInfo, IInfoProvider<GDPRContactInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.gdpr_contact";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GDPRContactInfo>), OBJECT_TYPE, "BDO.GDPRContact", "GDPRContactID", "GDPRContactLastModified", "GDPRContactGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// GDPR contact ID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPRContactID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPRContactID)), 0);
            set => SetValue(nameof(GDPRContactID), value);
        }


        /// <summary>
        /// GDPR contact guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GDPRContactGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GDPRContactGuid)), Guid.Empty);
            set => SetValue(nameof(GDPRContactGuid), value);
        }


        /// <summary>
        /// GDPR contact last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GDPRContactLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GDPRContactLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GDPRContactLastModified), value);
        }


        /// <summary>
        /// Email address.
        /// </summary>
        [DatabaseField]
        public virtual string EmailAddress
        {
            get => ValidationHelper.GetString(GetValue(nameof(EmailAddress)), String.Empty);
            set => SetValue(nameof(EmailAddress), value, String.Empty);
        }


        /// <summary>
        /// Last modified by.
        /// </summary>
        [DatabaseField]
        public virtual string LastModifiedBy
        {
            get => ValidationHelper.GetString(GetValue(nameof(LastModifiedBy)), String.Empty);
            set => SetValue(nameof(LastModifiedBy), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="GDPRContactInfo"/> class.
        /// </summary>
        public GDPRContactInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GDPRContactInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GDPRContactInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}