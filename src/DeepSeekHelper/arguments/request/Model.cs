using System.Text.Json;
using System.Text.Json.Serialization;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper.arguments.request;

[JsonConverter(typeof(ModelConverter))]
public enum Model
{
    deepseek_chat, deepseek_reasoner
}

