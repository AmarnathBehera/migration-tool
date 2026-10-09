namespace BDO.GWT.Common.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureEndsWith(this string value, string suffix)
        {
            return value.EndsWith(suffix) ? value : value.Insert(value.Length, suffix);
        }
    }
}
