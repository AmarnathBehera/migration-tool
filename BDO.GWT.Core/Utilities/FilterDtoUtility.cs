using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.Utilities;

public static class FilterDtoUtility
{
    public static FilterItemDto<TOptionDto> CreateFilterItemDto<TOptionDto>(string type, string name, string title, string? defaultValue, TOptionDto[] options)
        where TOptionDto : OptionDto
    {
        var filterItem = new FilterItemDto<TOptionDto>
        {
            Type = type,
            Title = title,
            Name = name,
            DefaultValue = defaultValue,
            Options = options,
        };

        return filterItem;
    }

    public static OptionDto CreateOptionDto(string label, string value)
    {
        var option = new OptionDto
        {
            Label = label,
            Value = value,
        };

        return option;
    }
}
