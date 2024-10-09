using Microsoft.AspNetCore.Mvc;
using RustIDE.Server.Models;
using RustIDE.Server.Services;
using System.Collections.Generic;

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditorConfigController : ControllerBase
    {
        private readonly HooksService _hooksService;
        public EditorConfigController(HooksService hooksService)
        {
            _hooksService = hooksService;
        }

        [HttpGet("syntax")]
        public ActionResult<SyntaxConfig> GetSyntaxConfig()
        {
            var hooks = _hooksService.Hooks().Select(s => s.Name).ToArray();

            var syntaxConfig = new SyntaxConfig
            {
                MonarchLanguage = new MonarchLanguage
                {
                    Tokenizer = new Dictionary<string, MonarchLanguageRule[]>
                    {
                        {
                            "root", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"^\s*#.*$",
                                    Action = new MonarchLanguageAction { Token = "preprocessor" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @$"\b({string.Join('|', hooks)}|abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|by|descending|dynamic|equals|from|get|global|group|into|join|let|nameof|on|orderby|partial|remove|select|set|value|var|when|where|yield)\b",
                                    Action = new MonarchLanguageAction { Token = "keyword" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\b[A-Z][\w]*\b",
                                    Action = new MonarchLanguageAction { Token = "type" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\b[a-zA-Z_]\w*\s*\(",
                                    Action = new MonarchLanguageAction { Token = "function" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\/\/.*$",
                                    Action = new MonarchLanguageAction { Token = "comment" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\/\*",
                                    Action = new MonarchLanguageAction { Token = "comment", Next = "@comment" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"@""",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@verbatimstring" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"""",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@string" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"'",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@character" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\d*\.\d+([eE][\-+]?\d+)?[fFdD]?",
                                    Action = new MonarchLanguageAction { Token = "number.float" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"0[xX][0-9a-fA-F]+",
                                    Action = new MonarchLanguageAction { Token = "number.hex" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\d+[uU]?[lL]?",
                                    Action = new MonarchLanguageAction { Token = "number" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"\b(true|false|null|this|base)\b",
                                    Action = new MonarchLanguageAction { Token = "keyword" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"[{}()\[\]]",
                                    Action = new MonarchLanguageAction { Token = "delimiter.bracket" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"[;,.]",
                                    Action = new MonarchLanguageAction { Token = "delimiter" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"@[a-zA-Z_]\w*\b",
                                    Action = new MonarchLanguageAction { Token = "annotation" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"[=><!~?:&|+\-*\/\^%]+",
                                    Action = new MonarchLanguageAction { Token = "operator" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"[a-zA-Z_]\w*",
                                    Action = new MonarchLanguageAction { Token = "identifier" }
                                }
                            }
                        },
                        {
                            "comment", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"\*\/",
                                    Action = new MonarchLanguageAction { Token = "comment", Next = "@pop" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @".",
                                    Action = new MonarchLanguageAction { Token = "comment" }
                                }
                            }
                        },
                        {
                            "string", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"\\.",
                                    Action = new MonarchLanguageAction { Token = "string.escape" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"""",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@pop" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @".",
                                    Action = new MonarchLanguageAction { Token = "string" }
                                }
                            }
                        },
                        {
                            "verbatimstring", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"""""",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@pop" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @".",
                                    Action = new MonarchLanguageAction { Token = "string" }
                                }
                            }
                        },
                        {
                            "character", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"\\.",
                                    Action = new MonarchLanguageAction { Token = "string.escape" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @"'",
                                    Action = new MonarchLanguageAction { Token = "string.quote", Next = "@pop" }
                                },
                                new MonarchLanguageRule
                                {
                                    Regex = @".",
                                    Action = new MonarchLanguageAction { Token = "string" }
                                }
                            }
                        },
                        {
                            "symbols", new[]
                            {
                                new MonarchLanguageRule
                                {
                                    Regex = @"[=><!~?:&|+\-*\/\^%]+",
                                    Action = new MonarchLanguageAction { Token = "operator" }
                                }
                            }
                        }
                    },
                    IgnoreCase = false,
                    DefaultToken = "source",
                    Brackets = new[]
                    {
                        new MonarchLanguageBracket { Open = "{", Close = "}", Token = "delimiter.curly" },
                        new MonarchLanguageBracket { Open = "[", Close = "]", Token = "delimiter.square" },
                        new MonarchLanguageBracket { Open = "(", Close = ")", Token = "delimiter.parenthesis" }
                    },
                    Start = "root",
                    TokenPostfix = ".cs",
                },
                LanguageConfiguration = new LanguageConfiguration
                {
                    Comments = new CommentRule
                    {
                        LineComment = "//",
                        BlockComment = new CharacterPair { Open = "/*", Close = "*/" }
                    },
                    Brackets = new[]
                    {
                        new CharacterPair { Open = "{", Close = "}" },
                        new CharacterPair { Open = "[", Close = "]" },
                        new CharacterPair { Open = "(", Close = ")" }
                    },
                    AutoClosingPairs = new[]
                    {
                        new AutoClosingPairConditional { Open = "{", Close = "}", NotIn = new List<string>() },
                        new AutoClosingPairConditional { Open = "[", Close = "]", NotIn = new List<string>() },
                        new AutoClosingPairConditional { Open = "(", Close = ")", NotIn = new List<string>() },
                        new AutoClosingPairConditional { Open = "\"", Close = "\"", NotIn = new List<string>() },
                        new AutoClosingPairConditional { Open = "'", Close = "'", NotIn = new List<string>() }
                    },
                    SurroundingPairs = new[]
                    {
                        new AutoClosingPair { Open = "{", Close = "}" },
                        new AutoClosingPair { Open = "[", Close = "]" },
                        new AutoClosingPair { Open = "(", Close = ")" },
                        new AutoClosingPair { Open = "\"", Close = "\"" },
                        new AutoClosingPair { Open = "'", Close = "'" }
                    }
                }
            };

            return syntaxConfig;
        }
    }
}
