using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Helper;

public static class ObjectHelper
{
    public static async Task<string> CalculateObjectHashAsync(object obj)
    {
        var jsonString = string.Empty;
        using (var memoryStream = new MemoryStream())
        {
            await JsonSerializer.SerializeAsync(memoryStream, obj);
            jsonString = Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        var byteArray = Encoding.UTF8.GetBytes(jsonString);
        var hashBytes = SHA256.HashData(byteArray);
        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    public static void CopyNotNullProperties<TObject>(TObject source, TObject target)
    {
        var properties = typeof(TObject).GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(source);
            if (value != null)
            {
                property.SetValue(target, value);
            }
        }
    }
}
