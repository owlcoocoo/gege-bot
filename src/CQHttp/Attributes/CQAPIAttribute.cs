using System;

namespace CQHttp.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CQAPIAttribute : Attribute
    {
        public string Action { get; private set; }

        public CQAPIAttribute(string action)
        {
            Action = action;
        }
    }
}
