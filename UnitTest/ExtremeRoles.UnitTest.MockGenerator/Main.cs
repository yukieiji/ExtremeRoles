using System;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks; // For GetAllTypes and other helpers
using System.Collections.Generic; // Required for List<T>

// Add helper methods for checking type attributes if not directly available
// For example, checking for CompilerGeneratedAttribute
public static class TypeDefinitionExtensions
{
    public static bool IsCompilerGenerated(this TypeDefinition type)
    {
        return type.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
    }

    public static bool IsAnonymousType(this TypeDefinition type)
    {
        // Anonymous types are compiler-generated and have specific naming patterns.
        // This is a common check, but might need refinement.
        return type.IsCompilerGenerated() && type.Name.Contains("AnonymousType");
    }

    // Helper extension method to check if a TypeDefinition is a delegate
    public static bool IsDelegate(this TypeDefinition type)
    {
        if (type == null || type.BaseType == null)
            return false;
        return type.BaseType.FullName == "System.MulticastDelegate";
    }
}


namespace ExtremeRoles.UnitTest.MockGenerator
{
    public static class Generator
    {
        public static void GenerateMocks(string[] assemblyPaths, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var typesByNamespace = new Dictionary<string, List<TypeDefinition>>();

            foreach (string assemblyPath in assemblyPaths)
            {
                if (!File.Exists(assemblyPath))
                {
                    Console.WriteLine($"Warning: Assembly not found at {assemblyPath}. Skipping.");
                    continue;
                }

                Console.WriteLine($"Loading assembly: {assemblyPath}");
                AssemblyDefinition assembly;
                try
                {
                    var resolver = new DefaultAssemblyResolver();
                    var assemblyDir = Path.GetDirectoryName(assemblyPath);
                    if (!string.IsNullOrEmpty(assemblyDir))
                    {
                        resolver.AddSearchDirectory(assemblyDir);
                    }
                    // Add common search directories if needed, e.g., NuGet package paths or specific framework paths
                    // This might be necessary if the DLL has dependencies not in its own directory.

                    var readerParameters = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = false, ReadSymbols = false };
                    assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
                }
                catch (BadImageFormatException ex)
                {
                    Console.WriteLine($"Warning: Could not load {assemblyPath} as a valid .NET assembly. Skipping. Error: {ex.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading assembly {assemblyPath}: {ex.Message}. Skipping.");
                    continue;
                }

                Console.WriteLine($"Processing types in assembly: {assembly.FullName}");

                foreach (TypeDefinition type in assembly.MainModule.GetAllTypes())
                {
                    if (!type.IsClass || type.IsInterface || type.IsEnum || type.IsValueType ||
                        type.Name.Contains("<") || type.Name.Contains("$") || // Basic check for compiler-generated
                        type.IsAnonymousType() || type.IsCompilerGenerated() ||
                        type.IsDelegate())
                    {
                        continue;
                    }

                    if (type.Namespace != null && (type.Namespace.StartsWith("System") || type.Namespace.StartsWith("Microsoft") || type.Namespace.StartsWith("Mono") || type.Namespace.StartsWith("Unity") || type.Namespace.StartsWith("UnityEngine") || type.Namespace.StartsWith("TMPro") || type.Namespace.StartsWith("Il2Cpp")))
                    {
                        continue;
                    }
                     // Skip if it's a nested private type, as they are often implementation details
                    if (type.IsNestedPrivate)
                    {
                        continue;
                    }


                    string ns = string.IsNullOrEmpty(type.Namespace) ? "Global.Namespace" : type.Namespace; // Ensure "Global" is distinct
                    if (!typesByNamespace.ContainsKey(ns))
                    {
                        typesByNamespace[ns] = new List<TypeDefinition>();
                    }
                    typesByNamespace[ns].Add(type);
                }
            }

            foreach (var kvp in typesByNamespace)
            {
                string namespaceName = kvp.Key;
                List<TypeDefinition> types = kvp.Value;

                if (!types.Any()) continue;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("// This file was auto-generated by ExtremeRoles.UnitTest.MockGenerator.");
                sb.AppendLine("// DO NOT MODIFY THIS FILE MANUALLY.");
                sb.AppendLine();

                string indent = "";
                if (namespaceName != "Global.Namespace")
                {
                    sb.AppendLine($"namespace {namespaceName}");
                    sb.AppendLine("{");
                    indent = "    ";
                }

                foreach (TypeDefinition type in types.OrderBy(t => t.Name)) // Order for consistency
                {
                    GenerateInterfaceForClass(sb, type, indent);
                }

                if (namespaceName != "Global.Namespace")
                {
                    sb.AppendLine("}");
                }

                string sanitizedNamespace = string.Join("_", namespaceName.Split(Path.GetInvalidFileNameChars()))
                                                 .Replace(".", "_"); // Replace dots for cleaner filenames
                string outputPath = Path.Combine(outputDirectory, $"GeneratedMocks_{sanitizedNamespace}.cs");
                File.WriteAllText(outputPath, sb.ToString());
                Console.WriteLine($"Generated mock interfaces for namespace {namespaceName} to: {outputPath}");
            }
             Console.WriteLine("Mock generation complete.");
        }

