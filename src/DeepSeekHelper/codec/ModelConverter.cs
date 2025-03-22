using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.arguments.request;

namespace TheColdWorld.DeepSeekHelper.codec;

public class ModelConverter : JsonConverter<Model>
{
    protected ModelConverter() { }
    public static ModelConverter Instance => _instance.Value;
    private static Lazy<ModelConverter> _instance = new(() => new());
    public override Model Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            if (reader.TokenType is JsonTokenType.PropertyName or JsonTokenType.StartObject or JsonTokenType.StartArray) reader.Read();
            else throw new JsonException($"Invaild token type{reader.TokenType}");
        return reader.TokenType != JsonTokenType.String
            ? throw new JsonException($"Invaild token type{reader.TokenType}")
            : reader.GetString() switch
            {
                "deepseek_chat" => Model.deepseek_chat,
                "deepseek_reasoner" => Model.deepseek_reasoner,
                _ => throw new JsonException($"Unknown model: {reader.GetString()}")
            };
    }

    public override void Write(Utf8JsonWriter writer, Model value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            Model.deepseek_chat => "deepseek-chat",
            Model.deepseek_reasoner => "deepseek-reasoner",
            _ => throw new JsonException("Invalid Model value")
        });
    }
}
