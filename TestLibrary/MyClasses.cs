using System;
using System.Collections.Generic;

namespace TestLibrary.NS1
{
    public class MyClass1
    {
        public int PublicField;
        private string _privateField;
        protected bool ProtectedField;
        internal double InternalField;

        public static int StaticField;

        public MyClass1(string pf)
        {
            _privateField = pf;
        }

        public void PublicMethod(int a) { Console.WriteLine($"PublicMethod called with {a}"); }
        private void PrivateMethod() { Console.WriteLine("PrivateMethod called"); }
        protected string ProtectedMethod() { return "ProtectedMethod"; }
        internal static void InternalStaticMethod() { Console.WriteLine("InternalStaticMethod called"); }

        public static string StaticMethod(string input) { return $"Static: {input}"; }

        public int PublicProperty { get; set; }
        private string PrivateProperty { get; } = "PrivateValue";
        public string ReadOnlyProperty => _privateField + PrivateProperty; // Example of expression-bodied property
    }

    public static class MyStaticClass
    {
        public static string StaticConfig = "InitialConfig";
        private static int StaticCounter = 0;

        public static void Configure(string config)
        {
            StaticConfig = config;
        }

        public static int IncrementCounter()
        {
            StaticCounter++;
            return StaticCounter;
        }
    }

    public class GenericClass<T, U> where T : class
    {
        public T GenericField;
        private U _genericPrivateField;

        public GenericClass(T gf, U gpf)
        {
            GenericField = gf;
            _genericPrivateField = gpf;
        }

        public T GetGenericField() { return GenericField; }
        public void SetPrivateGenericField(U value) { _genericPrivateField = value; }
        public static void StaticGenericMethod<K>(K param) { Console.WriteLine($"StaticGenericMethod with {param}");}

        public List<T> GetList() { return new List<T>(); }
        public Dictionary<string, U> GetDictionary() { return new Dictionary<string, U>(); }

    }

    namespace NestedNS
    {
        public class OuterClass
        {
            public int OuterField;

            public class NestedClass
            {
                public string NestedField;
                public void NestedMethod() { Console.WriteLine("NestedMethod"); }

                public class DoublyNestedClass
                {
                    public bool DoublyNestedField;
                }
            }

            public static class NestedStaticClass
            {
                public static int NestedStaticField;
            }
        }
    }
}

namespace Another.NS
{
    public class AnotherClass
    {
        public System.Threading.Tasks.Task<string> GetAsyncTask()
        {
            return System.Threading.Tasks.Task.FromResult("async result");
        }

        public void MethodWithKeywords(int @int, string @string, object @event)
        {
             Console.WriteLine($"{@int} {@string} {@event}");
        }
    }
}
