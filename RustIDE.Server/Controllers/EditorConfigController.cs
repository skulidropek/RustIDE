using Microsoft.AspNetCore.Mvc;
using RustIDE.Server.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace RustIDE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditorConfigController : ControllerBase
    {
        [HttpGet("syntax")]
        public IActionResult GetSyntaxConfig()
        {
            var syntaxConfig = new SyntaxConfig
            {
                Keywords = GetCSharpKeywords(),
                Operators = new[]
                {
                    "=", ">", "<", "!", "~", "?", ":", "==", "<=", ">=", "!=", "&&", "||", "++", "--", "+", "-", "*", "/",
                    "&", "|", "^", "%", "<<", ">>", ">>>", "+=", "-=", "*=", "/=", "&=", "|=", "^=", "%=", "<<=", ">>=", ">>>=",
                    "??", "?.", "=>", "??"
                },
                Comments = new CommentConfig
                {
                    LineComment = "//",
                    BlockComment = new[] { "/*", "*/" }
                },
                Brackets = new[]
                {
                    new BracketPair { Open = "{", Close = "}" },
                    new BracketPair { Open = "[", Close = "]" },
                    new BracketPair { Open = "(", Close = ")" },
                },
                Tokenizer = new Dictionary<string, TokenRule[]>
                {
                    {
                        "root", new[]
                        {
                            new TokenRule { Regex = @"^\s*#.*$", Action = new TokenAction { Token = "preprocessor" } },
                            new TokenRule { Regex = @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|by|descending|dynamic|equals|from|get|global|group|into|join|let|nameof|on|orderby|partial|remove|select|set|value|var|when|where|yield)\b", Action = new TokenAction { Token = "keyword" } },
                            new TokenRule { Regex = @"\b[A-Z][\w]*\b", Action = new TokenAction { Token = "type" } },
                            new TokenRule { Regex = @"\b[a-zA-Z_]\w*\s*\(", Action = new TokenAction { Token = "function" } },
                            new TokenRule { Include = "common" }
                        }
                    },
                    {
                        "common", new[]
                        {
                            new TokenRule { Regex = @"\/\/.*$", Action = new TokenAction { Token = "comment" } },
                            new TokenRule { Regex = @"\/\*", Action = new TokenAction { Token = "comment", Next = "@comment" } },
                            new TokenRule { Regex = @"@""", Action = new TokenAction { Token = "string.quote", Next = "@verbatimstring" } },
                            new TokenRule { Regex = @"""", Action = new TokenAction { Token = "string.quote", Next = "@string" } },
                            new TokenRule { Regex = @"'", Action = new TokenAction { Token = "string.quote", Next = "@character" } },
                            new TokenRule { Regex = @"\d*\.\d+([eE][\-+]?\d+)?[fFdD]?", Action = new TokenAction { Token = "number.float" } },
                            new TokenRule { Regex = @"0[xX][0-9a-fA-F]+", Action = new TokenAction { Token = "number.hex" } },
                            new TokenRule { Regex = @"\d+[uU]?[lL]?", Action = new TokenAction { Token = "number" } },
                            new TokenRule { Regex = @"\b(true|false|null|this|base)\b", Action = new TokenAction { Token = "keyword" } },
                            new TokenRule { Regex = @"[{}()\[\]]", Action = new TokenAction { Token = "delimiter.bracket" } },
                            new TokenRule { Regex = @"[<>](?!@symbols)", Action = new TokenAction { Token = "delimiter.angle" } },
                            new TokenRule { Regex = @"[;,.]", Action = new TokenAction { Token = "delimiter" } },
                            new TokenRule { Regex = @"@[a-zA-Z_]\w*\b", Action = new TokenAction { Token = "annotation" } },
                            new TokenRule { Regex = @"@symbols", Action = new TokenAction { Token = "operator" } },
                            new TokenRule { Regex = @"[a-zA-Z_]\w*", Action = new TokenAction { Token = "identifier" } },
                        }
                    },
                    {
                        "comment", new[]
                        {
                            new TokenRule { Regex = @"[^\/*]+", Action = new TokenAction { Token = "comment" } },
                            new TokenRule { Regex = @"\/\*", Action = new TokenAction { Token = "comment", Next = "@push" } },
                            new TokenRule { Regex = @"\*\/", Action = new TokenAction { Token = "comment", Next = "@pop" } },
                            new TokenRule { Regex = @"[\/*]", Action = new TokenAction { Token = "comment" } }
                        }
                    },
                    {
                        "string", new[]
                        {
                            new TokenRule { Regex = @"[^\""\\]+", Action = new TokenAction { Token = "string" } },
                            new TokenRule { Regex = @"\\.", Action = new TokenAction { Token = "string.escape" } },
                            new TokenRule { Regex = @"\""", Action = new TokenAction { Token = "string.quote", Next = "@pop" } },
                        }
                    },
                    {
                        "verbatimstring", new[]
                        {
                            new TokenRule { Regex = @"[^""]", Action = new TokenAction { Token = "string" } },
                            new TokenRule { Regex = @"""""", Action = new TokenAction { Token = "string" } },
                            new TokenRule { Regex = @"""", Action = new TokenAction { Token = "string.quote", Next = "@pop" } },
                        }
                    },
                    {
                        "character", new[]
                        {
                            new TokenRule { Regex = @"[^'\\]", Action = new TokenAction { Token = "string" } },
                            new TokenRule { Regex = @"\\.", Action = new TokenAction { Token = "string.escape" } },
                            new TokenRule { Regex = @"'", Action = new TokenAction { Token = "string.quote", Next = "@pop" } },
                        }
                    }
                }
            };

            return Ok(syntaxConfig);
        }

        private static string[] GetCSharpKeywords()
        {
            var keywordKinds = Enum.GetValues(typeof(SyntaxKind))
                .Cast<SyntaxKind>()
                .Where(k => k.ToString().EndsWith("Keyword"))
                .ToArray();

            return keywordKinds.Select(kind => SyntaxFacts.GetText(kind)).Where(keyword => keyword != null).ToArray();
        }
    }
}