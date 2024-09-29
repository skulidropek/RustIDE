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
        /// <summary>
        /// Map from string to ILanguageRule[].
        /// </summary>
        [JsonPropertyName("tokenizer")]
        public Dictionary<string, MonarchLanguageRule[]> Tokenizer { get; set; }

        /// <summary>
        /// Is the language case insensitive?
        /// </summary>
        [JsonPropertyName("ignoreCase")]
        public bool? IgnoreCase { get; set; }

        /// <summary>
        /// Is the language unicode-aware?
        /// </summary>
        [JsonPropertyName("unicode")]
        public bool? Unicode { get; set; }

        /// <summary>
        /// If no match in the tokenizer, assign this token class (default 'source').
        /// </summary>
        [JsonPropertyName("defaultToken")]
        public string DefaultToken { get; set; }

        /// <summary>
        /// Brackets, e.g., [['{','}','delimiter.curly']]
        /// </summary>
        [JsonPropertyName("brackets")]
        public MonarchLanguageBracket[] Brackets { get; set; }

        /// <summary>
        /// Start symbol in the tokenizer (by default the first entry is used).
        /// </summary>
        [JsonPropertyName("start")]
        public string Start { get; set; }

        /// <summary>
        /// Attach this to every token class (by default '.' + name).
        /// </summary>
        [JsonPropertyName("tokenPostfix")]
        public string TokenPostfix { get; set; }

        /// <summary>
        /// Include line feeds (in the form of a \n character) at the end of lines. Defaults to false.
        /// </summary>
        [JsonPropertyName("includeLF")]
        public bool? IncludeLF { get; set; }

        /// <summary>
        /// Other keys that can be referred to by the tokenizer.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; set; }
    }

    public class MonarchLanguageRule
    {
        [JsonPropertyName("regex")]
        public string Regex { get; set; }  // RegExp converted to string

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
        /// <summary>
        /// The language's comment settings.
        /// </summary>
        [JsonPropertyName("comments")]
        public CommentRule Comments { get; set; }

        /// <summary>
        /// The language's brackets.
        /// This configuration implicitly affects pressing Enter around these brackets.
        /// </summary>
        [JsonPropertyName("brackets")]
        public CharacterPair[] Brackets { get; set; }

        /// <summary>
        /// The language's word definition.
        /// A regex string that matches anything except known separators.
        /// </summary>
        [JsonPropertyName("wordPattern")]
        public string WordPattern { get; set; } // Регулярное выражение в виде строки

        /// <summary>
        /// The language's indentation settings.
        /// </summary>
        [JsonPropertyName("indentationRules")]
        public IndentationRule IndentationRules { get; set; }

        /// <summary>
        /// The language's rules to be evaluated when pressing Enter.
        /// </summary>
        [JsonPropertyName("onEnterRules")]
        public OnEnterRule[] OnEnterRules { get; set; }

        /// <summary>
        /// The language's auto closing pairs.
        /// </summary>
        [JsonPropertyName("autoClosingPairs")]
        public AutoClosingPairConditional[] AutoClosingPairs { get; set; }

        /// <summary>
        /// The language's surrounding pairs.
        /// </summary>
        [JsonPropertyName("surroundingPairs")]
        public AutoClosingPair[] SurroundingPairs { get; set; }

        /// <summary>
        /// Defines a list of bracket pairs that are colorized depending on their nesting level.
        /// </summary>
        [JsonPropertyName("colorizedBracketPairs")]
        public CharacterPair[] ColorizedBracketPairs { get; set; }

        /// <summary>
        /// Defines what characters must be after the cursor for bracket or quote autoclosing to occur.
        /// </summary>
        [JsonPropertyName("autoCloseBefore")]
        public string AutoCloseBefore { get; set; }

        /// <summary>
        /// The language's folding rules.
        /// </summary>
        [JsonPropertyName("folding")]
        public FoldingRules Folding { get; set; }

        /// <summary>
        /// Deprecated character support settings.
        /// </summary>
        [JsonPropertyName("__electricCharacterSupport")]
        public ElectricCharacterSupport ElectricCharacterSupport { get; set; }
    }

    public class ElectricCharacterSupport
    {
        [JsonPropertyName("docComment")]
        public DocComment DocComment { get; set; }
    }

    // Модель для комментариев
    public class CommentRule
    {
        [JsonPropertyName("lineComment")]
        public string LineComment { get; set; } // Может быть null

        [JsonPropertyName("blockComment")]
        public CharacterPair BlockComment { get; set; } // Массив из двух строк
    }

    // Модель для CharacterPair (пара открывающая-закрывающая символов)
    public class CharacterPair
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }
    }

    // Модель для правил отступов
    public class IndentationRule
    {
        [JsonPropertyName("decreaseIndentPattern")]
        public string DecreaseIndentPattern { get; set; } // Регулярное выражение в виде строки

        [JsonPropertyName("increaseIndentPattern")]
        public string IncreaseIndentPattern { get; set; } // Регулярное выражение в виде строки

        [JsonPropertyName("indentNextLinePattern")]
        public string IndentNextLinePattern { get; set; } // Может быть null, регулярное выражение

        [JsonPropertyName("unIndentedLinePattern")]
        public string UnIndentedLinePattern { get; set; } // Может быть null, регулярное выражение
    }

    // Модель для правила авто-закрытия пар символов
    public class AutoClosingPair
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }
    }

    // Модель для условного авто-закрытия пар символов
    public class AutoClosingPairConditional : AutoClosingPair
    {
        [JsonPropertyName("notIn")]
        public List<string> NotIn { get; set; } // Список условий
    }

    // Модель для правил нажатия клавиши Enter
    public class OnEnterRule
    {
        [JsonPropertyName("beforeText")]
        public string BeforeText { get; set; } // Регулярное выражение перед курсором

        [JsonPropertyName("afterText")]
        public string AfterText { get; set; } // Регулярное выражение после курсора

        [JsonPropertyName("previousLineText")]
        public string PreviousLineText { get; set; } // Регулярное выражение для предыдущей строки

        [JsonPropertyName("action")]
        public EnterAction Action { get; set; } // Действие, которое необходимо выполнить
    }

    // Модель для действия при нажатии клавиши Enter
    public class EnterAction
    {
        [JsonPropertyName("indentAction")]
        public string IndentAction { get; set; } // Например, indent или outdent

        [JsonPropertyName("appendText")]
        public string AppendText { get; set; } // Текст для добавления

        [JsonPropertyName("removeText")]
        public int? RemoveText { get; set; } // Количество символов для удаления (может быть null)
    }

    // Модель для правил сворачивания кода
    public class FoldingRules
    {
        [JsonPropertyName("offSide")]
        public bool? OffSide { get; set; } // Является ли язык off-side (как Python)

        [JsonPropertyName("markers")]
        public FoldingMarkers Markers { get; set; } // Маркеры для сворачивания
    }

    // Модель для маркеров сворачивания
    public class FoldingMarkers
    {
        [JsonPropertyName("start")]
        public string Start { get; set; } // Регулярное выражение для начала блока

        [JsonPropertyName("end")]
        public string End { get; set; } // Регулярное выражение для конца блока
    }

    // Модель для комментариев документации
    public class DocComment
    {
        [JsonPropertyName("open")]
        public string Open { get; set; } // Открывающий символ комментария, например '/**'

        [JsonPropertyName("close")]
        public string Close { get; set; } // Закрывающий символ комментария, например '*/' (может быть null)
    }
}