        private static void GenerateInterfaceForClass(StringBuilder sb, TypeDefinition type, string baseIndent)
        {
            Console.WriteLine($"{baseIndent}Generating interface for class: {(type.Namespace != null ? type.Namespace + "." : "")}{type.Name}");
            string interfaceName = type.Name; // Use the same name as the class
            // Handle nested class names by concatenating containing type names
            if (type.IsNested)
            {
                interfaceName = GetNestedTypeName(type);
            }

            sb.AppendLine($"{baseIndent}// Original Type: {type.FullName}");
            // Handle generic parameters for the interface definition
            string genericParameters = "";
            if (type.HasGenericParameters)
            {
                genericParameters = "<" + string.Join(", ", type.GenericParameters.Select(p => p.Name)) + ">";
            }

            sb.AppendLine($"{baseIndent}public interface {interfaceName}{genericParameters}");
            sb.AppendLine($"{baseIndent}{{");

            string memberIndent = baseIndent + "    ";

            // Process Fields as Properties
            foreach (FieldDefinition field in type.Fields)
            {
                if (field.IsCompilerControlled || field.Name.Contains("<") || field.Name.Contains("$") || field.IsSpecialName || field.IsRuntimeSpecialName)
                {
                     if (field.Name.EndsWith("k__BackingField")) continue;
                }
                sb.AppendLine($"{memberIndent}{GetFriendlyTypeName(field.FieldType, type.GenericParameters.ToList())} {Capitalize(field.Name)} {{ get; set; }} // From field: {field.Name}");
            }

            // Process Properties
            foreach (PropertyDefinition property in type.Properties)
            {
                if (property.Parameters.Any()) {
                    sb.AppendLine($"{memberIndent}// Skipped indexer: {property.Name}");
                    continue;
                }
                string propType = GetFriendlyTypeName(property.PropertyType, type.GenericParameters.ToList());
                sb.Append($"{memberIndent}{propType} {property.Name} {{ ");
                if (property.GetMethod != null) sb.Append("get; ");
                if (property.SetMethod != null) sb.Append("set; ");
                sb.AppendLine("}");
            }

            var methodSignatures = new HashSet<string>();

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.IsConstructor || method.IsSpecialName || method.IsRuntimeSpecialName ||
                    method.IsGetter || method.IsSetter || method.IsAddOn || method.IsRemoveOn || method.IsFire ||
                    method.Name.Contains("<") || method.Name.Contains("$") ||
                    (method.IsPrivate && !type.IsStatic && !type.IsVirtual) ) // Allow private virtual (explicit interface implementations)
                {
                    continue;
                }

                // Skip private static methods if they are not intended to be part of the public contract of the interface
                // However, the requirement was "スタティックメソッドはからのメソッドにして下さい" implying they should be included.
                // If a method is an explicit interface implementation, its name will contain a '.'
                if (method.IsPrivate && method.Name.Contains("."))
                {
                    // This is likely an explicit interface implementation, which we might want to skip or handle specially
                    // For now, let's include it if it's not special name.
                }


                string methodGenericParams = "";
                if (method.HasGenericParameters)
                {
                    methodGenericParams = "<" + string.Join(", ", method.GenericParameters.Select(p => p.Name)) + ">";
                }

                string returnType = GetFriendlyTypeName(method.ReturnType, type.GenericParameters.Concat(method.GenericParameters).ToList());
                string parameters = string.Join(", ", method.Parameters.Select(p =>
                {
                    string paramType = GetFriendlyTypeName(p.ParameterType, type.GenericParameters.Concat(method.GenericParameters).ToList());
                    string modifiers = "";
                    if (p.IsOut) modifiers += "out ";
                    else if (p.ParameterType.IsByReference && !p.IsOut) modifiers += "ref ";
                    return $"{modifiers}{paramType} {SanitizeParameterName(p.Name)}";
                }));

                string signature = $"{method.Name}{methodGenericParams}({parameters})";
                if (methodSignatures.Contains(signature))
                {
                    continue;
                }
                methodSignatures.Add(signature);

                sb.AppendLine($"{memberIndent}{returnType} {method.Name}{methodGenericParams}({parameters});{(method.IsStatic ? " // Was static" : "")}");
            }

