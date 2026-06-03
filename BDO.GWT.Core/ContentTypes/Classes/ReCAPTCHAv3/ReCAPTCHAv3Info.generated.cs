using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ReCAPTCHAv3Info), ReCAPTCHAv3Info.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ReCAPTCHAv3Info"/>.
    /// </summary>
    public partial class ReCAPTCHAv3Info : AbstractInfo<ReCAPTCHAv3Info, IInfoProvider<ReCAPTCHAv3Info>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.recaptchav3";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ReCAPTCHAv3Info>), OBJECT_TYPE, "BDO.reCAPTCHAv3", "ReCAPTCHAv3ID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Re CAPTCH av 3 ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReCAPTCHAv3ID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReCAPTCHAv3ID)), 0);
            set => SetValue(nameof(ReCAPTCHAv3ID), value);
        }


        /// <summary>
        /// Sitekey.
        /// </summary>
        [DatabaseField]
        public virtual string Sitekey
        {
            get => ValidationHelper.GetString(GetValue(nameof(Sitekey)), String.Empty);
            set => SetValue(nameof(Sitekey), value);
        }


        /// <summary>
        /// Secretkey.
        /// </summary>
        [DatabaseField]
        public virtual string Secretkey
        {
            get => ValidationHelper.GetString(GetValue(nameof(Secretkey)), String.Empty);
            set => SetValue(nameof(Secretkey), value);
        }


        /// <summary>
        /// Scorethreshold.
        /// </summary>
        [DatabaseField]
        public virtual string Scorethreshold
        {
            get => ValidationHelper.GetString(GetValue(nameof(Scorethreshold)), String.Empty);
            set => SetValue(nameof(Scorethreshold), value);
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
        /// Creates an empty instance of the <see cref="ReCAPTCHAv3Info"/> class.
        /// </summary>
        public ReCAPTCHAv3Info()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ReCAPTCHAv3Info"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReCAPTCHAv3Info(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}