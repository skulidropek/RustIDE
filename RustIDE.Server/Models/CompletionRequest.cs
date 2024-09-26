namespace RustIDE.Server.Models
{
    public class CompletionRequest
    {
        public string Code { get; set; }
        public int Position { get; set; }
    }

    public class CompletionItem
    {
        public string Label { get; set; }
        public string Kind { get; set; }
        public string InsertText { get; set; }
        public string Detail { get; set; }
    }
}
