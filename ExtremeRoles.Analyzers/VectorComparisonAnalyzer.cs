using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExtremeRoles.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class VectorComparisonAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor RuleERA003 = new DiagnosticDescriptor(
        "ERA003",
        "UnityのVector型を == や != で比較することはできません",
		"UnityのVector型を == や != で比較することはできません。代わりにVector.IsCloseToかVector.IsNotCloseToを使用してください",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "UnityのVector型は浮動小数点数のため、== や != での比較は想定しない結果を生む可能性があります.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleERA003];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(analyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void analyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        var leftTypeInfo = semanticModel.GetTypeInfo(binaryExpression.Left).Type;
        var rightTypeInfo = semanticModel.GetTypeInfo(binaryExpression.Right).Type;

        if (leftTypeInfo == null || rightTypeInfo == null)
        {
            return;
        }

        if (isVectorType(leftTypeInfo) || isVectorType(rightTypeInfo))
        {
            var diagnostic = Diagnostic.Create(RuleERA003, binaryExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool isVectorType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.ContainingNamespace?.ToString() == "UnityEngine")
        {
            switch (typeSymbol.Name)
            {
                case "Vector2":
                case "Vector3":
                case "Vector4":
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
}
