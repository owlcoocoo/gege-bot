namespace CQHttp.DTOs
{
    public class CQEventGroupBan : CQEventNotice
    {
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 操作者 QQ 号
        /// </summary>
        public long operator_id { get; set; }
        /// <summary>
        /// 被禁言 QQ 号 (为全员禁言时为0)
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 禁言时长, 单位秒 (为全员禁言时为-1)
        /// </summary>
        public long duration { get; set; }
    }
}
