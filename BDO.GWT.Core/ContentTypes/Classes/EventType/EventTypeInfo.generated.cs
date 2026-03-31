using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(EventTypeInfo), EventTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="EventTypeInfo"/>.
    /// </summary>
    public partial class EventTypeInfo : AbstractInfo<EventTypeInfo, IInfoProvider<EventTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.eventtype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<EventTypeInfo>), OBJECT_TYPE, "BDOMetadata.EventType", "EventTypeID", "EventTypeLastModified", "EventTypeGuid", "EventTypeCodeName", "EventTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("EventTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Event type ID.
        /// </summary>
        [DatabaseField]
        public virtual int EventTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EventTypeID)), 0);
            set => SetValue(nameof(EventTypeID), value);
        }


        /// <summary>
        /// Event type name.
        /// </summary>
        [DatabaseField]
        public virtual string EventTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(EventTypeName)), String.Empty);
            set => SetValue(nameof(EventTypeName), value);
        }


        /// <summary>
        /// Event type code name.
        /// </summary>
        [DatabaseField]
        public virtual string EventTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(EventTypeCodeName)), String.Empty);
            set => SetValue(nameof(EventTypeCodeName), value);
        }


        /// <summary>
        /// Event type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string EventTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(EventTypeCultureCode)), String.Empty);
            set => SetValue(nameof(EventTypeCultureCode), value);
        }


        /// <summary>
        /// Event type call to action text.
        /// </summary>
        [DatabaseField]
        public virtual string EventTypeCallToActionText
        {
            get => ValidationHelper.GetString(GetValue(nameof(EventTypeCallToActionText)), String.Empty);
            set => SetValue(nameof(EventTypeCallToActionText), value);
        }


        /// <summary>
        /// Event type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int EventTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EventTypeSiteID)), 0);
            set => SetValue(nameof(EventTypeSiteID), value);
        }


        /// <summary>
        /// Event type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid EventTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(EventTypeGuid)), Guid.Empty);
            set => SetValue(nameof(EventTypeGuid), value);
        }


        /// <summary>
        /// Event type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime EventTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(EventTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(EventTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="EventTypeInfo"/> class.
        /// </summary>
        public EventTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="EventTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public EventTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}