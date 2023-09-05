namespace CQHttp.DTOs
{
    public class CQEventLifecycle : CQEventBase
    {
        /// <summary>
        /// 子类型
        /// <para>enable, disable, connect</para>
        /// </summary>
        public string sub_type { get; set; }
    }
}
