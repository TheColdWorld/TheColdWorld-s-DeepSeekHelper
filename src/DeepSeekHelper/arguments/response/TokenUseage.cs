namespace TheColdWorld.DeepSeekHelper.arguments.response;

public readonly record struct TokenUseage
{
    public readonly int Completion_tokens { get; }
    public readonly int Prompt_tokens{ get; }
    public readonly int Prompt_cache_hit_tokens{ get; }
    public readonly int Prompt_cache_miss_tokens{ get; }
    public readonly int Total_tokens{ get; }
    public readonly int? Reasoning_tokens { get; }
    public TokenUseage(int completion_tokens, int prompt_tokens, int prompt_cache_hit_tokens, int prompt_cache_miss_tokens, int total_tokens)
    {
        this.Completion_tokens = completion_tokens;
        this.Prompt_tokens = prompt_tokens;
        this.Prompt_cache_hit_tokens = prompt_cache_hit_tokens;
        this.Prompt_cache_miss_tokens = prompt_cache_miss_tokens;
        this.Total_tokens = total_tokens;
        Reasoning_tokens = null;
    }
    public TokenUseage(int completion_tokens, int prompt_tokens, int prompt_cache_hit_tokens, int prompt_cache_miss_tokens, int total_tokens, int reasoning_tokens) : this(completion_tokens, prompt_cache_hit_tokens, prompt_cache_hit_tokens, prompt_cache_miss_tokens, total_tokens) => Reasoning_tokens = reasoning_tokens;
    public override String ToString() => Reasoning_tokens is null
        ? $"TheColdWorld.DeepSeekHelper.TokenUseage@{base.GetHashCode()}[Total_tokens:{Total_tokens};Completion_tokens:{Completion_tokens};Prompt_tokens:{Prompt_tokens}(cache hit:{Prompt_cache_hit_tokens};cache miss:{Prompt_cache_miss_tokens});]"
        : $"TheColdWorld.DeepSeekHelper.TokenUseage@{base.GetHashCode()}[Total_tokens:{Total_tokens};Completion_tokens:{Completion_tokens};Reasoning_tokens:{Completion_tokens};Prompt_tokens:{Prompt_tokens}(cache hit:{Prompt_cache_hit_tokens};cache miss:{Prompt_cache_miss_tokens});]";
}
