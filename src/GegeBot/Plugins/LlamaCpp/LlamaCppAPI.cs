using CQHttp;
using RestSharp;
using System.Text.Json.Nodes;

namespace GegeBot.Plugins.LlamaCpp
{
    internal class LlamaCppAPI
    {
        private readonly RestClient client;
        private readonly string _serverAddress;

        public LlamaCppAPI(string serverAddress)
        {
            _serverAddress = serverAddress;
            client = new RestClient();
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Accept", "application/json");
        }

        public string Completion(string prompt, decimal temperature, List<string> stop)
        {
            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{_serverAddress}/completion", Method.Post);
            requestObj.Add("stream", false);
            requestObj.Add("temperature", temperature);
            requestObj.Add("n_predict", -1);
            requestObj.Add("n_keep", -1);
            requestObj.Add("prompt", prompt);
            JsonArray stopArray = new JsonArray();
            foreach (var item in stop)
            {
                stopArray.Add(item);
            }
            requestObj.Add("stop", stopArray);
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            if (string.IsNullOrWhiteSpace(response.Content))
                return "";
            JsonNode json_result = Json.ToJsonNode(response.Content);
            string content = json_result["content"].GetValue<string>();
            return content;
        }
    }
}
