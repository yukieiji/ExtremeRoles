using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace ExtremeRoles.UnitTest.MockGenerator;

[Generator]
public class MockGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        IncrementalValuesProvider<string> sourceTextsProvider = compilationProvider.SelectMany((compilation, ct) =>
        {
            List<string> generatedSources = new List<string>();
            IAssemblySymbol gameLibExporterAssembly = null;

            if (compilation != null)
            {
                foreach (var reference in compilation.References)
                {
                    var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                    if (assemblySymbol != null && assemblySymbol.Name.Equals("AmongUsGameLibExporter", StringComparison.OrdinalIgnoreCase))
                    {
                        gameLibExporterAssembly = assemblySymbol;
                        break;
                    }
                }
            }

            if (gameLibExporterAssembly != null)
            {
                List<INamedTypeSymbol> allTypeSymbols = new List<INamedTypeSymbol>();
                CollectTypesFromNamespace(gameLibExporterAssembly.GlobalNamespace, allTypeSymbols);

                foreach (var typeSymbol in allTypeSymbols)
                {
                    if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.DeclaredAccessibility == Accessibility.Public)
                    {
                        string interfaceCode = GenerateInterfaceForClass(typeSymbol);
                        if (!string.IsNullOrEmpty(interfaceCode))
                        {
                            generatedSources.Add(interfaceCode);
                        }

                        string staticClassCode = GenerateStaticClassForStaticMethods(typeSymbol);
                        if (!string.IsNullOrEmpty(staticClassCode))
                        {
                            generatedSources.Add(staticClassCode);
                        }
                    }
                }
            }
            else
            {
                generatedSources.Add("// AmongUsGameLibExporter assembly not found in referenced assemblies.");
            }
            return generatedSources;
        });

        IncrementalValueProvider<string> finalSourceProvider = sourceTextsProvider.Collect().Select((sources, ct) =>
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Auto-generated code by ExtremeRoles.UnitTest.MockGenerator (Incremental)");
            sb.AppendLine("// Target: AmongUsGameLibExporter");
            sb.AppendLine("using System;");
            foreach (string sourceEntry in sources)
            {
                sb.AppendLine(sourceEntry);
            }
            return sb.ToString();
        });

        context.RegisterSourceOutput(finalSourceProvider, (spc, source) =>
        {
            spc.AddSource("GeneratedMocks.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    // New recursive helper to collect types
    private void CollectTypesFromNamespace(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> collectedTypes)
    {
        foreach (var typeMember in namespaceSymbol.GetTypeMembers())
        {
            collectedTypes.Add(typeMember);
            // If you need to collect nested types within types (not just namespaces), add logic here.
            // For now, this collects top-level types within the namespace and its sub-namespaces.
        }

        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            CollectTypesFromNamespace(subNamespace, collectedTypes);
        }
    }

    private string GenerateInterfaceForClass(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.IsStatic || classSymbol.DeclaredAccessibility != Accessibility.Public) return string.Empty;

        StringBuilder sb = new StringBuilder();
        string interfaceName = classSymbol.Name;
        sb.AppendLine($"namespace {classSymbol.ContainingNamespace.ToDisplayString()}");
        sb.AppendLine("{");
        sb.AppendLine($"    public interface {interfaceName}");
        sb.AppendLine("    {");

        foreach (var fieldSymbol in classSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!fieldSymbol.IsStatic)
            {
                sb.AppendLine($"        {fieldSymbol.Type.ToDisplayString()} {fieldSymbol.Name} {{ get; set; }}");
            }
        }

        foreach (var methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (!methodSymbol.IsStatic && methodSymbol.MethodKind == MethodKind.Ordinary && methodSymbol.DeclaredAccessibility == Accessibility.Public)
            {
                string returnType = methodSymbol.ReturnType.ToDisplayString();
                string parameters = string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                sb.AppendLine($"        {returnType} {methodSymbol.Name}({parameters});");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        return sb.ToString();
    }

	private string GenerateStaticClassForStaticMethods(INamedTypeSymbol classSymbol)
	{
		if (classSymbol.DeclaredAccessibility != Accessibility.Public)
		{
			return string.Empty;
		}

		var staticMethods = classSymbol.GetMembers().OfType<IMethodSymbol>()
				.Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary)
				.ToList();

		if (staticMethods.Count == 0)
		{
			return string.Empty;
		}

		StringBuilder sb = new StringBuilder();
		string staticClassName = classSymbol.Name;
		sb.AppendLine($"namespace {classSymbol.ContainingNamespace.ToDisplayString()}");
		sb.AppendLine("{");
		sb.AppendLine($"    public static class {staticClassName}");
		sb.AppendLine("    {");

		foreach (var methodSymbol in staticMethods)
		{
			string delegateType;
			string parametersWithType = string.Join(", ", methodSymbol.Parameters.Select(p => p.Type.ToDisplayString()));
			string mockPropertyName = methodSymbol.Name + "_Mock";

			if (methodSymbol.ReturnsVoid)
			{
				delegateType = methodSymbol.Parameters.Any() ? $"System.Action<{parametersWithType}>" : "System.Action";
			}
			else
			{
				string returnTypeString = methodSymbol.ReturnType.ToDisplayString();
				delegateType = methodSymbol.Parameters.Any() ? $"System.Func<{parametersWithType}, {returnTypeString}>" : $"System.Func<{returnTypeString}>";
			}
			sb.AppendLine($"        public static {delegateType} {mockPropertyName} {{ get; set; }}");
		}
		sb.AppendLine();

		foreach (var methodSymbol in staticMethods)
		{
			string returnTypeString = methodSymbol.ReturnType.ToDisplayString();
			string mockPropertyName = methodSymbol.Name + "_Mock";
			string parametersWithTypeAndName = string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
			string parameterNamesOnly = string.Join(", ", methodSymbol.Parameters.Select(p => p.Name));

			sb.AppendLine($"        public static {returnTypeString} {methodSymbol.Name}({parametersWithTypeAndName})");
			sb.AppendLine("        {");
			sb.AppendLine($"            if ({mockPropertyName} != null)");
			sb.AppendLine("            {");
			if (methodSymbol.ReturnsVoid)
			{
				sb.AppendLine($"                {mockPropertyName}({parameterNamesOnly});");
				sb.AppendLine($"                return;");
			}
			else
			{
				sb.AppendLine($"                return {mockPropertyName}({parameterNamesOnly});");
			}
			sb.AppendLine("            }");
			if (!methodSymbol.ReturnsVoid)
			{
				sb.AppendLine($"            return default({returnTypeString});");
			}
			sb.AppendLine("        }");
		}

		sb.AppendLine("    }");
		sb.AppendLine("}");
		sb.AppendLine();
		return sb.ToString();
	}
}
