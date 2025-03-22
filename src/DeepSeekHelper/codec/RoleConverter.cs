using System.Text.Json.Serialization;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.arguments;

namespace TheColdWorld.DeepSeekHelper.codec;

public class RoleConverter : JsonConverter<Role>
{
    protected RoleConverter() { }
    public static RoleConverter Instance => _instance.Value;
    private static Lazy<RoleConverter> _instance = new(() => new());
    public override Role Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            if (reader.TokenType is JsonTokenType.PropertyName or JsonTokenType.StartObject or JsonTokenType.StartArray) reader.Read();
            else throw new JsonException($"Invaild token type{reader.TokenType}");
        return reader.TokenType != JsonTokenType.String
            ? throw new JsonException($"Invaild token type{reader.TokenType}")
            : reader.GetString() switch
            {
                "system" => Role.system,
                "user" => Role.user,
                "assistant" => Role.assistant,
                _ => throw new JsonException()
            };
    }

    public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            Role.system => "system",
            Role.user => "user",
            Role.assistant => "assistant",
            _ => throw new JsonException()
        });
    }
}
