using System.Text.Json.Serialization;
using System.Text.Json;

namespace RustIDE.Server.Models
{
    public class SyntaxConfig
    {
        [JsonPropertyName("monarchLanguage")]
        public MonarchLanguage MonarchLanguage { get; set; }

        [JsonPropertyName("languageConfiguration")]
        public LanguageConfiguration LanguageConfiguration { get; set; }
    }

    public class MonarchLanguage
    {
        [JsonPropertyName("tokenizer")]
        public Dictionary<string, MonarchLanguageRule[]> Tokenizer { get; set; }

        [JsonPropertyName("ignoreCase")]
        public bool? IgnoreCase { get; set; }

        [JsonPropertyName("unicode")]
        public bool? Unicode { get; set; }

        [JsonPropertyName("defaultToken")]
        public string DefaultToken { get; set; }

        [JsonPropertyName("brackets")]
        public MonarchLanguageBracket[] Brackets { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("tokenPostfix")]
        public string TokenPostfix { get; set; }

        [JsonPropertyName("includeLF")]
        public bool? IncludeLF { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; set; }
    }

    public class MonarchLanguageRule
    {
        [JsonPropertyName("regex")]
        public string Regex { get; set; }
        [JsonPropertyName("action")]
        public MonarchLanguageAction Action { get; set; }

        [JsonPropertyName("include")]
        public string Include { get; set; }
    }

    public class MonarchLanguageAction
    {
        [JsonPropertyName("group")]
        public MonarchLanguageAction[] Group { get; set; }

        [JsonPropertyName("cases")]
        public Dictionary<string, MonarchLanguageAction> Cases { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("switchTo")]
        public string SwitchTo { get; set; }

        [JsonPropertyName("goBack")]
        public int? GoBack { get; set; }

        [JsonPropertyName("bracket")]
        public string Bracket { get; set; }

        [JsonPropertyName("nextEmbedded")]
        public string NextEmbedded { get; set; }

        [JsonPropertyName("log")]
        public string Log { get; set; }
    }

    public class MonarchLanguageBracket
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class LanguageConfiguration
    {
        [JsonPropertyName("comments")]
        public CommentRule Comments { get; set; }

        [JsonPropertyName("brackets")]
        public CharacterPair[] Brackets { get; set; }

        [JsonPropertyName("wordPattern")]
        public string WordPattern { get; set; }
        [JsonPropertyName("indentationRules")]
        public IndentationRule IndentationRules { get; set; }

        [JsonPropertyName("onEnterRules")]
        public OnEnterRule[] OnEnterRules { get; set; }

        [JsonPropertyName("autoClosingPairs")]
        public AutoClosingPairConditional[] AutoClosingPairs { get; set; }

        [JsonPropertyName("surroundingPairs")]
        public AutoClosingPair[] SurroundingPairs { get; set; }

        [JsonPropertyName("colorizedBracketPairs")]
        public CharacterPair[] ColorizedBracketPairs { get; set; }

        [JsonPropertyName("autoCloseBefore")]
        public string AutoCloseBefore { get; set; }

        [JsonPropertyName("folding")]
        public FoldingRules Folding { get; set; }

        [JsonPropertyName("__electricCharacterSupport")]
        public ElectricCharacterSupport ElectricCharacterSupport { get; set; }
    }

    public class ElectricCharacterSupport
    {
        [JsonPropertyName("docComment")]
        public DocComment DocComment { get; set; }
    }

    public class CommentRule
    {
        [JsonPropertyName("lineComment")]
        public string LineComment { get; set; }
        [JsonPropertyName("blockComment")]
        public CharacterPair BlockComment { get; set; }
    }

    public class CharacterPair
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }
    }

    public class IndentationRule
    {
        [JsonPropertyName("decreaseIndentPattern")]
        public string DecreaseIndentPattern { get; set; }
        [JsonPropertyName("increaseIndentPattern")]
        public string IncreaseIndentPattern { get; set; }
        [JsonPropertyName("indentNextLinePattern")]
        public string IndentNextLinePattern { get; set; }
        [JsonPropertyName("unIndentedLinePattern")]
        public string UnIndentedLinePattern { get; set; }
    }

    public class AutoClosingPair
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }
    }

    public class AutoClosingPairConditional : AutoClosingPair
    {
        [JsonPropertyName("notIn")]
        public List<string> NotIn { get; set; }
    }

    public class OnEnterRule
    {
        [JsonPropertyName("beforeText")]
        public string BeforeText { get; set; }
        [JsonPropertyName("afterText")]
        public string AfterText { get; set; }
        [JsonPropertyName("previousLineText")]
        public string PreviousLineText { get; set; }
        [JsonPropertyName("action")]
        public EnterAction Action { get; set; }
    }

    public class EnterAction
    {
        [JsonPropertyName("indentAction")]
        public string IndentAction { get; set; }
        [JsonPropertyName("appendText")]
        public string AppendText { get; set; }
        [JsonPropertyName("removeText")]
        public int? RemoveText { get; set; }
    }

    public class FoldingRules
    {
        [JsonPropertyName("offSide")]
        public bool? OffSide { get; set; }
        [JsonPropertyName("markers")]
        public FoldingMarkers Markers { get; set; }
    }

    public class FoldingMarkers
    {
        [JsonPropertyName("start")]
        public string Start { get; set; }
        [JsonPropertyName("end")]
        public string End { get; set; }
    }

    public class DocComment
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }
        [JsonPropertyName("close")]
        public string Close { get; set; }
    }
}
