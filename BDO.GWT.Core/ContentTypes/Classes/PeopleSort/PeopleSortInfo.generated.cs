using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(PeopleSortInfo), PeopleSortInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="PeopleSortInfo"/>.
    /// </summary>
    public partial class PeopleSortInfo : AbstractInfo<PeopleSortInfo, IInfoProvider<PeopleSortInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.peoplesort";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<PeopleSortInfo>), OBJECT_TYPE, "BDO.PeopleSort", "PeopleSortID", "PeopleSortLastModified", "PeopleSortGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// People sort ID.
        /// </summary>
        [DatabaseField]
        public virtual int PeopleSortID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PeopleSortID)), 0);
            set => SetValue(nameof(PeopleSortID), value);
        }


        /// <summary>
        /// People sort guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid PeopleSortGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(PeopleSortGuid)), Guid.Empty);
            set => SetValue(nameof(PeopleSortGuid), value);
        }


        /// <summary>
        /// People sort last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PeopleSortLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(PeopleSortLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(PeopleSortLastModified), value);
        }


        /// <summary>
        /// People sort site ID.
        /// </summary>
        [DatabaseField]
        public virtual int PeopleSort_SiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PeopleSort_SiteID)), 0);
            set => SetValue(nameof(PeopleSort_SiteID), value);
        }


        /// <summary>
        /// People sort node guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid PeopleSort_NodeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(PeopleSort_NodeGuid)), Guid.Empty);
            set => SetValue(nameof(PeopleSort_NodeGuid), value);
        }


        /// <summary>
        /// People sort people node guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid PeopleSort_PeopleNodeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(PeopleSort_PeopleNodeGuid)), Guid.Empty);
            set => SetValue(nameof(PeopleSort_PeopleNodeGuid), value);
        }


        /// <summary>
        /// People sort sort order.
        /// </summary>
        [DatabaseField]
        public virtual int PeopleSort_SortOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PeopleSort_SortOrder)), 0);
            set => SetValue(nameof(PeopleSort_SortOrder), value);
        }


        /// <summary>
        /// People sort culture code.
        /// </summary>
        [DatabaseField]
        public virtual string PeopleSort_CultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(PeopleSort_CultureCode)), String.Empty);
            set => SetValue(nameof(PeopleSort_CultureCode), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="PeopleSortInfo"/> class.
        /// </summary>
        public PeopleSortInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="PeopleSortInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public PeopleSortInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}