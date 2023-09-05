using CQHttp.DTOs;
using System.Collections.Generic;

namespace CQHttp
{
    public class CQSession
    {
        public CQEventMessageEx CurrentMessage { get; set; }

        public Queue<CQEventMessageEx> MessageQueue { get; set; } = new Queue<CQEventMessageEx>();
    }
}
