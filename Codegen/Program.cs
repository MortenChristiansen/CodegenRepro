using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Codegen
{
    class Program
    {
        private static readonly CSharpParseOptions _parseOptions = new CSharpParseOptions(languageVersion: LanguageVersion.CSharp7_3);

        static void Main(string[] args)
        {
            var code =
@"using System;

namespace Code
{
    public class Thing
    {
        public int GetNumber()
        {
            return 12;
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(code, _parseOptions);
            var compilation = CSharpCompilation.Create(
                "Things.dll",
                new[] { tree },
                CollectReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release)
            );
            string path = Path.Combine(Directory.GetCurrentDirectory(), compilation.AssemblyName);
            var compilationResult = compilation.Emit(path);

            if (compilationResult.Success)
            {
                Console.WriteLine("Success");
                Console.ReadLine();
            }
            else
            {
                foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                {
                    string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: { codeIssue.Location.GetLineSpan()}, Severity: { codeIssue.Severity}";
                    Console.WriteLine(issue);
                }
                Console.ReadLine();
            }
        }

        private static List<MetadataReference> CollectReferences()
        {
            var loc = typeof(Program).GetTypeInfo().Assembly.Location;
            var path = loc.Substring(0, loc.Length - @"Codegen\bin\Debug\netcoreapp2.1\Codegen.dll".Length) + @"libs\";
            var result = new List<MetadataReference>();
            result.Add(MetadataReference.CreateFromFile(path + "netstandard.dll"));
            return result;
        }
    }
}
