using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(UpgradeLogInfo), UpgradeLogInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="UpgradeLogInfo"/>.
    /// </summary>
    public partial class UpgradeLogInfo : AbstractInfo<UpgradeLogInfo, IInfoProvider<UpgradeLogInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdorelease.upgradelog";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<UpgradeLogInfo>), OBJECT_TYPE, "BDORelease.UpgradeLog", "UpgradeLogID", "UpgradeLogLastModified", "UpgradeLogGuid", "Name", "Name", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Upgrade log ID.
        /// </summary>
        [DatabaseField]
        public virtual int UpgradeLogID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(UpgradeLogID)), 0);
            set => SetValue(nameof(UpgradeLogID), value);
        }


        /// <summary>
        /// Upgrade log guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid UpgradeLogGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(UpgradeLogGuid)), Guid.Empty);
            set => SetValue(nameof(UpgradeLogGuid), value);
        }


        /// <summary>
        /// Upgrade log last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UpgradeLogLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(UpgradeLogLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(UpgradeLogLastModified), value);
        }


        /// <summary>
        /// Type.
        /// </summary>
        [DatabaseField]
        public virtual string Type
        {
            get => ValidationHelper.GetString(GetValue(nameof(Type)), String.Empty);
            set => SetValue(nameof(Type), value);
        }


        /// <summary>
        /// Version.
        /// </summary>
        [DatabaseField]
        public virtual string Version
        {
            get => ValidationHelper.GetString(GetValue(nameof(Version)), String.Empty);
            set => SetValue(nameof(Version), value);
        }


        /// <summary>
        /// Name.
        /// </summary>
        [DatabaseField]
        public virtual string Name
        {
            get => ValidationHelper.GetString(GetValue(nameof(Name)), String.Empty);
            set => SetValue(nameof(Name), value);
        }


        /// <summary>
        /// Date time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DateTime
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DateTime)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DateTime), value);
        }


        /// <summary>
        /// State.
        /// </summary>
        [DatabaseField]
        public virtual string State
        {
            get => ValidationHelper.GetString(GetValue(nameof(State)), String.Empty);
            set => SetValue(nameof(State), value, String.Empty);
        }


        /// <summary>
        /// Message.
        /// </summary>
        [DatabaseField]
        public virtual string Message
        {
            get => ValidationHelper.GetString(GetValue(nameof(Message)), String.Empty);
            set => SetValue(nameof(Message), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="UpgradeLogInfo"/> class.
        /// </summary>
        public UpgradeLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="UpgradeLogInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public UpgradeLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}