using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace AopProxy
{
    public delegate void BeforeExecute();
    public delegate void AfterExecute();
    public delegate void ErrorExecuting();

    public class DynamicProxy<T> : RealProxy
    {
        private readonly T _decorated;
        private Predicate<MethodInfo> _filter;
        public BeforeExecute BeforeExecute { get; set; }
        public AfterExecute AfterExecute { get; set; }
        public ErrorExecuting ErrorExecuting { get; set; }
        public DynamicProxy(T decorated): base(typeof(T))
        {
            _decorated = decorated;
            _filter = m =>  true;
        }
        public Predicate<MethodInfo> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                    _filter = m => true;
                else
                    _filter = value;
            }
        }

        public override IMessage Invoke(IMessage msg)
        {
            // ReSharper disable PossibleNullReferenceException
            var methodCall = msg as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;
            if (_filter(methodInfo))
            {
                if (BeforeExecute != null) BeforeExecute();
            }
            try
            {
                var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                if (_filter(methodInfo))
                {
                    if (AfterExecute != null) AfterExecute();
                }
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                if (_filter(methodInfo))
                {
                    if (ErrorExecuting != null) ErrorExecuting();
                }
                return new ReturnMessage(e, methodCall);
            }
            // ReSharper restore PossibleNullReferenceException
        }
    }

    public class AopProxyFactory
    {
        public static T Create<T,TA>(BeforeExecute beforeExecute, AfterExecute afterExecute, ErrorExecuting errorExecuting) where T : class, new()
        {
            var decorated = new T();
            var dynamicProxy = new DynamicProxy<T>(decorated)
            {
                Filter = m => m.GetCustomAttributes(typeof(TA), true).Any(),
                BeforeExecute = beforeExecute,
                AfterExecute = afterExecute,
                ErrorExecuting = errorExecuting
            };
            return dynamicProxy.GetTransparentProxy() as T;
        }
    }
}