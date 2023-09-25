using System.Text.Json;
using System.Text.Json.Nodes;

namespace CQHttp.DTOs
{
    public class CQResponse
    {
        public string status { get; set; }
        public int retcode { get; set; }
        public string msg { get; set; }
        public string wording { get; set; }
        public JsonObject data { get; set; }
        public string echo { get; set; }

        public void FireCallBack(CQAPIContext context)
        {
            if (context.CallBack == null || context.CallBackType == null) return;
            object obj = null;
            if (retcode == 0) obj = data.Deserialize(context.CallBackType);
            context.CallBack.Invoke(context.CallBackTarget, new[] { obj });
        }
    }
}
