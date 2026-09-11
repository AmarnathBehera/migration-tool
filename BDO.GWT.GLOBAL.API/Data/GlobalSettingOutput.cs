namespace BDO.GWT.GLOBAL.API.Data;

public class GlobalSettingOutput 
{
    public required string CodeName { get; set; }

    public required string DisplayName { get; set; }

    public required string Type { get; set; }

    public object? Value { get; set; }
}
