using System;

namespace AopProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseClass = AopProxyFactory.Create<BaseClass, LogAttribute>(
                () => Console.WriteLine("Before"),
                () => Console.WriteLine("After"),
                () => Console.WriteLine("Error")
                );
            baseClass.ProxiedMethod();
            baseClass.NotProxiedMethod();
        }
    }


    public class LogAttribute : Attribute { }

    public class BaseClass : MarshalByRefObject
    {
        [Log]
        public void ProxiedMethod()
        {
            Console.WriteLine("Вызов проксированного метода");
        }

        public void NotProxiedMethod()
        {
            Console.WriteLine("Вызов НЕ проксированного метода");
        }
    }
}
