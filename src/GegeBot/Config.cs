using CQHttp;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GegeBot
{
    internal class Config
    {
        private readonly string filePath;

        public Config(string filepath)
        {
            filePath = filepath;
        }

        IEnumerable<Type> GetAllConfigs()
        {
            var asmTypes = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.IsClass);
            var implClasses = asmTypes.Where(a => a.GetCustomAttribute<ConfigAttribute>() != null);
            return implClasses;
        }

        public void Load()
        {
            string data = File.ReadAllText(filePath);
            if (data == null)
            {
                Console.WriteLine($"[配置]读取失败！");
                return;
            }

            JsonObject jsonObj = Json.ToJsonNode(data).AsObject();

            foreach (var t in GetAllConfigs())
            {
                object[] attrs = t.GetCustomAttributes(false);
                foreach (var a in attrs)
                {
                    if (a is ConfigAttribute config)
                    {
                        Console.WriteLine($"[配置]加载 {config.Name}");
                        try
                        {
                            jsonObj.TryGetPropertyValue(config.Name, out var jsonNode);
                            if (jsonNode == null) continue;

                            foreach (var node in jsonNode.AsObject())
                            {
                                var property = t.GetProperty(node.Key);
                                property.SetValue(property, node.Value.Deserialize(property.PropertyType));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[配置]加载失败 {config.Name}，{ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine($"[配置]加载完毕");
        }
    }
}
