using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ExtremeRoles.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VectorComparisonAnalyzer : DiagnosticAnalyzer
{
    private const string category = "Usage";

    private static readonly DiagnosticDescriptor ruleERA003 = new DiagnosticDescriptor(
        "ERA003",
        "UnityのVector型を == や != で比較することはできません",
        "UnityのVector型を == や != で比較することはできません。代わりにVector.IsCloseToかVector.IsNotCloseToを使用してください",
        category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "UnityのVector型は浮動小数点数のため、== や != での比較は想定しない結果を生む可能性があります.");

    private static readonly DiagnosticDescriptor ruleERA004 = new DiagnosticDescriptor(
        "ERA004",
        "Vector.IsCloseToとVector.IsNotCloseToの第2引数が0.1以上です",
        "Vector.IsCloseToとVector.IsNotCloseToの第2引数に0.1以上の値を指定することは推奨されません。より小さい値を指定してください",
        category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Vector.IsCloseToとVector.IsNotCloseToの第2引数は2乗誤差（sqrEps）であり、0.1以上は誤差としては大きすぎます.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ruleERA003, ruleERA004];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(analyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(analyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    private static void analyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        var operation = semanticModel.GetOperation(invocation) as IInvocationOperation;

        if (operation == null)
        {
            return;
        }

        var methodSymbol = operation.TargetMethod;

        if (methodSymbol.Name != "IsCloseTo" && methodSymbol.Name != "IsNotCloseTo")
        {
            return;
        }

        if (methodSymbol.ContainingType?.Name != "VectorExtension")
        {
            return;
        }

        if (methodSymbol.ContainingNamespace?.ToString() != "ExtremeRoles.Extension.Vector")
        {
            return;
        }

        var sqrEpsArgument = operation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "sqrEps");

        if (sqrEpsArgument == null || sqrEpsArgument.ArgumentKind == ArgumentKind.DefaultValue)
        {
            return;
        }

        var constantValue = sqrEpsArgument.Value.ConstantValue;

        if (constantValue.HasValue)
        {
            if (constantValue.Value is not (float or double))
            {
                return;
            }

            float floatValue = constantValue.Value switch
            {
                float f => f,
                double d => (float)d,
                _ => 0,
            };

            if (floatValue >= 0.1f)
            {
                var diagnostic = Diagnostic.Create(ruleERA004, sqrEpsArgument.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
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
            var diagnostic = Diagnostic.Create(ruleERA003, binaryExpression.GetLocation());
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
