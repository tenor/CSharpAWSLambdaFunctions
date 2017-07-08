using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PackageBuilder.Models;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PackageBuilder
{
    public static class ScriptCompiler
    {
        public static CompilationResult Compile(string code)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code, options: new CSharpParseOptions(kind: SourceCodeKind.Script));

            var privCorLibLocation = typeof(object).GetTypeInfo().Assembly.Location;
            var corlibLocation = Path.Combine(Path.GetDirectoryName(privCorLibLocation), "mscorlib.dll");
            var sysRuntimeLocation = Path.Combine(Path.GetDirectoryName(privCorLibLocation), "System.Runtime.dll");

            var compilation = CSharpCompilation.Create("DotNetFunction", 
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, scriptClassName: "Function" ))
                .AddReferences(MetadataReference.CreateFromFile(privCorLibLocation)
                    , MetadataReference.CreateFromFile(corlibLocation)
                    , MetadataReference.CreateFromFile(sysRuntimeLocation)
                    , MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location) //Import MS.CSharp
                    , MetadataReference.CreateFromFile(typeof(XmlDocument).GetTypeInfo().Assembly.Location) //Import XmlDocument
                    , MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XDocument).GetTypeInfo().Assembly.Location) //Import XDocument
                    , MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpRequestMessage).GetTypeInfo().Assembly.Location)
                    , MetadataReference.CreateFromFile(typeof(System.Net.HttpStatusCode).GetTypeInfo().Assembly.Location)
                    , MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly.Location)
                    )
                .AddSyntaxTrees(tree);

            var result = new CompilationResult()
            {
                Diagnostics = compilation.GetDiagnostics()
            };

            if (result.Diagnostics != null && !result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                MemoryStream ms = new MemoryStream();
                var emitResult = compilation.Emit(ms);
                result.OutputBinary = ms.ToArray();

            }

            return result;

        }
    }
}
