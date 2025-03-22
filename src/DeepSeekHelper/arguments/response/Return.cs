using System;
using System.Collections.Generic;
using System.Text;
using TheColdWorld.DeepSeekHelper.arguments;

namespace TheColdWorld.DeepSeekHelper.arguments.response;

public class Return(string id, List<Choice> choices, int created)
{
    public string id { get; } = id;
    public List<Choice> Choices { get; } = choices;
    public int created { get; } = created;
    public override string ToString() => $"TheColdWorld.DeepSeekHelper.Return@{base.GetHashCode()}[ID:{id};Created:{created}({DateTimeOffset.FromUnixTimeSeconds(created)});Choice count:{Choices.Count}]";
}
