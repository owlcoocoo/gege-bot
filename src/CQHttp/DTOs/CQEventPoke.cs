namespace CQHttp.DTOs
{
    public class CQEventPoke : CQEventNotice
    {
        /// <summary>
        /// 发送者 QQ 号
        /// </summary>
        public long sender_id { get; set; }
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 发送者 QQ 号
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 被戳者 QQ 号
        /// </summary>
        public long target_id { get; set; }
    }
}
