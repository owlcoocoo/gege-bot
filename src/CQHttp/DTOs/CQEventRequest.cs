namespace CQHttp.DTOs
{
    public class CQEventRequest
    {
        /// <summary>
        /// 请求类型
        /// </summary>
        public string request_type { get; set; }
        /// <summary>
        /// 发送请求的 QQ 号
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 请求子类型, 分别表示加群请求、邀请登录号入群
        /// </summary>
        public string sub_type { get; set; }
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 验证信息
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// 请求 flag, 在调用处理请求的 API 时需要传入
        /// </summary>
        public string flag { get; set; }
    }

}
