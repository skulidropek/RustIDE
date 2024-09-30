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

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompletionController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CompletionItem>>> GetCompletions([FromBody] CompletionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Invalid request or empty code.");
            }

            var document = CreateDocument(request.Code);

            var completionService = CompletionService.GetService(document);
            if (completionService == null)
            {
                return BadRequest("Completion service is not available for the provided document.");
            }

            var completionList = await completionService.GetCompletionsAsync(document, request.Position);
            if (completionList == null)
            {
                return Ok(Enumerable.Empty<CompletionItem>());
            }

            // Фильтрация дубликатов по DisplayText
            var result = completionList.Items
                .GroupBy(item => item.DisplayText)
                .Select(group => group.First())  // Оставляем только первый элемент из группы с одинаковыми DisplayText
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
            // Создаем MEF host и рабочее пространство
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var workspace = new AdhocWorkspace(host);

            // Настройка проекта
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings(new[] { "System", "System.Linq" });

            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "MyProject",
                "MyAssembly",
                LanguageNames.CSharp,
                isSubmission: false)  // Указываем, что это не скрипт
                .WithMetadataReferences(GetMetadataReferences())
                .WithCompilationOptions(compilationOptions);

            var project = workspace.AddProject(projectInfo);

            // Создаем документ с SourceCodeKind.Regular
            var documentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "MyFile.cs",
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())),
                sourceCodeKind: SourceCodeKind.Regular);

            var document = workspace.AddDocument(documentInfo);

            return document;
        }

        private IEnumerable<MetadataReference> GetMetadataReferences()
        {
            var directoryPath = @"C:\RustServer 2.0\rustserver\RustDedicated_Data\Managed";

            var references = Directory.GetFiles(directoryPath)
                                      .Where(file => file.EndsWith(".dll"))
                                      .Where(file => !file.Contains("Newtonsoft.Json.dll"))
                                      .Select(file => MetadataReference.CreateFromFile(file.Trim()))
                                      .ToList();

            // Добавляем стандартные сборки
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
