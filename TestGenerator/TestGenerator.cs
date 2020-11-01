using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGen
{
    public class TestGenerator
    {
        private int count = 0;

        public TestInfo Generate(string SourceCode)
        {
            return new TestInfo() { FileName = $"{count++}.cs", TestCode = SourceCode + "\nmodified" };
        }
    }
}
