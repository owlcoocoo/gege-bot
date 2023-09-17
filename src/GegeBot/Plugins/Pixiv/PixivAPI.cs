using CQHttp;
using CQHttp.DTOs;
using RestSharp;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json.Nodes;

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

        /// <summary>
        /// 拼图
        /// </summary>
        /// <param name="data">图片数据集</param>
        /// <param name="text">文本数据集，需要跟data对应</param>
        /// <param name="perWidth">每张图片宽度</param>
        /// <param name="perHeight">每张图片高度</param>
        /// <param name="quality">图片质量</param>
        /// <param name="maxCols">最大列数</param>
        /// <param name="textSize">文本大小</param>
        /// <returns></returns>
        public static byte[] Puzzle(List<byte[]> data, List<string> text, int perWidth, int perHeight, int quality, int maxCols = 3, int textSize = 32)
        {
            int count = data.Count;
            maxCols = count < maxCols ? count : maxCols;
            int bitmapRows = (int)Math.Ceiling((decimal)count / maxCols);
            int textHeightTotal = text != null && text.Count > 0 ? textSize * bitmapRows : 0;
            var newBitmap = new SKBitmap(perWidth * maxCols, perHeight * bitmapRows + textHeightTotal);
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

                int textHeight = 0;
                if (textHeightTotal > 0)
                {
                    SKPaint paint = new SKPaint();
                    paint.TextSize = textSize;
                    paint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText(text[i], col * perWidth + perWidth / 2, row * (perHeight + textSize) + (float)(textSize * 0.8), paint);
                    textHeight = (row + 1) * textSize;
                }
                
                canvas.DrawBitmap(bitmap, col * perWidth, textHeight + row * perHeight);

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
            Console.WriteLine($"[pixiv]下载失败 {url}");
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
                }), i);
                tasks.Add(task);
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
            return imageDict.OrderBy(a => a.Key).ToArray();
        }

        private bool IsError(JsonNode node)
        {
            if (node["error"].GetValue<bool>()) return true;

            return false;
        }

        private bool IsExist(JsonNode node, string body_array_key)
        {
            if (IsError(node)) return false;

            var illusts = node["body"][body_array_key].AsArray();
            if (!illusts.Any()) return false;

            return true;
        }

        private PixivDto Touch_GetIllusts(PixivDto dto, string id, int index = 0)
        {
            List<string> urls = new List<string>();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/illust/details?illust_id={id}", Method.Get);
            var response = ExecuteAndRetry(request);
            var json_result = Json.ToJsonNode(response.Content);
            if (IsError(json_result))
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

            dto.Alt += $"{illust_details["alt"]}\r\n作品id{illust_details["id"]}\r\n画师id{illust_details["user_id"]}";

            dto.ImageCount = urls.Count;
            urls = urls.Skip(index).Take(PixivConfig.MaxImages).ToList();
            var images = DownloadImages(urls);
            if (images.Length < 1)
                dto.Message = "图片下载失败";
            else
            {
                foreach(var item in images) 
                {
                    dto.Images.Add(item.Value);
                }

                dto.ImageMessage = $"{images.Length + index} / {dto.ImageCount} \r\n\r\n";
            }

            return dto;
        }

        /// <summary>
        /// 查找作品
        /// </summary>
        /// <param name="word">关键字</param>
        /// <param name="action">图片定位操作，提供作品id，需返回图片index。</param>
        /// <returns></returns>
        public PixivDto Touch_SearchIllusts(string word, Func<string, int> action = null)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/illusts?include_meta=1&s_mode=s_tag&type=all&word={word}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            if (!IsExist(json_result, "illusts"))
            {
                dto.Message = "没有搜到相关的作品";
                return dto;
            }

            var illusts = json_result["body"]["illusts"].AsArray();
            var n = new Random().Next(0, illusts.Count - 1);
            var illust = illusts[n];
            string id = illust["id"].GetValue<string>();
            int index = action?.Invoke(id) ?? 0;
            return Touch_GetIllusts(dto, id, index);
        }

        /// <summary>
        /// 获取作品
        /// </summary>
        /// <param name="id">作品id</param>
        /// <param name="index">定位图片</param>
        /// <returns></returns>
        public PixivDto Touch_GetIllusts(string id, int index = 0)
        {
            PixivDto dto = new PixivDto();
            return Touch_GetIllusts(dto, id, index);
        }

        private JsonArray Touch_GetUserIllusts(string user_id, string tag, out string error)
        {
            error = "";

            string param_tag = string.IsNullOrEmpty(tag) ? "" : $"&tag={tag}";
            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/user/illusts?id={user_id}{param_tag}", Method.Get);
            var response = ExecuteAndRetry(request);
            var json_result = Json.ToJsonNode(response.Content);
            if (!IsExist(json_result, "illusts"))
            {
                error = "没有搜到相关的画师作品";
                return null;
            }
            var illusts = json_result["body"]["illusts"].AsArray();
            return illusts;
        }

        private JsonArray Touch_SearchUserIllusts(string nick, string tag, out string error)
        {
            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/users?nick={nick}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            if (!IsExist(json_result, "users"))
            {
                error = "没有搜到相关的画师";
                return null;
            }

            var users = json_result["body"]["users"].AsArray();
            var user = users[0];
            var illusts = Touch_GetUserIllusts(user["user_id"].ToString(), tag, out error);
            return illusts;
        }

        /// <summary>
        /// 查找用户作品
        /// </summary>
        /// <param name="nick">用户名</param>
        /// <param name="user_id">用户id，如传入则无视nick字段</param>
        /// <param name="tag">作品标签</param>
        /// <param name="action">图片定位操作，提供作品id，需返回图片index。</param>
        /// <returns></returns>
        public PixivDto Touch_SearchUserIllusts(string nick, string user_id = "", string tag = "", Func<string, int> action = null)
        {
            PixivDto dto = new PixivDto();

            var illusts = string.IsNullOrEmpty(user_id)
                ? Touch_SearchUserIllusts(nick, tag, out string error)
                : Touch_GetUserIllusts(user_id, tag, out error);
            if (illusts ==  null)
            {
                dto.Message = error;
                return dto;
            }

            var n = new Random().Next(0, illusts.Count - 1);
            var illust = illusts[n];
            string id = illust["id"].GetValue<string>();
            int index = action?.Invoke(id) ?? 0;
            return Touch_GetIllusts(dto, id, index);
        }

        private PixivDto HandleImages(PixivDto dto, JsonArray illusts, int count, bool needAlt, int perWidth, int perHeight, int quality, int maxCols)
        {
            List<string> urls = new();
            List<string> ids = new();
            for (int i = 0; i < count; i++)
            {
                var item = illusts[i];
                if (needAlt)
                    dto.Alt += $"{i + 1} {item["alt"]}\r\n";
                string url = item["url_sm"].GetValue<string>();
                urls.Add(url);
                ids.Add($"id{item["id"]}");
            }

            var images = DownloadImages(urls);

            List<byte[]> dataList = new List<byte[]>();
            foreach (var image in images)
            {
                dataList.Add(image.Value);
            }

            dto.Images.Add(Puzzle(dataList, ids, perWidth, perHeight, quality, maxCols));

            return dto;
        }

        /// <summary>
        /// 获取排行版作品
        /// </summary>
        /// <param name="mode">daily|weekly|monthly|rookie|original|daily_ai|male|female</param>
        /// <returns></returns>
        public PixivDto Touch_GetRankIllusts(string mode)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/ranking/illust?mode={mode}&type=all&page=1", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            if (!IsExist(json_result, "ranking"))
            {
                dto.Message = "没有相关的作品";
                return dto;
            }

            var ranking = json_result["body"]["ranking"].AsArray().Take(10);
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

            return HandleImages(dto, illusts, 10, true, 360, 360, 90, 5);
        }

        /// <summary>
        /// 获取关键字作品的预览图
        /// </summary>
        /// <param name="word">关键字</param>
        /// <returns></returns>
        public PixivDto Touch_GetIllustsPreview(string word)
        {
            PixivDto dto = new PixivDto();

            var request = new RestRequest($"https://www.pixiv.net/touch/ajax/search/illusts?include_meta=1&s_mode=s_tag&type=all&word={word}", Method.Get);
            RestResponse response = ExecuteAndRetry(request);
            JsonNode json_result = Json.ToJsonNode(response.Content);
            if (!IsExist(json_result, "illusts"))
            {
                dto.Message = "没有搜到相关的作品";
                return dto;
            }

            var illusts = json_result["body"]["illusts"].AsArray();
            return HandleImages(dto, illusts, illusts.Count, false, 360, 360, 90, 6);
        }

        /// <summary>
        /// 获取用户作品的预览图
        /// </summary>
        /// <param name="nick">用户名</param>
        /// <param name="user_id">用户id，如传入则无视nick字段</param>
        /// <param name="tag">作品标签</param>
        /// <returns></returns>
        public PixivDto Touch_GetUserIllustsPreview(string nick, string user_id = "", string tag = "")
        {
            PixivDto dto = new PixivDto();

            var illusts = string.IsNullOrEmpty(user_id)
                ? Touch_SearchUserIllusts(nick, tag, out string error)
                : Touch_GetUserIllusts(user_id, tag, out error);
            if (illusts == null)
            {
                dto.Message = error;
                return dto;
            }

            return HandleImages(dto, illusts, illusts.Count, false, 360, 360, 90, 6);
        }
    }
}

