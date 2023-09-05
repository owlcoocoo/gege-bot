namespace CQHttp.DTOs
{
    public class CQEventQuickOperation_PrivateMessage
    {
        /// <summary>
        /// 要回复的内容
        /// </summary>
        public string reply { get; set; }
        /// <summary>
        /// 消息内容是否作为纯文本发送 ( 即不解析 CQ 码 ) , 只在 reply 字段是字符串时有效
        /// </summary>
        public bool auto_escape { get; set; }
    }

    public class CQEventQuickOperation_GroupMessage
    {
        /// <summary>
        /// 要回复的内容
        /// </summary>
        public string reply { get; set; }
        /// <summary>
        /// 消息内容是否作为纯文本发送 ( 即不解析 CQ 码 ) , 只在 reply 字段是字符串时有效
        /// </summary>
        public bool auto_escape { get; set; }
        /// <summary>
        /// 是否要在回复开头 at 发送者 ( 自动添加 ) , 发送者是匿名用户时无效
        /// </summary>
        public bool at_sender { get; set; }
        /// <summary>
        /// 撤回该条消息
        /// </summary>
        public bool delete { get; set; }
        /// <summary>
        /// 把发送者踢出群组 ( 需要登录号权限足够 ) , 不拒绝此人后续加群请求, 发送者是匿名用户时无效
        /// </summary>
        public bool kick { get; set; }
        /// <summary>
        /// 禁言该消息发送者, 对匿名用户也有效
        /// </summary>
        public bool ban { get; set; }
        /// <summary>
        /// 若要执行禁言操作时的禁言时长，默认 60 秒。
        /// </summary>
        public int ban_duration { get; set; } = 60;
    }
}
