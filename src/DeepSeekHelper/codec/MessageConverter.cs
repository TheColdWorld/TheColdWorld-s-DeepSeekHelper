using System.Text.Json;
using System.Text.Json.Serialization;
using TheColdWorld.DeepSeekHelper.arguments;

namespace TheColdWorld.DeepSeekHelper.codec;


    public class MessageConverter : JsonConverter<Message>
    {
        protected MessageConverter() { }
        public static MessageConverter Instance => _instance.Value;
        private static readonly Lazy<MessageConverter> _instance = new(() => new());
        public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException(JsonTokenType.StartObject, reader.TokenType);
            Role? role = null; string? content = null, reasoning_content = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string name = reader.GetString()!;
                    reader.Read();
                    switch (name)
                    {
                        case "role":
                            {
                            if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("role", JsonTokenType.String, reader.TokenType);
                            role = RoleConverter.Instance.Read(ref reader, null!, null!);
                                break;
                            }
                        case "content":
                            {
                            if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("content", JsonTokenType.String, reader.TokenType);
                            content = reader.GetString()!.Replace("\n\n","\n");
                                break;
                            }
                        case "reasoning_content":
                            {
                            if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("reasoning_content", JsonTokenType.String, reader.TokenType);
                            reasoning_content = reader.GetString()!.Replace("\n\n", "\n");
                                break;
                            }
                        default:  break;
                }
                }

            }
            return role is null || content is null
                    ? throw new JsonException("Incomplete JSON data")
                    : reasoning_content is null
                        ? new(role.Value, content!)
                        : new(role.Value, content!, reasoning_content!);
        }

        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("role");
            RoleConverter.Instance.Write(writer,value.Role,options);
            writer.WriteString("content", value.Content);
            writer.WriteEndObject();
        }
    }

