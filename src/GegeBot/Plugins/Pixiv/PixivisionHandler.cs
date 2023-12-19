using CQHttp;
using CQHttp.DTOs;

namespace GegeBot.Plugins.Pixiv
{
    [Plugin]
    public class PixivisionHandler : IPlugin
    {
        readonly CQBot cqBot;
        readonly PixivAPI pixivAPI = new PixivAPI();
        readonly PixivisionAPI pixivisionAPI = new PixivisionAPI();

        readonly Log log = new Log("pixivision");

        Task pushTask = null;

        public PixivisionHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedMessage += CqBot_ReceivedMessage;

            Reload();
        }

        public void Reload()
        {
            if (!PixivConfig.EnablePixivisionPush) return;

            if (pushTask == null)
            {
                pushTask = Task.Factory.StartNew(FetchIllustration, TaskCreationOptions.LongRunning);
                pushTask.ContinueWith((t) =>
                {
                    pushTask = null;
                    if (t.Exception != null)
                    {
                        log.WriteError(t.Exception.ToString());

                        Task.Delay(1000 * 60 * PixivConfig.PixivisionFetchInterval).Wait();

                        Reload();
                    }
                });
            }
        }

        private int GetIllustIndex(string id, long group_id, out string key)
        {
            key = "";
            int index = 0;
            if (!string.IsNullOrEmpty(id))
            {
                key = $"a{id}_{group_id}";
                int.TryParse(PixivisionDb.Db.GetValue(key), out index);
            }
            return index;
        }

        private void SaveIllustIndex(string key, int index, int imageCount)
        {
            if (!string.IsNullOrEmpty(key))
            {
                index += PixivConfig.MaxImages;
                if (index >= imageCount) index = 0;
                PixivisionDb.Db.SetValue(key, index.ToString());
            }
        }

        private void FetchIllustration()
        {
            while (PixivConfig.EnablePixivisionPush)
            {
                var pixivisionList = pixivisionAPI.GetIllustrationArticleList(1, DateTime.Now);
                foreach (var pixivision in pixivisionList)
                {
                    var article = pixivisionAPI.GetIllustrationArticle(pixivision.Url);
                    if (article == null) continue;

                    string key = $"a{pixivision.Id}";
                    PixivisionModel model = null;
                    string value = PixivisionDb.Db.GetValue(key);
                    if (string.IsNullOrEmpty(value))
                    {
                        model = new PixivisionModel();
                        model.Id = pixivision.Id;
                        model.Url = pixivision.Url;
                        model.Article = article;
                        model.Article.Thumbnail = pixivision.Thumbnail;
                        PixivisionDb.Db.SetValue(key, Json.ToJsonString(model));
                    }

                    CQGroupInfo[] groupInfos = cqBot.Group_GetGroupListSync().Result;
                    if (PixivConfig.PixivisionGroupWhiteList.Any())
                    {
                        groupInfos = groupInfos.Where(g => PixivConfig.PixivisionGroupWhiteList.Exists(w => g.group_id == w)).ToArray();
                    }

                    string imageBase64 = "";

                    foreach (var groupInfo in groupInfos)
                    {
                        key = $"{DateTime.Now:yyyyMMdd}_{pixivision.Id}_{groupInfo.group_id}_push";
                        value = PixivisionDb.Db.GetValue(key);
                        if (string.IsNullOrEmpty(value))
                        {
                            Console.WriteLine($"[pixivision]推送{pixivision.Id} - {groupInfo.group_id}");

                            if (string.IsNullOrEmpty(imageBase64))
                            {
                                var image = pixivAPI.DownloadImages(new List<string>() { pixivision.Thumbnail })[0];
                                imageBase64 = $"base64://{Convert.ToBase64String(image.Value)}";
                            }

                            CQCode cqCode = new CQCode();
                            cqCode.SetText($"{article.Title}\n\n");
                            cqCode.SetImage(imageBase64);
                            cqCode.SetText($"\n\n{article.Text}");
                            cqCode.SetText($"想看插画特辑就发送“pvck{pixivision.Id}”吧~");

                            CQRequestMessage requestMessage = new CQRequestMessage();
                            requestMessage.group_id = groupInfo.group_id;
                            requestMessage.message_type = CQMessageType.Group;
                            requestMessage.message = cqCode.ToJson();
                            cqBot.Message_SendMsg(requestMessage);

                            PixivisionDb.Db.SetValue(key, "ok");
                        }
                    }
                }

                Task.Delay(1000 * 60 * PixivConfig.PixivisionFetchInterval).Wait();
            }
        }

        private void CqBot_ReceivedMessage(CQEventMessageEx obj)
        {
            if (obj.message_type != CQMessageType.Group)
                return;

            if (PixivConfig.PixivisionGroupWhiteList.Any() && !PixivConfig.PixivisionGroupWhiteList.Exists(g => g == obj.group_id))
                return;

            string text = CQCode.GetText(obj.message, out var atList, out _).TrimStart();
            if (atList.Count > 0 && !atList.Contains(obj.self_id.ToString())) return;

            if (text.StartsWith("pv"))
            {
                Console.WriteLine($"[pixivision]处理 {text}");

                string keyword = text[2..];

                if (keyword.StartsWith("ck"))
                {
                    keyword = keyword[2..];

                    string key = $"a{keyword}";
                    string value = PixivisionDb.Db.GetValue(key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        var model = Json.FromJsonString<PixivisionModel>(value);

                        int index = GetIllustIndex(keyword, obj.group_id, out key);
                        var images = model.Article.Images.Skip(index).Take(PixivConfig.MaxImages).ToArray();
                        List<string> urls = new List<string>();
                        foreach (var item in images)
                        {
                            urls.Add(item.ImageUrl);
                        }
                        var imageDataList = pixivAPI.DownloadImages(urls);

                        CQCode cqCode = new CQCode();
                        cqCode.SetText($"已发送 {imageDataList.Length + index} 个特辑，共 {model.Article.Images.Count} 个特辑。\n\n");

                        for (int i = 0; i < images.Length; i++)
                        {
                            var image = images[i];
                            cqCode.SetText($"{image.ImageTitle} - {image.UserName}\n");
                            cqCode.SetText($"作品id{image.ImageId} - 画师id{image.UserId}\n");
                            cqCode.SetImage($"base64://{Convert.ToBase64String(imageDataList[i].Value)}");

                            if (i < images.Length - 1)
                                cqCode.SetText($"\n\n");
                        }

                        if (index + PixivConfig.MaxImages < model.Article.Images.Count) 
                        {
                            cqCode.SetText($"\n\n发送“pvck{keyword}”继续查看。");
                        }

                        Console.WriteLine($"[pixivision]发送消息");

                        cqBot.Message_QuickReply(obj, cqCode);

                        SaveIllustIndex(key, index, model.Article.Images.Count);
                    }
                }
            }
        }
    }
}
