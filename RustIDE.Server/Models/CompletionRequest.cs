using System.Text.Json.Serialization;

namespace RustIDE.Server.Models
{
    public class CompletionRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }
    }

    public class CompletionItem
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("insertText")]
        public string InsertText { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("documentation")]
        public string Documentation { get; set; }

        [JsonPropertyName("commitCharacters")]
        public string[] CommitCharacters { get; set; }

        [JsonPropertyName("insertTextRules")]
        public string InsertTextRules { get; set; }
    }
}
