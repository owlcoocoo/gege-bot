using System.Text.Json.Nodes;

namespace CQHttp.DTOs
{
    public class CQRequest
    {
        public string action { get; set; }
        public JsonNode @params { get; set; }
        public string echo { get; set; }

        public CQRequest(string action)
        {
            this.action = action;
        }
    }
}
