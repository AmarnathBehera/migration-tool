using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ReCAPTCHAv2Info), ReCAPTCHAv2Info.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ReCAPTCHAv2Info"/>.
    /// </summary>
    public partial class ReCAPTCHAv2Info : AbstractInfo<ReCAPTCHAv2Info, IInfoProvider<ReCAPTCHAv2Info>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.recaptchav2";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ReCAPTCHAv2Info>), OBJECT_TYPE, "BDO.reCAPTCHAv2", "ReCAPTCHAv2ID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Re CAPTCH av 2 ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReCAPTCHAv2ID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReCAPTCHAv2ID)), 0);
            set => SetValue(nameof(ReCAPTCHAv2ID), value);
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
        /// Creates an empty instance of the <see cref="ReCAPTCHAv2Info"/> class.
        /// </summary>
        public ReCAPTCHAv2Info()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ReCAPTCHAv2Info"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReCAPTCHAv2Info(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}