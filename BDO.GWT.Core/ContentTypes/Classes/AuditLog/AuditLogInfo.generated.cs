using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(AuditLogInfo), AuditLogInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="AuditLogInfo"/>.
    /// </summary>
    public partial class AuditLogInfo : AbstractInfo<AuditLogInfo, IInfoProvider<AuditLogInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdoauditlog.auditlog";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<AuditLogInfo>), OBJECT_TYPE, "BDOAuditLog.AuditLog", "AuditLogID", null, "AuditLogGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("UserId", "cms.user", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Audit log ID.
        /// </summary>
        [DatabaseField]
        public virtual int AuditLogID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(AuditLogID)), 0);
            set => SetValue(nameof(AuditLogID), value);
        }


        /// <summary>
        /// Audit log guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid AuditLogGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(AuditLogGuid)), Guid.Empty);
            set => SetValue(nameof(AuditLogGuid), value);
        }


        /// <summary>
        /// Time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime Time
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(Time)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(Time), value);
        }


        /// <summary>
        /// Username.
        /// </summary>
        [DatabaseField]
        public virtual string Username
        {
            get => ValidationHelper.GetString(GetValue(nameof(Username)), String.Empty);
            set => SetValue(nameof(Username), value);
        }


        /// <summary>
        /// User id.
        /// </summary>
        [DatabaseField]
        public virtual int UserId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(UserId)), 0);
            set => SetValue(nameof(UserId), value, 0);
        }


        /// <summary>
        /// IP address.
        /// </summary>
        [DatabaseField]
        public virtual string IPAddress
        {
            get => ValidationHelper.GetString(GetValue(nameof(IPAddress)), String.Empty);
            set => SetValue(nameof(IPAddress), value);
        }


        /// <summary>
        /// Source.
        /// </summary>
        [DatabaseField]
        public virtual string Source
        {
            get => ValidationHelper.GetString(GetValue(nameof(Source)), String.Empty);
            set => SetValue(nameof(Source), value);
        }


        /// <summary>
        /// Action.
        /// </summary>
        [DatabaseField]
        public virtual string Action
        {
            get => ValidationHelper.GetString(GetValue(nameof(Action)), String.Empty);
            set => SetValue(nameof(Action), value);
        }


        /// <summary>
        /// Object identifier.
        /// </summary>
        [DatabaseField]
        public virtual string ObjectIdentifier
        {
            get => ValidationHelper.GetString(GetValue(nameof(ObjectIdentifier)), String.Empty);
            set => SetValue(nameof(ObjectIdentifier), value, String.Empty);
        }


        /// <summary>
        /// Description.
        /// </summary>
        [DatabaseField]
        public virtual string Description
        {
            get => ValidationHelper.GetString(GetValue(nameof(Description)), String.Empty);
            set => SetValue(nameof(Description), value);
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
        /// Creates an empty instance of the <see cref="AuditLogInfo"/> class.
        /// </summary>
        public AuditLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AuditLogInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AuditLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}