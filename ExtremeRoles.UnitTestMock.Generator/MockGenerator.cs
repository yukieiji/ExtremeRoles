using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO; // Added for Path operations

namespace ExtremeRoles.UnitTestMock.Generator;

[Generator]
public class MockGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Pipeline 1: For generating mocks from AmongUs.GameLibs.Steam
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;
        context.RegisterSourceOutput(compilationProvider, (spc, compilation) =>
        {
            ExecuteMockInterfaceGeneration(compilation, spc);
        });

        // Pipeline 2: For processing ExtremeRolesPlugin.cs to remove attributes
        IncrementalValuesProvider<AdditionalText> pluginFiles = context.AdditionalTextsProvider
            .Where(at => at.Path.EndsWith("ExtremeRolesPlugin.cs", StringComparison.OrdinalIgnoreCase));

        context.RegisterSourceOutput(pluginFiles, (spc, additionalText) =>
        {
            ExecutePluginAttributeStripping(spc, additionalText);
        });
    }

    // --- Methods for Mock Interface Generation (from original MockGenerator) ---
    private void ExecuteMockInterfaceGeneration(Compilation compilation, SourceProductionContext spc)
    {
        var amongUsLib = compilation.ExternalReferences
            .Select(er => compilation.GetAssemblyOrModuleSymbol(er) as IAssemblySymbol)
            .FirstOrDefault(asm => asm != null && asm.Identity.Name == "AmongUs.GameLibs.Steam");

        if (amongUsLib == null)
        {
            spc.AddSource("MockInterfaceGenerator_Error.g.cs", SourceText.From("// AmongUs.GameLibs.Steam not found for mock generation", Encoding.UTF8));
            return;
        }

        var allNamespaces = new List<INamespaceSymbol>();
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(amongUsLib.GlobalNamespace);

        while (stack.Count > 0)
        {
            var currentNamespace = stack.Pop();
            allNamespaces.Add(currentNamespace);
            foreach (var subNamespace in currentNamespace.GetNamespaceMembers())
            {
                stack.Push(subNamespace);
            }
        }

        var sb = new StringBuilder(); // StringBuilder will be used by GenerateMockInterface

        foreach (var ns in allNamespaces)
        {
            foreach (var typeSymbol in GetAllTypesInNamespace(ns)) // Assumes GetAllTypesInNamespace is defined below
            {
                if (typeSymbol.TypeKind == TypeKind.Class &&
                    (typeSymbol.DeclaredAccessibility == Accessibility.Public || typeSymbol.DeclaredAccessibility == Accessibility.Internal))
                {
                    if (!typeSymbol.IsImplicitlyDeclared && !typeSymbol.IsAnonymousType && typeSymbol.Name != "<PrivateImplementationDetails>")
                    {
                        GenerateMockInterface(sb, typeSymbol, spc, compilation); // Assumes GenerateMockInterface is defined below
                    }
                }
            }
        }
    }

    private IEnumerable<ITypeSymbol> GetAllTypesInNamespace(INamespaceSymbol nsSymbol)
    {
        foreach (var typeMember in nsSymbol.GetTypeMembers())
        {
            yield return typeMember;
            foreach (var nestedType in GetAllNestedTypes(typeMember)) // Assumes GetAllNestedTypes is defined below
            {
                yield return nestedType;
            }
        }
    }

    private IEnumerable<ITypeSymbol> GetAllNestedTypes(ITypeSymbol typeSymbol)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
                if (!nestedType.IsImplicitlyDeclared && !nestedType.IsAnonymousType && nestedType.Name != "<PrivateImplementationDetails>")
                {
                yield return nestedType;
                foreach (var subNestedType in GetAllNestedTypes(nestedType))
                {
                    yield return subNestedType;
                }
                }
        }
    }

    private void GenerateMockInterface(StringBuilder sb, ITypeSymbol classSymbol, SourceProductionContext spc, Compilation compilation)
    {
        string originalClassName = classSymbol.Name;

        string fullTypeNameForHint = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                                .Replace("global::", "")
                                                .Replace("<", "_")
                                                .Replace(">", "_")
                                                .Replace(".", "_")
                                                .Replace(",", "_")
                                                .Replace(" ", "_")
                                                .Replace("`", "_");

        string originalNamespace = classSymbol.ContainingNamespace.ToDisplayString();

        sb.Clear();
        sb.AppendLine("// Auto-generated MOCK INTERFACE by ExtremeRoles.UnitTestMock.Generator"); // Clarify source
        sb.AppendLine($"// Original Type: {classSymbol.ToDisplayString()}");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.Events;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using InnerNet;");
        sb.AppendLine("using Steamworks;");
        sb.AppendLine();

        bool hasNamespace = !string.IsNullOrEmpty(originalNamespace) && !classSymbol.ContainingNamespace.IsGlobalNamespace;
        string currentIndent = "";

        List<string> parentInterfaceDeclarations = new List<string>();
        if (hasNamespace)
        {
            sb.AppendLine($"namespace {originalNamespace}");
            sb.AppendLine("{");
            currentIndent = "    ";
        }

        Stack<ITypeSymbol> parentTypes = new Stack<ITypeSymbol>();
        ITypeSymbol containingType = classSymbol.ContainingType;
        while(containingType != null && (containingType.DeclaredAccessibility == Accessibility.Public || containingType.DeclaredAccessibility == Accessibility.Internal))
        {
            parentTypes.Push(containingType);
            containingType = containingType.ContainingType;
        }

        string originalCurrentIndent = currentIndent;

        foreach(var parentType in parentTypes)
        {
            sb.AppendLine($"{currentIndent}public partial interface I{parentType.Name} // Interface for containing type");
            sb.AppendLine($"{currentIndent}{{");
            parentInterfaceDeclarations.Add($"{currentIndent}}} // End of I{parentType.Name}");
            currentIndent += "    ";
        }

        sb.AppendLine($"{currentIndent}public interface {originalClassName}");
        sb.AppendLine($"{currentIndent}{{");

        string memberIndent = currentIndent + "    ";

        foreach (var member in classSymbol.GetMembers())
        {
            bool isPublicOrInternal = member.DeclaredAccessibility == Accessibility.Public || member.DeclaredAccessibility == Accessibility.Internal || member.DeclaredAccessibility == Accessibility.ProtectedOrInternal;
            if (!isPublicOrInternal || member.IsImplicitlyDeclared)
            {
                continue;
            }

            if (member.Kind == SymbolKind.Property)
            {
                var property = (IPropertySymbol)member;
                if (!IsTypeAccessible(property.Type, compilation) || property.IsStatic)
                {
                    continue;
                }

                string getter = property.GetMethod != null && (property.GetMethod.DeclaredAccessibility == Accessibility.Public || property.GetMethod.DeclaredAccessibility == Accessibility.Internal || property.GetMethod.DeclaredAccessibility == Accessibility.ProtectedOrInternal) ? "get;" : "";
                string setter = property.SetMethod != null && (property.SetMethod.DeclaredAccessibility == Accessibility.Public || property.SetMethod.DeclaredAccessibility == Accessibility.Internal || property.SetMethod.DeclaredAccessibility == Accessibility.ProtectedOrInternal) ? "set;" : "";
                if (!string.IsNullOrEmpty(getter) || !string.IsNullOrEmpty(setter))
                {
                        sb.AppendLine($"{memberIndent}{property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {property.Name} {{ {(getter)} {(setter)} }}");
                }
            }
            else if (member.Kind == SymbolKind.Field)
            {
                var field = (IFieldSymbol)member;
                if (field.AssociatedSymbol == null && !field.IsConst && !field.IsStatic)
                {
                    if (!IsTypeAccessible(field.Type, compilation))
                    {
                        continue;
                    }
                    if (field.IsReadOnly)
                    {
                        sb.AppendLine($"{memberIndent}{field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {field.Name} {{ get; }}");
                    }
                    else
                    {
                        sb.AppendLine($"{memberIndent}{field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {field.Name} {{ get; set; }}");
                    }
                }
            }
            else if (member.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)member;
                if (method.MethodKind == MethodKind.Ordinary &&
                    !method.IsGenericMethod &&
                    !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_"))
                {
                    if (!IsTypeAccessible(method.ReturnType, compilation) || method.Parameters.Any(p => !IsTypeAccessible(p.Type, compilation) || p.Type.SpecialType == SpecialType.System_Void))
                    {
                        continue;
                    }
                    if (method.Parameters.Any(p => p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref))
                    {
                        continue;
                    }

                    string returnTypeDisplay = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    string methodName = method.Name;
                    string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {SanitizeParameterName(p.Name)}"));

                    if (method.IsStatic)
                    {
                        sb.AppendLine($"{memberIndent}{returnTypeDisplay} {methodName}({parameters}); // Was static. Implement to return default or specific mock behavior.");
                    }
                    else
                    {
                        sb.AppendLine($"{memberIndent}{returnTypeDisplay} {methodName}({parameters});");
                    }
                }
            }
        }

        sb.AppendLine($"{currentIndent}}} // End of {originalClassName}");

        for(int i = parentInterfaceDeclarations.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(parentInterfaceDeclarations[i]);
        }

        currentIndent = originalCurrentIndent;

        if (hasNamespace)
        {
            sb.AppendLine("} // End of namespace");
        }

        string hintName = $"Mock_{fullTypeNameForHint}.g.cs"; // Prefix hint name for clarity
        spc.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private string SanitizeParameterName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "_param" + Guid.NewGuid().ToString("N").Substring(0, 4);
        }
        name = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (string.IsNullOrWhiteSpace(name))
        {
            return "_param" + Guid.NewGuid().ToString("N").Substring(0,4);
        }
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    }

    private bool IsTypeAccessible(ITypeSymbol typeSymbol, Compilation compilation)
    {
        if (typeSymbol == null || typeSymbol is IErrorTypeSymbol)
        {
            return false;
        }
        if (typeSymbol.SpecialType == SpecialType.System_Void)
        {
            return true;
        }
        if (typeSymbol is IPointerTypeSymbol)
        {
            return false;
        }
        if (typeSymbol.TypeKind == TypeKind.TypeParameter)
        {
            return true;
        }

        var accessibility = typeSymbol.DeclaredAccessibility;
        bool isAccessibleEnough = accessibility == Accessibility.Public ||
                                    accessibility == Accessibility.Internal ||
                                    accessibility == Accessibility.ProtectedOrInternal;

        if (!isAccessibleEnough)
        {
            return typeSymbol.ContainingType != null && IsTypeAccessible(typeSymbol.ContainingType, compilation);
        }

        if (typeSymbol is INamedTypeSymbol namedType)
        {
            if (namedType.IsGenericType)
            {
                if (!namedType.TypeArguments.All(ta => IsTypeAccessible(ta, compilation)))
                {
                    return false;
                }
            }
        }
        else if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return IsTypeAccessible(arrayType.ElementType, compilation);
        }

        return true;
    }

    // --- Methods for Attribute Stripping (from AttributeRemoverGenerator) ---
    private void ExecutePluginAttributeStripping(SourceProductionContext spc, AdditionalText pluginFile)
    {
        SourceText sourceText = pluginFile.GetText(spc.CancellationToken);
        if (sourceText == null)
        {
            spc.ReportDiagnostic(Diagnostic.Create("MG001", "MockGenerator.AttributeStripping", $"Could not read AdditionalText: {pluginFile.Path}", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 0));
            return;
        }

        string originalCode = sourceText.ToString();
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(originalCode, cancellationToken: spc.CancellationToken);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot(spc.CancellationToken);

        var rewriter = new AttributeRemovalRewriter(); // Assumes AttributeRemovalRewriter is defined below
        SyntaxNode newRoot = rewriter.Visit(root);

        if (newRoot != root)
        {
            string fileName = Path.GetFileNameWithoutExtension(pluginFile.Path) + ".Stripped.cs";
            spc.AddSource(fileName, SourceText.From(newRoot.ToFullString(), Encoding.UTF8));
        }
        else
        {
                spc.AddSource(Path.GetFileNameWithoutExtension(pluginFile.Path) + ".NoAttributeChanges.cs", SourceText.From($"// MockGenerator: No attributes found or removed from {Path.GetFileName(pluginFile.Path)} by attribute stripping logic.", Encoding.UTF8));
        }
    }
}

// --- AttributeRemovalRewriter class (from AttributeRemoverGenerator) ---
public class AttributeRemovalRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Identifier.ValueText == "ExtremeRolesPlugin")
        {
            if (node.AttributeLists.Any())
            {
                return node.WithAttributeLists(new SyntaxList<AttributeListSyntax>());
            }
        }
        return base.VisitClassDeclaration(node);
    }
}
