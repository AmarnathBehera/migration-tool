using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(EventRegistrationInfo), EventRegistrationInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="EventRegistrationInfo"/>.
    /// </summary>
    public partial class EventRegistrationInfo : AbstractInfo<EventRegistrationInfo, IInfoProvider<EventRegistrationInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.eventregistration";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<EventRegistrationInfo>), OBJECT_TYPE, "BDO.EventRegistration", "EventRegistrationID", "EventRegistrationLastModified", "EventRegistrationGUID", "Codename", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Event registration ID.
        /// </summary>
        [DatabaseField]
        public virtual int EventRegistrationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EventRegistrationID)), 0);
            set => SetValue(nameof(EventRegistrationID), value);
        }


        /// <summary>
        /// Codename.
        /// </summary>
        [DatabaseField]
        public virtual string Codename
        {
            get => ValidationHelper.GetString(GetValue(nameof(Codename)), String.Empty);
            set => SetValue(nameof(Codename), value);
        }


        /// <summary>
        /// Event GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid EventGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(EventGUID)), Guid.Empty);
            set => SetValue(nameof(EventGUID), value);
        }


        /// <summary>
        /// Status.
        /// </summary>
        [DatabaseField]
        public virtual string Status
        {
            get => ValidationHelper.GetString(GetValue(nameof(Status)), "registered");
            set => SetValue(nameof(Status), value);
        }


        /// <summary>
        /// User GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid UserGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(UserGUID)), Guid.Empty);
            set => SetValue(nameof(UserGUID), value);
        }


        /// <summary>
        /// Event registration GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid EventRegistrationGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(EventRegistrationGUID)), Guid.Empty);
            set => SetValue(nameof(EventRegistrationGUID), value);
        }


        /// <summary>
        /// Event registration last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime EventRegistrationLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(EventRegistrationLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(EventRegistrationLastModified), value);
        }


        /// <summary>
        /// Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteID)), 0);
            set => SetValue(nameof(SiteID), value);
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
        /// Creates an empty instance of the <see cref="EventRegistrationInfo"/> class.
        /// </summary>
        public EventRegistrationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="EventRegistrationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public EventRegistrationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}