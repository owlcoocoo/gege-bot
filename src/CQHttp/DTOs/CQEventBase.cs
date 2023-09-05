namespace CQHttp.DTOs
{
    public class CQEventBase
    {
        /// <summary>
        /// 事件发生的unix时间戳
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 收到事件的机器人的 QQ 号
        /// </summary>
        public long self_id { get; set; }

        /// <summary>
        /// 表示该上报的类型
        /// <para>message, message_sent, request, notice, meta_event</para>
        /// <para>消息, 消息发送, 请求, 通知, 或元事件</para>
        /// </summary>
        public string post_type { get; set; }
    }
}
