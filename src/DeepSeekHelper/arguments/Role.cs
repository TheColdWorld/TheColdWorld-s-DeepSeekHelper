using System.Text.Json;
using System.Text.Json.Serialization;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper.arguments;


[JsonConverter(typeof(RoleConverter))]
public enum Role
{
    system, user, assistant
}

