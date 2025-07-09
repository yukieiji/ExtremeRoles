```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExtremeRoles.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ERA001";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = "Il2CppSystem.Objectを継承するクラスのコンストラクタと属性の検証";
        private static readonly LocalizableString MessageFormat = "クラス '{0}' は Il2CppSystem.Object または UnityEngine.Object を継承していますが、System.IntPtr を受け取るコンストラクタを持たず、Il2CppRegisterAttribute 属性もありません。";
        private static readonly LocalizableString Description = "Il2CppSystem.Object または UnityEngine.Object を継承するクラスは、System.IntPtr を受け取るコンストラクタを持つか、Il2CppRegisterAttribute 属性を持つ必要があります。";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null)
            {
                return;
            }

            bool inheritsFromUnityObject = InheritsFromType(classSymbol, "UnityEngine.Object");
            bool inheritsFromIl2CppSystemObject = !inheritsFromUnityObject && InheritsFromType(classSymbol, "Il2CppSystem.Object");

            // Heuristic: For IL2CPP, non-UnityEngine classes directly inheriting System.Object
            // are often treated as Il2CppSystem.Object.
            bool isPotentialIl2CppTarget = classSymbol.BaseType?.SpecialType == SpecialType.System_Object &&
                                           classSymbol.BaseType.ToDisplayString() == "System.Object";

            if (!inheritsFromUnityObject && !inheritsFromIl2CppSystemObject && !isPotentialIl2CppTarget)
            {
                return;
            }

            // If it's a direct sub-class of System.Object, ensure it's not a common BCL type we should ignore.
            if (isPotentialIl2CppTarget && classSymbol.ContainingNamespace.ToDisplayString().StartsWith("System"))
            {
                // Likely a BCL type, not a user-defined Il2CppSystem.Object candidate.
                return;
            }

            bool hasIl2CppRegisterAttribute = classSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "Il2CppRegisterAttribute");

            if (hasIl2CppRegisterAttribute)
            {
                return;
            }

            bool hasIntPtrConstructor = classSymbol.Constructors.Any(ctor =>
                ctor.Parameters.Length == 1 &&
                ctor.Parameters[0].Type.ToDisplayString() == "System.IntPtr");

            if (!hasIntPtrConstructor)
            {
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool InheritsFromType(INamedTypeSymbol classSymbol, string targetTypeName)
        {
            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.ToDisplayString() == targetTypeName)
                {
                    return true;
                }
                // Consider UnityEngine.Object as a match if checking for Il2CppSystem.Object.
                if (targetTypeName == "Il2CppSystem.Object" && baseType.ToDisplayString() == "UnityEngine.Object")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
```
