namespace CQHttp.DTOs
{
    public class CQEventMessageGroup : CQEventMessage
    {
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 匿名信息, 如果不是匿名消息则为 null
        /// </summary>
        public object anonymous { get; set; }
    }
}
