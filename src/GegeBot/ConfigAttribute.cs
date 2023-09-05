namespace GegeBot
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : Attribute
    {
        public string Name { get; private set; }

        public ConfigAttribute(string name)
        {
            Name = name;
        }
    }
}
