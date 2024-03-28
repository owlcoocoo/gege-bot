using CQHttp;
using RestSharp;
using System.Text.Json.Nodes;

namespace GegeBot.Plugins.LlamaCpp
{
    internal class SetuAPI
    {
        private readonly RestClient client;

        const string API_URL = "https://api.lolicon.app/setu/v2";

        public SetuAPI()
        {
            client = new RestClient();
            client.AddDefaultHeader("Content-Type", "application/json");
        }

        public JsonNode Get(bool excludeAI = false, int r18 = 0)
        {
            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{API_URL}?proxy=&excludeAI={excludeAI}&r18={r18}&num=1", Method.Get);
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            return json_result;
        }
    }
}
