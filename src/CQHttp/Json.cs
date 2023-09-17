using System.Text.Json;
using System.Text.Json.Nodes;

namespace CQHttp
{
    public static class Json
    {
        public static T FromJsonString<T>(string json) where T : new()
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }

        public static string ToJsonString<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static JsonNode ToJsonNode(string json)
        {
            var documentOptions = new JsonDocumentOptions();
            documentOptions.AllowTrailingCommas = true;
            documentOptions.CommentHandling = JsonCommentHandling.Skip;
            return JsonNode.Parse(json, documentOptions: documentOptions);
        }

        public static JsonNode ToJsonNode<T>(T obj)
        {
            return JsonSerializer.SerializeToNode(obj);
        }
    }
}
