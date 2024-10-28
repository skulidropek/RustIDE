using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RustIDE.Server.Models;
using System.Reflection;

namespace RustIDE.Server.Services
{
    public class CodeExecutorService
    {
        public async Task<CompilationResult> Compile(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = Directory.GetFiles("Managed")
                                       .Where(s => s.EndsWith(".dll"))
                                       .Where(f => !f.Contains("Newtonsoft.Json.dll"))
                                       .Select(path => MetadataReference.CreateFromFile(path.Trim()))
                                       .ToList();

            var compilation = CSharpCompilation.Create("InMemoryAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            var compilationResult = new CompilationResult();

            if (!result.Success)
            {
                compilationResult.Success = false;
                compilationResult.Errors = result.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => new CompilationError
                    {
                        // Получаем начало ошибки
                        StartLine = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
                        StartColumn = diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1,

                        // Получаем конец ошибки
                        EndLine = diagnostic.Location.GetLineSpan().EndLinePosition.Line + 1,
                        EndColumn = diagnostic.Location.GetLineSpan().EndLinePosition.Character + 1,

                        // Сообщение об ошибке
                        Message = diagnostic.GetMessage(),
                        Severity = diagnostic.Severity.ToString()
                    })
                    .ToList();
            }
            else
            {
                compilationResult.Success = true;
                // Вы можете добавить вывод программы, если это необходимо
            }

            return compilationResult;
        }
    }
}
