namespace BDO.GWT.Core.ValueObjects;

public record RelativeLink : Link
{
    public override string ToString()
    {
        if (Path is null && Query is null && Hash is null)
        {
            return string.Empty;
        }

        var uriString = $"~/{Path}";

        if (Query != null)
        {
            uriString += $"?{Query}";
        }

        if (Hash != null)
        {
            uriString += $"#{Hash}";
        }

        return uriString;
    }
    
    public override string ToHtmlCompatibleString()
    {
        return ToString().TrimStart('~');
    }
}
