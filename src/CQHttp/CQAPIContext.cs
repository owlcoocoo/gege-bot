using CQHttp.DTOs;
using System;
using System.Reflection;

namespace CQHttp
{
    public class CQAPIContext
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public CQRequest Request { get; set; }
        public MethodInfo CallBack { get; set; }
        public Type CallBackType { get; set; }
        public object CallBackTarget { get; set; }

        public CQAPIContext(CQRequest request)
        {
            Request = request;
        }

        public void SetCallBack<T>(Action<T> callback)
        {
            if (callback == null) return;

            CallBack = callback.Method;
            CallBackTarget = callback.Target;
            CallBackType = typeof(T);
        }
    }
}
