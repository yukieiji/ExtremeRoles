using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorVerifier<
	ExtremeRoles.UnitTest.MockGenerator.MockGenerator,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using System.Threading.Tasks;
using System.IO; // For MemoryStream
using System.Linq; // For emitResult.Diagnostics.Select

namespace ExtremeRoles.UnitTest.MockGenerator.Tests;

[TestClass]
public class GeneratorTests
{
    [TestMethod]
    public async Task ClassInMockAssembly_GeneratesCorrectInterfaceAndStaticClass() // Renamed to reflect it generates both
    {
		string gameLibSource = """
namespace AmongUsGameLibExporter // Matches the name the generator looks for
{
public class ExportedGameClass
{
    public int PublicField;
    private string _privateField;      // Will become _privateField property
    internal bool m_internal_Field; // Will become m_internal_Field property

    public string GetPrivateField() => _privateField;
    public void SetPrivateField(string val) => _privateField = val;

    public void InstanceMethod(int val) { }
    public static string HelperStaticMethod(string input) { return input + " processed"; }
    public static int AnotherStatic(int x, int y) { return x + y; }

    public ExportedGameClass(int fieldVal) {
        PublicField = fieldVal;
        _privateField = "init";
        m_internal_Field = false;
    }
}
}
""";

		string mainSource = "";

		string expectedGeneratedCode = """
// Auto-generated code by ExtremeRoles.UnitTest.MockGenerator
// Target: AmongUsGameLibExporter
using System;
// Found assembly: AmongUsGameLibExporter
// Original Class: AmongUsGameLibExporter.ExportedGameClass (IsStatic: False)
namespace AmongUsGameLibExporter
{
public interface ExportedGameClass
{
    int PublicField { get; set; }
    System.String _privateField { get; set; }
    System.Boolean m_internal_Field { get; set; }
    void InstanceMethod(int val);
}
}

// Original Class: AmongUsGameLibExporter.ExportedGameClass (IsStatic: False)
namespace AmongUsGameLibExporter
{
public static class ExportedGameClass
{
    public static System.String HelperStaticMethod(string input)
    {
        // Static methods are empty as per specification
        return default(System.String);
    }
    public static int AnotherStatic(int x, int y)
    {
        // Static methods are empty as per specification
        return default(int);
    }
}
}

""";

        var gameLibCompilation = CSharpCompilation.Create(
            assemblyName: "AmongUsGameLibExporter",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(gameLibSource) },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new System.IO.MemoryStream())
        {
            var emitResult = gameLibCompilation.Emit(ms);
            Assert.IsTrue(emitResult.Success, "Mock AmongUsGameLibExporter compilation failed." + System.Environment.NewLine + string.Join(System.Environment.NewLine, emitResult.Diagnostics.Select(d => d.GetMessage())));
        }

        var verifierTest = new VerifyCS.Test
        {
            TestState =
            {
                Sources = { mainSource },
                GeneratedSources =
                {
                    (typeof(ExtremeRoles.UnitTest.MockGenerator.MockGenerator), "GeneratedMocks.g.cs", expectedGeneratedCode),
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            }
        };
        verifierTest.TestState.AdditionalReferences.Add(gameLibCompilation.ToMetadataReference());
        await verifierTest.RunAsync();
    }

    [TestMethod]
    public async Task StaticClass_GeneratesStaticClassWithEmptyMethods()
    {
        var gameLibSource = """
namespace AmongUsGameLibExporter
{
public static class StaticUtilityClass
{
    public static void StaticVoidMethod() { /* original logic */ }
    public static int StaticIntMethod(bool b) { return b ? 1 : 0; }
    public static string SomeStaticField = "hello";
}
}
""";
        var mainSource = "";

        var expectedGeneratedCode = """
// Auto-generated code by ExtremeRoles.UnitTest.MockGenerator
// Target: AmongUsGameLibExporter
using System;
// Found assembly: AmongUsGameLibExporter
// Original Class: AmongUsGameLibExporter.StaticUtilityClass (IsStatic: True)
namespace AmongUsGameLibExporter
{
public static class StaticUtilityClass
{
    public static void StaticVoidMethod()
    {
        // Static methods are empty as per specification
    }
    public static int StaticIntMethod(bool b)
    {
        // Static methods are empty as per specification
        return default(int);
    }
}
}

""";

        var gameLibCompilation = CSharpCompilation.Create(
            assemblyName: "AmongUsGameLibExporter",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(gameLibSource) },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new System.IO.MemoryStream())
        {
            var emitResult = gameLibCompilation.Emit(ms);
            Assert.IsTrue(emitResult.Success, "Mock AmongUsGameLibExporter (StaticClass) compilation failed." + System.Environment.NewLine + string.Join(System.Environment.NewLine, emitResult.Diagnostics.Select(d => d.GetMessage())));
        }

        var verifierTest = new VerifyCS
        {
            TestState =
            {
                Sources = { mainSource },
                GeneratedSources =
                {
                    (typeof(ExtremeRoles.UnitTest.MockGenerator.MockGenerator), "GeneratedMocks.g.cs", expectedGeneratedCode),
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
            }
        };
        verifierTest.TestState.AdditionalReferences.Add(gameLibCompilation.ToMetadataReference());
        await verifierTest.RunAsync();
    }
}
