using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Cyoukon.Extensions.Configuration.Abstractions
{
    public static class ConfigurationParser
    {
        public static Dictionary<string, string?> ParseJsonContent(string content)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;
                ParseElement(root, string.Empty, result);
            }
            catch (JsonException)
            {
                result["Content"] = content;
            }

            return result;
        }

        private static void ParseElement(JsonElement element, string prefix, Dictionary<string, string?> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                        ParseElement(property.Value, key, result);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        var key = $"{prefix}:{index}";
                        ParseElement(item, key, result);
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    result[prefix] = element.GetString();
                    break;

                case JsonValueKind.Number:
                    result[prefix] = element.GetRawText();
                    break;

                case JsonValueKind.True:
                    result[prefix] = "true";
                    break;

                case JsonValueKind.False:
                    result[prefix] = "false";
                    break;

                case JsonValueKind.Null:
                    result[prefix] = null;
                    break;
            }
        }

        public static string SerializeToJson(Dictionary<string, string?> data)
        {
            var root = new Dictionary<string, object?>();
            
            foreach (var kvp in data)
            {
                var parts = kvp.Key.Split(':');
                var current = root;
                
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var part = parts[i];
                    if (!current.ContainsKey(part))
                    {
                        current[part] = new Dictionary<string, object?>();
                    }
                    current = (Dictionary<string, object?>)current[part]!;
                }
                
                current[parts[^1]] = kvp.Value;
            }

            return JsonSerializer.Serialize(root, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static string DecodeBase64Content(string base64Content)
        {
            var bytes = Convert.FromBase64String(base64Content);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string EncodeToBase64(string content)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        }
    }
}
