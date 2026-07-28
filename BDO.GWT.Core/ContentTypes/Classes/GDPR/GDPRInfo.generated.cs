using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GDPRInfo), GDPRInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GDPRInfo"/>.
    /// </summary>
    public partial class GDPRInfo : AbstractInfo<GDPRInfo, IInfoProvider<GDPRInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.gdpr";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GDPRInfo>), OBJECT_TYPE, "BDO.GDPR", "GDPRID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// GDPRID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPRID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPRID)), 0);
            set => SetValue(nameof(GDPRID), value);
        }


        /// <summary>
        /// GDPR web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual string GDPRWebChannelSettingID
        {
            get => ValidationHelper.GetString(GetValue(nameof(GDPRWebChannelSettingID)), String.Empty);
            set => SetValue(nameof(GDPRWebChannelSettingID), value);
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
        /// Results.
        /// </summary>
        [DatabaseField]
        public virtual string Results
        {
            get => ValidationHelper.GetString(GetValue(nameof(Results)), String.Empty);
            set => SetValue(nameof(Results), value, String.Empty);
        }


        /// <summary>
        /// Consent.
        /// </summary>
        [DatabaseField]
        public virtual string Consent
        {
            get => ValidationHelper.GetString(GetValue(nameof(Consent)), String.Empty);
            set => SetValue(nameof(Consent), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="GDPRInfo"/> class.
        /// </summary>
        public GDPRInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GDPRInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GDPRInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}