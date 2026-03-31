using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(DateFormatInfo), DateFormatInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="DateFormatInfo"/>.
    /// </summary>
    public partial class DateFormatInfo : AbstractInfo<DateFormatInfo, IInfoProvider<DateFormatInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdositelocalization.dateformat";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<DateFormatInfo>), OBJECT_TYPE, "BDOSiteLocalization.DateFormat", "DateFormatID", "DateFormatLastModified", "DateFormatGuid", "DateFormatCodeName", "DateFormatCode", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Date format ID.
        /// </summary>
        [DatabaseField]
        public virtual int DateFormatID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DateFormatID)), 0);
            set => SetValue(nameof(DateFormatID), value);
        }


        /// <summary>
        /// Date format name.
        /// </summary>
        [DatabaseField]
        public virtual string DateFormatName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DateFormatName)), String.Empty);
            set => SetValue(nameof(DateFormatName), value);
        }


        /// <summary>
        /// Date format code name.
        /// </summary>
        [DatabaseField]
        public virtual string DateFormatCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DateFormatCodeName)), String.Empty);
            set => SetValue(nameof(DateFormatCodeName), value);
        }


        /// <summary>
        /// Date format code.
        /// </summary>
        [DatabaseField]
        public virtual string DateFormatCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(DateFormatCode)), String.Empty);
            set => SetValue(nameof(DateFormatCode), value);
        }


        /// <summary>
        /// Date format culture code.
        /// </summary>
        [DatabaseField]
        public virtual string DateFormatCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(DateFormatCultureCode)), String.Empty);
            set => SetValue(nameof(DateFormatCultureCode), value);
        }


        /// <summary>
        /// Date format type.
        /// </summary>
        [DatabaseField]
        public virtual string DateFormatType
        {
            get => ValidationHelper.GetString(GetValue(nameof(DateFormatType)), String.Empty);
            set => SetValue(nameof(DateFormatType), value, String.Empty);
        }


        /// <summary>
        /// Date format site ID.
        /// </summary>
        [DatabaseField]
        public virtual int DateFormatSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DateFormatSiteID)), 0);
            set => SetValue(nameof(DateFormatSiteID), value, 0);
        }


        /// <summary>
        /// Date format guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid DateFormatGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(DateFormatGuid)), Guid.Empty);
            set => SetValue(nameof(DateFormatGuid), value);
        }


        /// <summary>
        /// Date format last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DateFormatLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DateFormatLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DateFormatLastModified), value);
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
        /// Creates an empty instance of the <see cref="DateFormatInfo"/> class.
        /// </summary>
        public DateFormatInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="DateFormatInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DateFormatInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}