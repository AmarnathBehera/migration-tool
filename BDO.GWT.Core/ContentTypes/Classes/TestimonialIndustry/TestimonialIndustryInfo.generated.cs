using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(TestimonialIndustryInfo), TestimonialIndustryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="TestimonialIndustryInfo"/>.
    /// </summary>
    public partial class TestimonialIndustryInfo : AbstractInfo<TestimonialIndustryInfo, IInfoProvider<TestimonialIndustryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.testimonialindustry";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<TestimonialIndustryInfo>), OBJECT_TYPE, "BDOMetadata.TestimonialIndustry", "TestimonialIndustryID", "TestimonialIndustryLastModified", "TestimonialIndustryGuid", "IndustryCodeName", "IndustryName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("TestimonialIndustrySiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Testimonial industry ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialIndustryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialIndustryID)), 0);
            set => SetValue(nameof(TestimonialIndustryID), value);
        }


        /// <summary>
        /// Industry name.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryName)), String.Empty);
            set => SetValue(nameof(IndustryName), value);
        }


        /// <summary>
        /// Industry code name.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryCodeName)), String.Empty);
            set => SetValue(nameof(IndustryCodeName), value);
        }


        /// <summary>
        /// Industry culture code.
        /// </summary>
        [DatabaseField]
        public virtual string IndustryCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndustryCultureCode)), String.Empty);
            set => SetValue(nameof(IndustryCultureCode), value);
        }


        /// <summary>
        /// Testimonial industry site ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialIndustrySiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialIndustrySiteID)), 0);
            set => SetValue(nameof(TestimonialIndustrySiteID), value);
        }


        /// <summary>
        /// Testimonial industry guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid TestimonialIndustryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(TestimonialIndustryGuid)), Guid.Empty);
            set => SetValue(nameof(TestimonialIndustryGuid), value);
        }


        /// <summary>
        /// Testimonial industry last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TestimonialIndustryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(TestimonialIndustryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(TestimonialIndustryLastModified), value);
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
        /// Creates an empty instance of the <see cref="TestimonialIndustryInfo"/> class.
        /// </summary>
        public TestimonialIndustryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="TestimonialIndustryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TestimonialIndustryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}