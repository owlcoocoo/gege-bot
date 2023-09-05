namespace GegeBot.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class PluginAttribute : Attribute
    {
        public string Name { get; private set; }

        public PluginAttribute()
        {
        }

        public PluginAttribute(string name)
        {
            Name = name;
        }
    }
}