            sb.AppendLine($"{baseIndent}}}");
            sb.AppendLine();
        }

        private static string SanitizeParameterName(string name)
        {
            // C# keywords list - can be expanded
            var keywords = new HashSet<string> { "object", "event", "string", "bool", "int", "out", "ref", "params" };
            if (keywords.Contains(name))
            {
                return "@" + name;
            }
            return name;
        }


        private static string GetNestedTypeName(TypeDefinition type)
        {
            if (type.IsNested)
            {
                // For nested types, C# uses '.' but for our interface name, to avoid issues if we place it
                // in the same namespace, we might use '_'. Or, ensure the generator handles nesting correctly
                // if types are generated within a structure that mirrors the original nesting.
                // For simplicity here, concatenating with '_' for the flat interface list.
                return GetNestedTypeName(type.DeclaringType) + "_" + type.Name.Split('`')[0];
            }
            return type.Name.Split('`')[0];
        }


        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            // Sanitize field name to be a valid property name (e.g. remove common prefixes like m_ or _)
            if (s.StartsWith("m_") && s.Length > 2) s = s.Substring(2);
            else if (s.StartsWith("_") && s.Length > 1) s = s.Substring(1);

            if (string.IsNullOrEmpty(s)) return "_field"; // Default if name becomes empty

            if (s.Length == 1) return s.ToUpper();
            // Avoid changing names that already start with an uppercase letter or are common acronyms
            // if (char.IsUpper(s[0])) return s; // This might be too restrictive if original field was e.g. "isEnabled"
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string GetFriendlyTypeName(TypeReference typeRef, ICollection<GenericParameter> currentGenericContext = null)
        {
            currentGenericContext = currentGenericContext ?? new List<GenericParameter>();

            if (typeRef.IsGenericParameter)
            {
                 // Check if it's a type or method generic parameter based on the context
                var gp = currentGenericContext.FirstOrDefault(p => p.FullName == typeRef.FullName);
                if (gp != null) return gp.Name;
                return typeRef.Name; // Fallback, though context should ideally resolve it
            }

            if (typeRef.IsByReference)
            {
                return GetFriendlyTypeName(typeRef.GetElementType(), currentGenericContext); // out/ref handled by caller
            }
            if (typeRef.IsPointer)
            {
                return GetFriendlyTypeName(typeRef.GetElementType(), currentGenericContext) + "*";
            }

            if (typeRef.IsArray)
            {
                var arrayType = (ArrayType)typeRef;
                return GetFriendlyTypeName(arrayType.ElementType, currentGenericContext) + "[" + new string(',', arrayType.Rank - 1) + "]";
            }

            if (typeRef is GenericInstanceType genericInstance)
            {
                // Resolve the element type (the generic type definition itself)
                string baseName = GetFriendlyTypeName(genericInstance.ElementType, currentGenericContext); // Pass context
                baseName = baseName.Split('`')[0]; // Clean CIL generic arity `X
                string genericArgs = string.Join(", ", genericInstance.GenericArguments.Select(arg => GetFriendlyTypeName(arg, currentGenericContext))); // Pass context
                return $"{baseName}<{genericArgs}>";
            }

            // Built-in type aliases
            switch (typeRef.FullName)
            {
                case "System.Void": return "void";
                case "System.Object": return "object";
                case "System.String": return "string";
                case "System.Int32": return "int";
                case "System.Boolean": return "bool";
                case "System.Single": return "float";
                case "System.Double": return "double";
                case "System.Decimal": return "decimal";
                case "System.Byte": return "byte";
                case "System.SByte": return "sbyte";
                case "System.Char": return "char";
                case "System.Int16": return "short";
                case "System.UInt16": return "ushort";
                case "System.UInt32": return "uint";
                case "System.Int64": return "long";
                case "System.UInt64": return "ulong";
                case "System.IntPtr": return "System.IntPtr";
                case "System.UIntPtr": return "System.UIntPtr";
            }

            // For other types, construct the name.
            // If it's a nested type, Cecil's FullName uses '/', C# uses '.'
            // We need to ensure the name is usable in C# and correctly references the type.
            // The goal is to produce a name that can be resolved in the generated code's context.
            string nameToReturn;
            if (typeRef.IsNested)
            {
                // For nested types, recursively build the name with '.' separator
                nameToReturn = GetFriendlyTypeName(typeRef.DeclaringType, currentGenericContext) + "." + typeRef.Name.Split('`')[0];
            }
            else
            {
                // For non-nested types, use Namespace.Name
                nameToReturn = string.IsNullOrEmpty(typeRef.Namespace) ? typeRef.Name.Split('`')[0] : $"{typeRef.Namespace}.{typeRef.Name.Split('`')[0]}";
            }

            // If the type itself is generic but not instantiated (TypeDefinition not GenericInstanceType)
            // e.g. class MyList<T> { void Method(MyList<T> param); }
            // We need to append its generic parameters.
            if (typeRef.HasGenericParameters && !(typeRef is GenericInstanceType))
            {
                 // This path is for TypeDefinition of a generic type.
                 // We need to make sure we're not infinitely recursing if typeRef is part of currentGenericContext.
                 // This usually means we are referring to the type itself, e.g. MyClass<T>
                nameToReturn += "<" + string.Join(", ", typeRef.GenericParameters.Select(gp => gp.Name)) + ">";
            }


            return nameToReturn;
        }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ExtremeRoles.UnitTest.MockGenerator <outputDirectory> <assemblyPath1> [assemblyPath2] ...");
                Console.WriteLine("Example: ExtremeRoles.UnitTest.MockGenerator ./GeneratedMocks ./MyLibrary.dll");
                return;
            }

            string outputDirectory = args[0];
            string[] assemblyPaths = args.Skip(1).ToArray();

            try
            {
                Generator.GenerateMocks(assemblyPaths, outputDirectory);
                Console.WriteLine("Successfully generated mock interfaces.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during mock generation: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}