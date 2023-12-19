using CQHttp;
using GegeBot.Plugins.Pixiv;
using RestSharp;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace GegeBot.Plugins.EdgeGPT
{
    public class EdgeGptAPI
    {
        private readonly RestClient client;
        private readonly RestClient download_client;
        private readonly string _serverAddress;
        private string _proxy;

        private string _botId;

        public EdgeGptAPI(string serverAddress)
        {
            _serverAddress = serverAddress;
            client = new RestClient();
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Accept", "application/json");

            download_client = new RestClient();
        }

        public bool Create(string id, string cookies, string proxy = null)
        {
            _proxy = proxy;

            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{_serverAddress}/create", Method.Post);
            requestObj.Add("id", id);
            requestObj.Add("cookies", cookies);
            requestObj.Add("proxy", proxy);
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;
            JsonNode json_result = Json.ToJsonNode(response.Content);
            _botId = json_result["id"].GetValue<string>();
            return true;
        }

        public JsonNode Ask(string prompt, string conversation_style = "creative", string imageUrl = null, string wss_link = null)
        {
            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{_serverAddress}/ask", Method.Post);
            requestObj.Add("id", _botId);
            requestObj.Add("prompt", prompt);
            requestObj.Add("conversation_style", conversation_style);
            if (!string.IsNullOrEmpty(wss_link))
                requestObj.Add("wss_link", wss_link);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var attachmentObj = new JsonObject
                {
                    { "image_url", imageUrl },
                    { "proxy", _proxy }
                };
                requestObj.Add("attachment", attachmentObj);
            }
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            return json_result;
        }

        public JsonNode GenerateImages(string auth_cookie, string prompt, string proxy = "")
        {
            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{_serverAddress}/generate_image", Method.Post);
            requestObj.Add("auth_cookie", auth_cookie);
            requestObj.Add("prompt", prompt);
            if (!string.IsNullOrEmpty(proxy))
                requestObj.Add("proxy", proxy);
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            return json_result;
        }

        public bool Reset()
        {
            JsonObject requestObj = new JsonObject();
            var request = new RestRequest($"{_serverAddress}/reset", Method.Post);
            requestObj.Add("id", _botId);
            request.AddJsonBody(requestObj.ToJsonString(), false);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;
            return true;
        }

        private async Task<RestResponse> ExecuteDownloadAndRetryAsync(RestRequest request, int retryCount = 3)
        {
            int retryCounter = 0;
            request.Timeout = 1000 * 30;

            RestResponse response;
            do
            {
                response = await download_client.ExecuteAsync(request);
                if (!response.IsSuccessful || response.ContentLength < 1)
                {
                    retryCounter++;
                }
                else break;
            }
            while (retryCounter < retryCount);

            return response;
        }

        private async Task<byte[]> Download(string url)
        {
            Console.WriteLine($"[bing image]下载 {url}");
            var request = new RestRequest(url, Method.Get);
            RestResponse response = await ExecuteDownloadAndRetryAsync(request);
            if (response.RawBytes != null && response.RawBytes.Length > 0)
            {
                return response.RawBytes;
            }
            Console.WriteLine($"[bing image]下载失败 {url}");
            return null;
        }

        public KeyValuePair<int, byte[]>[] DownloadImages(List<string> urls)
        {
            ConcurrentDictionary<int, byte[]> imageDict = new();

            Parallel.For(0, urls.Count, (i) =>
            {
                string url = urls[i];
                var result = Download(url).Result;
                if (result != null)
                {
                    imageDict.TryAdd(i, result);
                }
            });

            return imageDict.OrderBy(a => a.Key).ToArray();
        }
    }
}
