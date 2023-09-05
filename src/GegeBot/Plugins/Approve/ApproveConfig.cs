namespace GegeBot.Plugins.Approve
{
    [Config("Approve")]
    internal class ApproveConfig
    {
        public static bool AutoApproveFriendAdd { get; set; }
        public static bool AutoApproveGroupInvite { get; set; }

    }
}
