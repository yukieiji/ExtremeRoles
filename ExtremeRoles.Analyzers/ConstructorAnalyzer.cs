using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExtremeRoles.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstructorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ERA001";
    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Il2CppSystem.Objectを継承するクラスのコンストラクタと属性のチェック";
    private static readonly LocalizableString MessageFormat = "クラス '{0}' は'Il2CppSystem.Object'を継承していますが、'System.IntPtr'を受け取るコンストラクタか'Il2CppRegisterAttribute'属性もありません。";
    private static readonly LocalizableString Description = "'Il2CppSystem.Object'を継承するクラスは'System.IntPtr'を受け取るコンストラクタを持ち'Il2CppRegisterAttribute'属性を持つ必要があります。";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        Description);

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

        if (!InheritsFromType(classSymbol, "Il2CppSystem.Object"))
        {
            // Likely a BCL type, not a user-defined Il2CppSystem.Object candidate.
            return;
        }
        if (classSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "Il2CppRegisterAttribute"))
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
			string typeStr = baseType.ToDisplayString();
            if (typeStr == targetTypeName)
            {
                return true;
            }
            // Consider UnityEngine.Object as a match if checking for Il2CppSystem.Object.
            if (targetTypeName == "Il2CppSystem.Object" &&
				typeStr == "UnityEngine.Object")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
