using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(LinkedPagesInfo), LinkedPagesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="LinkedPagesInfo"/>.
    /// </summary>
    public partial class LinkedPagesInfo : AbstractInfo<LinkedPagesInfo, IInfoProvider<LinkedPagesInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.linkedpages";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<LinkedPagesInfo>), OBJECT_TYPE, "BDO.LinkedPages", "LinkedPagesID", "LinkedPagesLastModified", "LinkedPagesGuid", "LinkedPagesCodeName", "LinkedPagesName", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Linked pages ID.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedPagesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LinkedPagesID)), 0);
            set => SetValue(nameof(LinkedPagesID), value);
        }


        /// <summary>
        /// Linked pages name.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesName)), String.Empty);
            set => SetValue(nameof(LinkedPagesName), value);
        }


        /// <summary>
        /// Linked pages code name.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesCodeName)), String.Empty);
            set => SetValue(nameof(LinkedPagesCodeName), value);
        }


        /// <summary>
        /// Linked pages culture code.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesCultureCode)), String.Empty);
            set => SetValue(nameof(LinkedPagesCultureCode), value, String.Empty);
        }


        /// <summary>
        /// Linked pages node id.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedPagesNodeId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LinkedPagesNodeId)), 0);
            set => SetValue(nameof(LinkedPagesNodeId), value, 0);
        }


        /// <summary>
        /// Linked pages people ID.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesPeopleID
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesPeopleID)), String.Empty);
            set => SetValue(nameof(LinkedPagesPeopleID), value, String.Empty);
        }


        /// <summary>
        /// Linked pages node GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid LinkedPagesNodeGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(LinkedPagesNodeGUID)), Guid.Empty);
            set => SetValue(nameof(LinkedPagesNodeGUID), value);
        }


        /// <summary>
        /// Linked pages after import node I ds.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedPagesAfterImportNodeIDs
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LinkedPagesAfterImportNodeIDs)), 0);
            set => SetValue(nameof(LinkedPagesAfterImportNodeIDs), value, 0);
        }


        /// <summary>
        /// Linked pages after import people I ds.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesAfterImportPeopleIDs
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesAfterImportPeopleIDs)), String.Empty);
            set => SetValue(nameof(LinkedPagesAfterImportPeopleIDs), value, String.Empty);
        }


        /// <summary>
        /// Linked pages classname.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedPagesClassname
        {
            get => ValidationHelper.GetString(GetValue(nameof(LinkedPagesClassname)), String.Empty);
            set => SetValue(nameof(LinkedPagesClassname), value, String.Empty);
        }


        /// <summary>
        /// Linked pages site ID.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedPagesSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(LinkedPagesSiteID)), 0);
            set => SetValue(nameof(LinkedPagesSiteID), value, 0);
        }


        /// <summary>
        /// Linked pages guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid LinkedPagesGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(LinkedPagesGuid)), Guid.Empty);
            set => SetValue(nameof(LinkedPagesGuid), value);
        }


        /// <summary>
        /// Linked pages last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LinkedPagesLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(LinkedPagesLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(LinkedPagesLastModified), value);
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
        /// Creates an empty instance of the <see cref="LinkedPagesInfo"/> class.
        /// </summary>
        public LinkedPagesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="LinkedPagesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public LinkedPagesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}