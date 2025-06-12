using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace ExtremeRoles.AttributeRemover.Generator
{
    [Generator]
    public class AttributeRemoverGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find AdditionalTexts that are marked as our target plugin file
            IncrementalValuesProvider<AdditionalText> pluginFiles = context.AdditionalTextsProvider
                .Where(at =>
                    {
                        // Option 1: Check for specific metadata (if we add it in csproj)
                        // bool hasMetadata = context.AnalyzerConfigOptionsProvider
                        //     .GetOptions(at)
                        //     .TryGetValue("build_metadata.AdditionalFiles.IsExtremeRolesPlugin", out string isPluginFlag);
                        // return hasMetadata && string.Equals(isPluginFlag, "true", StringComparison.OrdinalIgnoreCase);

                        // Option 2: Check by linked path (more robust if metadata isn't always set)
                        return at.Path.EndsWith("ExtremeRolesPlugin.cs", StringComparison.OrdinalIgnoreCase);
                    });

            context.RegisterSourceOutput(pluginFiles, (spc, additionalText) =>
            {
                ProcessPluginFile(spc, additionalText);
            });
        }

        private void ProcessPluginFile(SourceProductionContext spc, AdditionalText pluginFile)
        {
            SourceText sourceText = pluginFile.GetText(spc.CancellationToken);
            if (sourceText == null)
            {
                // Report diagnostic if file can't be read
                spc.ReportDiagnostic(Diagnostic.Create("AR001", "AttributeRemoverGenerator", $"Could not read AdditionalText: {pluginFile.Path}", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 0));
                return;
            }

            string originalCode = sourceText.ToString();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(originalCode, cancellationToken: spc.CancellationToken);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot(spc.CancellationToken);

            var rewriter = new AttributeRemovalRewriter();
            SyntaxNode newRoot = rewriter.Visit(root);

            if (newRoot != root) // Check if any changes were made
            {
                string fileName = Path.GetFileNameWithoutExtension(pluginFile.Path) + ".Stripped.cs";
                spc.AddSource(fileName, SourceText.From(newRoot.ToFullString(), Encoding.UTF8));
            }
            else
            {
                // If no attributes were found/removed (e.g. class not found or no attributes),
                // maybe output the original or nothing, or a comment.
                // For now, if no changes, we don't add a new source to avoid duplicate type definitions if the original is still compiled.
                // The csproj should ensure the original linked .cs is not directly compiled.
                 spc.AddSource(Path.GetFileNameWithoutExtension(pluginFile.Path) + ".NoChanges.cs", SourceText.From($"// AttributeRemoverGenerator: No attributes found or removed from {Path.GetFileName(pluginFile.Path)}.", Encoding.UTF8));
            }
        }
    }

    public class AttributeRemovalRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Only modify the class named "ExtremeRolesPlugin"
            if (node.Identifier.ValueText == "ExtremeRolesPlugin")
            {
                // Remove all attribute lists from this class
                if (node.AttributeLists.Any())
                {
                    return node.WithAttributeLists(new SyntaxList<AttributeListSyntax>());
                }
            }
            // Visit children to ensure nested classes are also processed if needed by base,
            // though our current requirement is only for the main ExtremeRolesPlugin class.
            return base.VisitClassDeclaration(node);
        }

        // Optionally, override VisitStructDeclaration, VisitInterfaceDeclaration, etc.,
        // if attributes need to be removed from other types or members.
        // For now, only targeting the class itself as per the request.
    }
}
