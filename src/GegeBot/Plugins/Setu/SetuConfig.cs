namespace GegeBot.Plugins.Setu
{
    [Config("Setu")]
    internal class SetuConfig
    {
        /// <summary>
        /// setu撤回时间（秒），0不撤回
        /// </summary>
        public static int DeleteSeconds { get; set; }
    }
}
