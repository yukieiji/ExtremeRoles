
using System;
using System.Reflection;
using UnityEngine;

class Program
{
    static void Main()
    {
        var asm = typeof(UnityEngine.Object).Assembly;
        foreach (var t in asm.GetTypes())
        {
            if (t.Name.StartsWith("MockObjectInstantiateHelper"))
            {
                var instanceProp = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                Console.WriteLine($"{t.Name}:");
                foreach (var m in t.GetMethods())
                {
                    if (m.Name == "Invoke")
                    {
                        var ps = string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name));
                        Console.WriteLine($"  Invoke({ps})");
                    }
                }
            }
        }
    }
}
