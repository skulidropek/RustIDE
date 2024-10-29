using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RustIDE.Server.Models;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using CompletionItem = RustIDE.Server.Models.CompletionItem;

namespace RustIDE.Server.Services
{
    public class CompletionService
    {
        public async Task<IEnumerable<CompletionItem>> GetCompletionsAsync(string code, int position)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Enumerable.Empty<CompletionItem>();
            }

            var document = CreateDocument(code);

            var completionService = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            if (completionService == null)
            {
                return Enumerable.Empty<CompletionItem>();
            }

            var completionList = await completionService.GetCompletionsAsync(document, position);
            if (completionList == null)
            {
                return Enumerable.Empty<CompletionItem>();
            }

            var result = completionList.Items
                .GroupBy(item => item.DisplayText)
                .Select(group => group.First())
                .Select(item => new CompletionItem
                {
                    Label = item.DisplayText,
                    Kind = ConvertCompletionItemKind(item.Tags),
                    InsertText = item.Properties.TryGetValue("InsertText", out var insertText) ? insertText : item.DisplayText,
                    Detail = item.InlineDescription,
                    Documentation = item.Properties.TryGetValue("Documentation", out var doc) ? doc : null,
                    CommitCharacters = item.Rules.CommitCharacterRules.IsDefaultOrEmpty
                        ? null
                        : item.Rules.CommitCharacterRules
                            .SelectMany(rule => rule.Characters)
                            .Select(c => c.ToString())
                            .ToArray(),
                }).ToArray();

            return result;
        }

        private Document CreateDocument(string code)
        {
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var workspace = new AdhocWorkspace(host);

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings(new[] { "System", "System.Linq" });

            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "MyProject",
                "MyAssembly",
                LanguageNames.CSharp,
                isSubmission: false)
                .WithMetadataReferences(GetMetadataReferences())
                .WithCompilationOptions(compilationOptions);

            var project = workspace.AddProject(projectInfo);

            var documentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "MyFile.cs",
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())),
                sourceCodeKind: SourceCodeKind.Regular);

            return workspace.AddDocument(documentInfo);
        }

        private IEnumerable<MetadataReference> GetMetadataReferences()
        {
            var directoryPath = Path.Combine(AppContext.BaseDirectory, "Managed");

            var references = Directory.GetFiles(directoryPath)
                                      .Where(file => file.EndsWith(".dll"))
                                      .Where(file => !file.Contains("Newtonsoft.Json.dll"))
                                      .Select(file => MetadataReference.CreateFromFile(file.Trim()))
                                      .ToList();

            references.AddRange(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation).Assembly.Location)
            });

            return references;
        }

        private string ConvertCompletionItemKind(ImmutableArray<string> tags)
        {
            if (tags.Contains(CompletionTags.Keyword)) return "Keyword";
            if (tags.Contains(CompletionTags.Class)) return "Class";
            if (tags.Contains(CompletionTags.Method)) return "Method";
            if (tags.Contains(CompletionTags.Property)) return "Property";
            if (tags.Contains(CompletionTags.Field)) return "Field";
            if (tags.Contains(CompletionTags.Event)) return "Event";
            if (tags.Contains(CompletionTags.Enum)) return "Enum";
            if (tags.Contains(CompletionTags.Interface)) return "Interface";
            if (tags.Contains("Structure")) return "Struct";
            if (tags.Contains(CompletionTags.Namespace)) return "Namespace";
            return "Variable";
        }
    }
}
