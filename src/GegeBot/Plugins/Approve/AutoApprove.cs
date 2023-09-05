using CQHttp;
using CQHttp.DTOs;

namespace GegeBot.Plugins.Approve
{
    [Plugin]
    internal class AutoApprove
    {
        readonly CQBot cqBot;

        public AutoApprove(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedRequest += CqBot_ReceivedRequest;
        }

        private void CqBot_ReceivedRequest(CQEventRequest obj)
        {
            if (ApproveConfig.AutoApproveFriendAdd && obj.request_type == CQRequestType.Friend)
            {
                if (!string.IsNullOrEmpty(obj.flag))
                {
                    cqBot.Friend_SetFriendAddRequest(obj.flag, true);
                }
            }
            else if (ApproveConfig.AutoApproveGroupInvite && obj.request_type == CQRequestType.Group)
            {
                if (obj.sub_type == CQRequestGroupSubType.Invite)
                {
                    if (!string.IsNullOrEmpty(obj.flag) && !string.IsNullOrEmpty(obj.sub_type))
                    {
                        cqBot.Group_SetGroupAddRequest(obj.flag, obj.sub_type, true);
                    }
                }
            }
        }
    }
}
