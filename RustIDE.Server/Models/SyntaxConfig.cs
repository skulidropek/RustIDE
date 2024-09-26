using System.Text.Json.Serialization;
using System.Text.Json;

namespace RustIDE.Server.Models
{
    public class SyntaxConfig
    {
        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        [JsonPropertyName("operators")]
        public string[] Operators { get; set; }

        [JsonPropertyName("comments")]
        public CommentConfig Comments { get; set; }

        [JsonPropertyName("brackets")]
        public BracketPair[] Brackets { get; set; }

        [JsonPropertyName("tokenizer")]
        public Dictionary<string, TokenRule[]> Tokenizer { get; set; }
    }

    public class CommentConfig
    {
        [JsonPropertyName("lineComment")]
        public string LineComment { get; set; }

        [JsonPropertyName("blockComment")]
        public string[] BlockComment { get; set; }
    }

    public class BracketPair
    {
        [JsonPropertyName("open")]
        public string Open { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }
    }

    public class TokenRule
    {
        [JsonPropertyName("regex")]
        public string Regex { get; set; }

        [JsonPropertyName("action")]
        [JsonConverter(typeof(TokenActionConverter))]
        public TokenAction Action { get; set; }

        [JsonPropertyName("include")]
        public string Include { get; set; }
    }

    public class TokenAction
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("cases")]
        public Dictionary<string, string> Cases { get; set; }
    }

    public class TokenActionConverter : JsonConverter<TokenAction>
    {
        public override TokenAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new TokenAction { Token = reader.GetString() };
            }

            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;

                var action = new TokenAction();

                if (root.TryGetProperty("token", out JsonElement tokenElement))
                {
                    action.Token = tokenElement.GetString();
                }

                if (root.TryGetProperty("next", out JsonElement nextElement))
                {
                    action.Next = nextElement.GetString();
                }

                if (root.TryGetProperty("cases", out JsonElement casesElement))
                {
                    action.Cases = JsonSerializer.Deserialize<Dictionary<string, string>>(casesElement.GetRawText(), options);
                }

                return action;
            }
        }

        public override void Write(Utf8JsonWriter writer, TokenAction value, JsonSerializerOptions options)
        {
            if (value.Cases == null && value.Next == null)
            {
                writer.WriteStringValue(value.Token);
            }
            else
            {
                writer.WriteStartObject();

                if (!string.IsNullOrEmpty(value.Token))
                {
                    writer.WriteString("token", value.Token);
                }

                if (!string.IsNullOrEmpty(value.Next))
                {
                    writer.WriteString("next", value.Next);
                }

                if (value.Cases != null)
                {
                    writer.WritePropertyName("cases");
                    JsonSerializer.Serialize(writer, value.Cases, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
