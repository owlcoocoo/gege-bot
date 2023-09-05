using CQHttp.Attributes;
using System.Linq;
using System.Reflection;
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

        public void FireEvent()
        {
            var exeTypes = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.Attributes.HasFlag(TypeAttributes.Public));
            foreach (var t in exeTypes)
            {
                foreach (var method in t.GetMethods().Where(a => a.IsPublic && !a.IsConstructor))
                {
                    bool isDefined = method.IsDefined(typeof(CQAPIAttribute), false);
                    if (isDefined)
                    {
                        object[] attrs = method.GetCustomAttributes(false);
                        foreach (var a in attrs)
                        {
                            if (a is CQAPIAttribute cqAPI)
                            {

                            }
                        }
                    }
                }

            }
        }

        public void FireCallBack(CQAPIContext context)
        {
            if (context.CallBack == null || context.CallBackType == null || retcode != 0) return;
            var obj = data.Deserialize(context.CallBackType);
            if (obj == null) return;
            context.CallBack.Invoke(context.CallBackTarget, new[] { obj });
        }
    }

}
