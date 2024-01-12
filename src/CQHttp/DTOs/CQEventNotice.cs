namespace CQHttp.DTOs
{
    public class CQEventNotice : CQEventBase
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// 提示类型
        /// </summary>
        public string sub_type { get; set; }
    }
}
