using CQHttp;
using RestSharp;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GegeBot.Plugins.Pixiv
{
    public class PixivAPI
    {
        private readonly RestClient client;
        private readonly RestClient download_client;

        const string UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1";

        public PixivAPI()
        {
            var options = new RestClientOptions()
            {
                UserAgent = UserAgent,
            };
            if (!string.IsNullOrEmpty(PixivConfig.Proxy))
                options.Proxy = new WebProxy(PixivConfig.Proxy);
            client = new RestClient(options);
            client.AddDefaultHeader("Accept", "application/json");
            client.AddDefaultHeader("Cookie", PixivConfig.Cookie);

            options = new RestClientOptions()
            {
                UserAgent = UserAgent,
            };
            if (string.IsNullOrEmpty(PixivConfig.PximgReverseProxy))
                options.Proxy = new WebProxy(PixivConfig.Proxy);
            download_client = new RestClient(options);
        }

        public static Task<byte[]> Compress(byte[] source, int quality)
        {
            if (quality >= 100) return Task.FromResult(source);
            return Task.Run(() =>
            {
                using var bitmap = SKBitmap.Decode(source);
                return bitmap.Encode(SKEncodedImageFormat.Jpeg, quality).ToArray();
            });
        }

        public static byte[] Puzzle(List<byte[]> data, int perWidth, int perHeight, int quality, int maxCols = 3)
        {
            int count = data.Count;
            maxCols = count < maxCols ? count : maxCols;
            int bitmapRows = (int)Math.Ceiling((decimal)count / maxCols);
            var newBitmap = new SKBitmap(perWidth * maxCols, perHeight * bitmapRows);
            SKCanvas canvas = new SKCanvas(newBitmap);
            canvas.DrawColor(new SKColor(255, 255, 255));
            for (int i = 0, row = 0, col = 0; i < count; i++)
            {
                if (i != 0 && i % maxCols == 0)
                {
                    col = 0;
                    row++;
                }

                var bitmap = SKBitmap.Decode(data[i]);
                bitmap = bitmap.Resize(new SKImageInfo(perHeight, perHeight), SKFilterQuality.High);
                canvas.DrawBitmap(bitmap, col * perWidth, row * perHeight);

                col++;
            }
            
            return newBitmap.Encode(SKEncodedImageFormat.Jpeg, quality).ToArray();
        }

        private RestResponse ExecuteAndRetry(RestRequest request, int retryCount = 10)
        {
            int retryCounter = 0;
            request.Timeout = 1000 * 10;

            RestResponse response;
            do
            {
                response = client.Execute(request);
                if (!response.IsSuccessful || response.ContentLength < 1)
                {
                    retryCounter++;
                }
                else break;
            }
            while (retryCounter < retryCount);

            return response;
        }

        private async Task<RestResponse> ExecuteDownloadAndRetryAsync(RestRequest request, int retryCount = 3)
        {
            int retryCounter = 0;
            request.Timeout = 1000 * PixivConfig.Timeout;

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
            if (!string.IsNullOrEmpty(PixivConfig.PximgReverseProxy))
                url = url.Replace("https://i.pximg.net", PixivConfig.PximgReverseProxy);
            Console.WriteLine($"[pixiv]下载 {url}");
            var request = new RestRequest(url, Method.Get);
            RestResponse response = await ExecuteDownloadAndRetryAsync(request);
            if (response.RawBytes != null && response.RawBytes.Length > 0)
            {
                return await Compress(response.RawBytes, PixivConfig.ImageQuality);
            }
            return null;
        }

        private KeyValuePair<int, byte[]>[] DownloadImages(List<string> urls)
        {
            ConcurrentDictionary<int, byte[]> imageDict = new();
            List<Task> tasks = new();

            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                Task task = new Task(new Action<object>((i) =>
                {
                    var result = Download(url).Result;
                    if (result != null)
                    {
                        imageDict.TryAdd((int)i, result);
                    }
                    else
                    {
                        Console.WriteLine($"[pixiv]下载失败 {url}");
                    }
                }), i);
                tasks.Add(task);
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
            return imageDict.OrderBy(a => a.Key).ToArray();
        }

        private PixivDto Touch_GetIllusts(PixivDto dto, string id)
        {
            List<string> urls = new List<string>();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/illust/details?illust_id={id}", Method.Get);
            var response = ExecuteAndRetry(request);
            var json_result = Json.ToJsonNode(response.Content);
            if (json_result["error"].GetValue<bool>())
            {
                dto.Message = "没有搜到相关的作品";
                return dto;
            }

            var illust_details = json_result["body"]["illust_details"];
            var manga_a = illust_details["manga_a"];
            if (manga_a == null)
                urls.Add(illust_details["url_big"].GetValue<string>());
            else
            {
                foreach (var item in manga_a.AsArray())
                    urls.Add(item["url_big"].GetValue<string>());
            }

            dto.Alt = illust_details["alt"].GetValue<string>();

            urls = urls.Take(PixivConfig.MaxImages).ToList();

            var images = DownloadImages(urls);

            if (images.Length < 1)
                dto.Message = "图片下载失败";
            else
            {
                foreach(var item in images) 
                {
                    dto.Images.Add($"base64://{Convert.ToBase64String(item.Value)}");
                }
            }

            return dto;
        }

        public PixivDto Touch_SearchIllusts(string word)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/illusts?include_meta=1&s_mode=s_tag&type=all&word={word}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            var illusts = json_result["body"]["illusts"].AsArray();
            if (json_result["error"].GetValue<bool>() || illusts.Count < 1)
            {
                dto.Message = "没有搜到相关的作品";
                return dto;
            }

            var n = new Random().Next(0, illusts.Count - 1);
            var illust = illusts[n];
            return Touch_GetIllusts(dto, illust["id"].GetValue<string>());
        }

        private JsonArray Touch_GetUserIllusts(string nick, string tag, out string error)
        {
            error = "";

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/users?nick={nick}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            var users = json_result["body"]["users"].AsArray();
            if (json_result["error"].GetValue<bool>() || users.Count < 1)
            {
                error = "没有搜到相关的画师";
                return null;
            }

            var user = users[0];

            string param_tag = string.IsNullOrEmpty(tag) ? "" : $"&tag={tag}";
            request = new RestRequest($"https://www.pixiv.net/touch/ajax/user/illusts?id={user["user_id"]}{param_tag}", Method.Get);
            response = ExecuteAndRetry(request);
            json_result = Json.ToJsonNode(response.Content);
            var illusts = json_result["body"]["illusts"].AsArray();
            if (json_result["error"].GetValue<bool>() || illusts.Count < 1)
            {
                error = "没有搜到相关的画师作品";
                return null;
            }

            return illusts;
        }


        public PixivDto Touch_SearchUserIllusts(string nick, string tag = "")
        {
            PixivDto dto = new PixivDto();

            var illusts = Touch_GetUserIllusts(nick, tag, out string errror);
            if (illusts ==  null)
            {
                dto.Message = errror;
                return dto;
            }

            var n = new Random().Next(0, illusts.Count - 1);
            var illust = illusts[n];
            return Touch_GetIllusts(dto, illust["id"].GetValue<string>());
        }

        public PixivDto Touch_GetRankIllusts(string mode)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/ranking/illust?mode={mode}&type=all&page=1", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            var ranking = json_result["body"]["ranking"].AsArray().Take(10);
            if (!ranking.Any())
            {
                dto.Message = "没有相关的作品";
                return dto;
            }

            string url = "https://www.pixiv.net/touch/ajax/illust/details/many?";
            foreach (var item in ranking)
            {
                url += $"illust_ids%5B%5D={item["illustId"]}&";
            }
            url = url.TrimEnd('&');

            request = new RestRequest(url, Method.Get);
            response = ExecuteAndRetry(request);
            json_result = Json.ToJsonNode(response.Content);
            var illusts = json_result["body"]["illust_details"].AsArray();
            if (illusts.Count < 1)
            {
                dto.Message = "没有相关的作品";
                return dto;
            }

            List<string> urls = new();

            for (int i = 0; i < 10; i++)
            {
                var item = illusts[i];
                dto.Alt += $"{i + 1} {item["alt"]}\r\n";
                url = item["url_sm"].GetValue<string>();
                urls.Add(url);
            }

            var images = DownloadImages(urls);

            List<byte[]> dataList = new List<byte[]>();
            foreach (var image in images)
            {
                dataList.Add(image.Value);
            }

            string result = "base64://" + Convert.ToBase64String(Puzzle(dataList, 360, 360, 90, 5));
            dto.Images.Add(result);

            return dto;
        }

        public PixivDto Touch_GetIllustsPreview(string word)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/illusts?include_meta=1&s_mode=s_tag&type=all&word={word}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            var illusts = json_result["body"]["illusts"].AsArray();
            if (illusts.Count < 1)
            {
                dto.Message = "没有搜到相关的作品";
                return dto;
            }

            List<string> urls = new();
            for (int i = 0; i < illusts.Count; i++)
            {
                var item = illusts[i];
                //dto.Alt += $"{i + 1} {item["alt"]}\r\n";
                string url = item["url_sm"].GetValue<string>();
                urls.Add(url);
            }

            var images = DownloadImages(urls);

            List<byte[]> dataList = new List<byte[]>();
            foreach (var image in images)
            {
                dataList.Add(image.Value);
            }
            string result = "base64://" + Convert.ToBase64String(Puzzle(dataList, 360, 360, 90, 6));
            dto.Images.Add(result);

            return dto;
        }

        public PixivDto Touch_GetUserIllustsPreview(string nick, string tag = "")
        {
            PixivDto dto = new PixivDto();

            var illusts = Touch_GetUserIllusts(nick, tag, out string errror);
            if (illusts == null)
            {
                dto.Message = errror;
                return dto;
            }

            List<string> urls = new();
            for (int i = 0; i < illusts.Count; i++)
            {
                var item = illusts[i];
                //dto.Alt += $"{i + 1} {item["alt"]}\r\n";
                string url = item["url_sm"].GetValue<string>();
                urls.Add(url);
            }

            var images = DownloadImages(urls);

            List<byte[]> dataList = new List<byte[]>();
            foreach (var image in images)
            {
                dataList.Add(image.Value);
            }
            string result = "base64://" + Convert.ToBase64String(Puzzle(dataList, 360, 360, 90, 6));
            dto.Images.Add(result);

            return dto;
        }
    }
}

