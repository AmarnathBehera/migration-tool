using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class DealCardsClassMappings
{
    public static IServiceCollection AddDealCardsMappings(this IServiceCollection services)
    {
        services.AddSingleton<IClassMapping>(CreateIndustryMapping());
        services.AddSingleton<IClassMapping>(CreateDealTypeMapping());
        services.AddSingleton<IClassMapping>(CreateRangeMapping());
        return services;
    }

    private static MultiClassMapping CreateIndustryMapping()
    {
        const string sourceClassName = "BDODealCards.Industry";
        var mapping = CreateMapping("BDO.DealCardIndustry", "BDO_DealCardIndustry_1", "Deal Card Industry");
        mapping.BuildField("Name").SetFrom(sourceClassName, "IndustryName");
        mapping.BuildField("CodeName").SetFrom(sourceClassName, "IndustryCodeName");
        mapping.BuildField("Language").SetFrom(sourceClassName, "IndustryCultureCode");
        return mapping;
    }

    private static MultiClassMapping CreateDealTypeMapping()
    {
        const string sourceClassName = "BDODealCards.DealType";
        var mapping = CreateMapping("BDO.DealCardDealType", "BDO_DealCardDealType", "Deal Card Deal Type");
        mapping.BuildField("Name").SetFrom(sourceClassName, "DealTypeName");
        mapping.BuildField("CodeName").SetFrom(sourceClassName, "DealTypeCodeName");
        mapping.BuildField("Language").SetFrom(sourceClassName, "DealTypeCultureCode");
        return mapping;
    }

    private static MultiClassMapping CreateRangeMapping()
    {
        const string sourceClassName = "BDODealCards.Range";
        var mapping = CreateMapping("BDO.DealCardRange", "BDO_DealCardRange", "Deal Card Range");
        mapping.BuildField("Name").SetFrom(sourceClassName, "RangeValue");
        mapping.BuildField("CodeName").SetFrom(sourceClassName, "RangeCodeName");
        return mapping;
    }

    private static MultiClassMapping CreateMapping(string targetClassName, string targetTableName, string displayName)
    {
        return new MultiClassMapping(targetClassName, target =>
        {
            target.ClassName = targetClassName;
            target.ClassTableName = targetTableName;
            target.ClassDisplayName = displayName;
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });
    }
}
