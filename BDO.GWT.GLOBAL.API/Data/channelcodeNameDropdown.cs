using System.ComponentModel;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Data;

[TypeConverter(typeof(ChannelCodeNameDropdownConverter))]
public class channelcodeNameDropdown
{
    /// <summary>Xperience website channel name, e.g. "Ind".</summary>
    public string Value { get; set; } = string.Empty;
}

public class ChannelCodeNameDropdownConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string s ? new channelcodeNameDropdown { Value = s } : base.ConvertFrom(context, culture, value);
}