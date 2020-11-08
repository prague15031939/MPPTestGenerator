using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using TestGen;

namespace UnitTests
{
    [TestClass]
    public class TestGeneratorTest
    {
        private string SourceCode;
        private TestInfo result;

        [TestInitialize]
        public void Setup()
        {
            using (StreamReader fStream = new StreamReader(@"D:\Works C#\��� ���� 4\MPPTestGenerator\Samples\SourceClasses\Tracer.cs"))
            {
                SourceCode = fStream.ReadToEnd();
            }
            result = new TestGenerator().Generate(SourceCode);
        }

        [TestMethod]
        public void TestNamespacesAndClasses()
        {
            string[] namespaces = CSharpSyntaxTree.ParseText(result.TestCode).
                GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().
                Select(item => item.Name.ToString()).ToArray();
            string[] classes = CSharpSyntaxTree.ParseText(result.TestCode).
                GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().
                Select(item => item.Identifier.ValueText).ToArray();

            Assert.AreEqual(namespaces.Length, 1);
            Assert.AreEqual(classes.Length, 1);
            Assert.AreEqual(namespaces[0], "UnitTests");
            Assert.AreEqual(classes[0], "TracerMainTest");
        }

        [TestMethod]
        public void TestUsingStatements()
        {
            string[] UsingStatements = CSharpSyntaxTree.ParseText(result.TestCode).
                GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>().
                Select(item => item.Name.ToString()).ToArray();

            Assert.AreEqual(UsingStatements.Length, 5);
            string[] DesiredUsings = { "System", "System.Diagnostics", "System.Threading", "System.Collections.Generic", "Microsoft.VisualStudio.TestTools.UnitTesting" };
            for (int i = 0; i < DesiredUsings.Length; i++)
                Assert.AreEqual(DesiredUsings[i], UsingStatements[i]);
        }

        [TestMethod]
        public void TestMethods()
        {
            var methods = CSharpSyntaxTree.ParseText(result.TestCode).
                GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();

            Assert.AreEqual(methods.Count(), 5);
            foreach (MethodDeclarationSyntax method in methods)
            {
                Assert.AreEqual(method.ReturnType.ToString(), "void");
                Assert.IsTrue(method.Modifiers.Any(SyntaxKind.PublicKeyword));
                if (method.Identifier.ValueText == "Setup")
                    Assert.AreEqual(method.AttributeLists[0].Attributes[0].Name.ToString(), "TestInitialize");
                else
                {
                    Assert.AreEqual(method.AttributeLists[0].Attributes[0].Name.ToString(), "TestMethod");
                    Assert.AreEqual(method.Body.Statements[0].NormalizeWhitespace().ToFullString(), "Assert.Fail(\"autogenerated\");");
                }

                Assert.IsTrue(methods.ToList().Exists(item => item.Identifier.ValueText == "TestDoSomething"));
                Assert.IsTrue(!methods.ToList().Exists(item => item.Identifier.ValueText == "ThreadStack"));
            }
        }
    }
}