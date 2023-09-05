namespace CQHttp.DTOs
{
    public class CQEventMessagePrivate : CQEventMessage
    {
        /// <summary>
        /// 接收者 QQ 号
        /// </summary>
        public long target_id { get; set; }

        /// <summary>
        /// 临时会话来源
        /// </summary>
        public int temp_source { get; set; }
    }
}
