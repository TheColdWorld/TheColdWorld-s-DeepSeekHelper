using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper.arguments;

[JsonConverter(typeof(MessageConverter))]
public readonly  record struct Message
{
    public readonly Role Role { get; }
    public readonly string Content { get; }

    [JsonIgnore] 
    public readonly string? reasoning_content;
    public Message(Role role, string content)
    {
        Role = role;
        Content = content;
        reasoning_content = null;
    }
    public Message(Role role, string content, string reasoning_content) : this(role, content) => this.reasoning_content = reasoning_content;
}
