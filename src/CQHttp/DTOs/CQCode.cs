using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CQHttp.DTOs
{
    public class CQCode : IDisposable
    {
        private class CQCodeModel
        {
            public string type { get; set; } = "";
            public JsonObject data { get; set; } = new JsonObject();
        }

        private List<CQCodeModel> data = new List<CQCodeModel>();


        public CQCode()
        { }

        public CQCode(string text)
        {
            SetText(text);
        }

        /// <summary>
        /// 解析 CQ 码成文本
        /// </summary>
        /// <param name="cqCode">CQ 码</param>
        /// <param name="atQQ">被@的QQ列表</param>
        /// <returns></returns>
        public static string GetText(string cqCode, out List<string> atQQ)
        {
            atQQ = new List<string>();

            var matches = Regex.Matches(cqCode, "\\[(.*?)\\]|[^\\[(.*?)\\]]+");
            if (matches.Count < 1) return cqCode;

            StringBuilder sb = new StringBuilder();
            foreach (var item in matches)
            {
                if (string.IsNullOrWhiteSpace(item.ToString())) continue;

                string codeText = item.ToString()?.TrimStart('[').TrimEnd(']') ?? "";

                string[] str_array = codeText.ToString().Split(',');
                if (str_array.Length > 1)
                {
                    string[] code_array = str_array[0].ToLower().Split(':');
                    if (code_array.Length < 2) continue;

                    string code = code_array[1];
                    if (code == "at")
                    {
                        atQQ.Add(str_array[1].Split('=')[1]);
                    }
                    else if (code == "text")
                    {
                        sb.Append(str_array[1].Split('=')[1]);
                    }
                }
                else
                {
                    sb.Append(codeText);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 普通消息
        /// </summary>
        /// <param name="text">文本</param>
        public CQCode SetText(string text)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "text";
            model.data.Add("text", text);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// 普通消息
        /// </summary>
        /// <param name="qq">@的 QQ 号, all 表示全体成员</param>
        /// <param name="name">当在群中找不到此QQ号的名称时才会生效</param>
        public CQCode SetAt(string qq, string name = "")
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "at";
            model.data.Add("qq", qq);
            if (!string.IsNullOrEmpty(name))
                model.data.Add("name", name);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// QQ 表情
        /// <para>
        /// <a href="https://github.com/richardchien/coolq-http-api/wiki/%E8%A1%A8%E6%83%85-CQ-%E7%A0%81-ID-%E8%A1%A8">QQ 表情 ID 表</a>
        /// </para>
        /// </summary>
        /// <param name="id">QQ 表情 ID</param>
        public CQCode SetFace(string id)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "face";
            model.data.Add("id", id);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="file">图片文件名</param>
        public CQCode SetImage(string file)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "image";
            model.data.Add("file", file);
            model.data.Add("subType", 0);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// 语音
        /// </summary>
        /// <param name="file">语音文件名</param>
        public void SetRecord(string file)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "record";
            model.data.Add("file", file);
            data.Add(model);
        }

        /// <summary>
        /// 文本转语音
        /// </summary>
        /// <param name="text">文本</param>
        public void SetTTS(string text)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "tts";
            model.data.Add("text", text);
            data.Add(model);
        }

        /// <summary>
        /// 回复
        /// </summary>
        /// <param name="id">回复时所引用的消息id, 必须为本群消息</param>
        public CQCode SetReply(int id)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "reply";
            model.data.Add("id", id);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// 回复
        /// </summary>
        /// <param name="text">自定义回复的信息</param>
        /// <param name="qq">自定义回复时的自定义QQ, 如果使用自定义信息必须指定.</param>
        /// <param name="seq">起始消息序号, 可通过 get_msg 获得</param>
        public CQCode SetReply(string text, string qq, long seq)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "reply";
            model.data.Add("text", text);
            model.data.Add("qq", qq);
            model.data.Add("time", DateTimeOffset.Now.ToUnixTimeSeconds());
            model.data.Add("seq", seq);
            data.Add(model);
            return this;
        }

        /// <summary>
        /// 戳一戳
        /// </summary>
        /// <param name="qq">需要戳的成员</param>
        public CQCode SetPoke(long qq)
        {
            CQCodeModel model = new CQCodeModel();
            model.type = "poke";
            model.data.Add("qq", qq);
            data.Add(model);
            return this;
        }

        public void Clear()
        {
            foreach (var item in data)
            {
                item.data.Clear();
                item.data = null;
            }
            data.Clear();
        }

        private string Format(string text)
        {
            return text.Replace(",", "&#44;").Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&&#93;");
        }

        /// <summary>
        /// 返回文本格式的 CQ 码
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public string ToText()
        {
            StringBuilder sb = new StringBuilder();

            foreach (CQCodeModel model in data)
            {
                sb.Append($"[CQ:{model.type}");

                foreach (var jObj in model.data)
                {
                    sb.Append($",{jObj.Key}={Format(jObj.Value?.ToString() ?? "")}");
                }

                sb.Append("]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 返回 JSON 格式的 CQ 码
        /// </summary>
        /// <returns></returns>
        public JsonNode ToJson()
        {
            return Json.ToJsonNode(data);
        }

        public void Dispose()
        {
            Clear();
            data = null;
        }
    }
}
