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
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor RuleERA001 = new DiagnosticDescriptor(
		"ERA001",
		"Il2CppSystem.Objectを継承するクラスには'System.IntPtr'を受け取るコンストラクタが必要です",
		"クラス '{0}' は'Il2CppSystem.Object'を継承していますが、'System.IntPtr'を受け取るコンストラクタが存在しません",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
		"'Il2CppSystem.Object'を継承するクラスは'System.IntPtr'を受け取るコンストラクタが必要です.");

	private static readonly DiagnosticDescriptor RuleERA002 = new DiagnosticDescriptor(
		"ERA002",
		"Il2CppSystem.Objectを継承するクラスには'Il2CppRegisterAttribute'属性がこのクラスか親クラスに必要です",
		"クラス '{0}' は'Il2CppSystem.Object'を継承していますが、'Il2CppRegisterAttribute'属性がこのクラスに存在しません",
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		"'Il2CppSystem.Object'を継承するクラスは'Il2CppRegisterAttribute'属性がこのクラスか親クラスに必要です.");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleERA001, RuleERA002];

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

		if (!inheritsIl2CppObject(classSymbol))
        {
            return;
        }

		if (!hasIntPtrConstructor(classSymbol))
		{
			var diagnostic = Diagnostic.Create(RuleERA001, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
			context.ReportDiagnostic(diagnostic);
		}

		if (!classSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "Il2CppRegisterAttribute"))
		{
			var diagnostic = Diagnostic.Create(RuleERA002, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool hasIntPtrConstructor(INamedTypeSymbol classSymbol)
		=> classSymbol.Constructors.Any(ctor =>
		{
			if (ctor.Parameters.Length != 1)
			{
				return false;
			}
			var t = ctor.Parameters[0].Type;
			var nameSpace = t.ContainingNamespace;
			return nameSpace.Name == "System" && t.Name.Contains("IntPtr");
		});

	private static bool inheritsIl2CppObject(INamedTypeSymbol classSymbol)
    {
        var baseType = classSymbol.BaseType;
        while (baseType != null)
        {
			var nameSpace = baseType.ContainingNamespace;

            if (nameSpace.Name == "Il2CppSystem" &&
				baseType.Name.Contains("Object"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
