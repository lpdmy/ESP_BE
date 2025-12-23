using System.Text.Encodings.Web;
using System.Text.Json;

namespace EduShpere.Shared;
public static class JsonSerializationExtensions
{
    public static string ToJson(this object obj)
    { 
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(obj, serializeOptions);
    }
}
