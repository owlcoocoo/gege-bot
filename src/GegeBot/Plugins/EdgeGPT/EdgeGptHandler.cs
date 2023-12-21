using CQHttp;
using CQHttp.DTOs;
using GegeBot.Plugins.LlamaCpp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GegeBot.Plugins.EdgeGPT
{
    internal class EdgeGptHandler : IPlugin
    {
        readonly CQBot cqBot;
        readonly Log log = new Log("edge_gpt");

        readonly Dictionary<string, EdgeGptAPI> edgeGptAPISessions = new();
        readonly Hashtable dictionary = new Hashtable();

        public EdgeGptHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedMessage += CqBot_ReceivedMessage; ;

            Reload();
        }

        public void Reload()
        {
            if (!EdgeGptConfig.Enable) return;

            dictionary.Clear();

            foreach (string file in EdgeGptConfig.DictFile)
            {
                Console.WriteLine($"[EdgeGpt]加载词库 {file}");

                string data = File.ReadAllText(file);
                var jsonArray = Json.ToJsonNode(data).AsArray();
                foreach (var item in jsonArray)
                {
                    dictionary.Add(item.ToString(), null);
                }
            }

            Console.WriteLine($"[EdgeGpt]词库加载完毕，共计 {dictionary.Count} 条。");
        }

        record GenImages(string Text, List<string> Images);

        string GetAskResultContent(JsonNode jsonNode, out GenImages genImages, bool isSource = false)
        {
            string content = "";
            genImages = null;

            var messages = jsonNode["response"]["item"]["messages"].AsArray();
            foreach (var msg in messages)
            {
                string author = msg["author"].GetValue<string>();
                if (author == "bot")
                {
                    if (msg["messageType"] == null)
                    {
                        if (isSource)
                        {
                            content = msg["adaptiveCards"][0]["body"][0]["text"].GetValue<string>();
                            content = content.TrimEnd('\n');
                        }
                        else
                        {
                            content = msg["text"].GetValue<string>();
                        }
                    }
                    else
                    {
                        string messageType = msg["messageType"].GetValue<string>();
                        if (messageType == "GenerateContentQuery")
                        {
                            genImages = new GenImages(msg["text"].GetValue<string>(), new List<string>());
                        }
                    }
                }
            }

            return content;
        }

        void InitFirstPrompt(EdgeGptAPI api)
        {
            if (string.IsNullOrWhiteSpace(EdgeGptConfig.PromptFilePath) || !File.Exists(EdgeGptConfig.PromptFilePath))
                return;

            string prompt = File.ReadAllText(EdgeGptConfig.PromptFilePath);

            if (string.IsNullOrWhiteSpace(prompt)) return;

            var result = api.Ask(prompt);
            string content = GetAskResultContent(result, out _);

            if (EdgeGptConfig.WriteMessageLog)
                log.WriteInfo($"\nfirst:{prompt}\nbot:{content}");
        }

        EdgeGptAPI GetEdgeGptAPI(CQEventMessageEx msg)
        {
            string key = BotSession.GetSessionKey(msg);
            if (edgeGptAPISessions.TryGetValue(key, out var api))
                return api;
            api = new EdgeGptAPI(EdgeGptConfig.ServerAddress);
            string cookies = File.ReadAllText(EdgeGptConfig.CookieFilePath);
            api.Create(key, cookies, EdgeGptConfig.Proxy);
            InitFirstPrompt(api);
            edgeGptAPISessions.Add(key, api);
            return api;
        }

        string Ask(CQEventMessageEx msg, EdgeGptAPI api, string prompt, out GenImages genImages, string imageUrl = null)
        {
            var result = api.Ask(prompt, imageUrl: imageUrl);
            int code = result["code"].GetValue<int>();
            if (code == -1)
            {
                string key = BotSession.GetSessionKey(msg);
                string cookies = File.ReadAllText(EdgeGptConfig.CookieFilePath);
                api.Create(key, cookies, EdgeGptConfig.Proxy);
                InitFirstPrompt(api);
            }
            string content = GetAskResultContent(result, out genImages);
            return content;
        }

        void Reset(EdgeGptAPI api)
        {
            api.Reset();
            InitFirstPrompt(api);
        }

        void DownloadImages(EdgeGptAPI api, ref GenImages genImages)
        {
            try
            {
                if (genImages != null)
                {
                    var images_result = api.GenerateImages(EdgeGptConfig.ImagesAuthCookie, genImages.Text, EdgeGptConfig.GenerateImageProxy).AsArray();
                    List<string> urls = new List<string>();
                    foreach (var image in images_result)
                    {
                        urls.Add(image.GetValue<string>());
                    }
                    var images_data = api.DownloadImages(urls);
                    foreach (var img in images_data)
                    {
                        string base64Img = $"base64://{Convert.ToBase64String(img.Value)}";
                        genImages.Images.Add(base64Img);
                    }
                }
            }
            catch { }
        }

        private void CqBot_ReceivedMessage(CQEventMessageEx obj)
        {
            if (!EdgeGptConfig.Enable) return;

            string text = CQCode.GetText(obj.message, out var atList, out var imageList).TrimStart();

            foreach (string keyword in EdgeGptConfig.FilterText)
            {
                if (text.StartsWith(keyword))
                {
                    return;
                }
            }

            bool isKeyword = EdgeGptConfig.ReplyProbability > 0 && dictionary.Count > 0
                             && !string.IsNullOrWhiteSpace(text) && dictionary.ContainsKey(text);
            if (isKeyword)
            {
                double num = new Random().NextDouble();
                if (num >= EdgeGptConfig.ReplyProbability)
                    isKeyword = false;
            }

            if (!isKeyword && obj.message_type == CQMessageType.Group &&
                (!atList.Any() || !atList.Contains(obj.self_id.ToString())))
                return;

            EdgeGptAPI edgeGptAPI = GetEdgeGptAPI(obj);

            if (!string.IsNullOrEmpty(EdgeGptConfig.ResetCommand) && text == EdgeGptConfig.ResetCommand)
            {
                Reset(edgeGptAPI);
                if (!string.IsNullOrEmpty(EdgeGptConfig.ResetMessage))
                    cqBot.Message_QuickReply(obj, new CQCode().SetReply(obj.message_id).SetText(EdgeGptConfig.ResetMessage));
                return;
            }

            int retryCounter = 0;

        RETRY:

            string userName = obj.sender.nickname;
            string content = "";
            GenImages genImages = null;
            try
            {
                string imgUrl = imageList.FirstOrDefault();
                content = Ask(obj, edgeGptAPI, $"{userName}说：{text}", out genImages, imgUrl);
            }
            catch (Exception ex)
            {
                log.WriteError(ex.ToString());
            }

            if (string.IsNullOrWhiteSpace(content) && retryCounter < 1)
            {
                Reset(edgeGptAPI);
                retryCounter++;
                goto RETRY;
            }

            if (EdgeGptConfig.WriteMessageLog)
                log.WriteInfo($"\n{userName}:{text}\nbot:{content}");

            if (string.IsNullOrWhiteSpace(content))
                content += $"\n检测到Bot无法回复，如持续出现此问题，请对我说“{EdgeGptConfig.ResetCommand}”。";

            CQCode cqCode = new CQCode();
            cqCode.SetReply(obj.message_id);
            cqCode.SetText(content);
            cqBot.Message_QuickReply(obj, cqCode);

            if (genImages != null)
            {
                Task.Delay(100 * new Random().Next(5, 10)).Wait();

                cqCode.Clear();
                cqCode.SetReply(obj.message_id);
                cqCode.SetText("正在生成图片请稍等...");
                cqBot.Message_QuickReply(obj, cqCode);

                DownloadImages(edgeGptAPI, ref genImages);

                cqCode.Clear();
                cqCode.SetReply(obj.message_id);
                if (genImages.Images.Count > 0)
                {
                    foreach (string img in genImages.Images)
                    {
                        cqCode.SetImage(img);
                    }
                }
                else
                {
                    cqCode.SetText("生成图片失败！");
                }
                cqBot.Message_QuickReply(obj, cqCode);
            }
        }
    }
}
