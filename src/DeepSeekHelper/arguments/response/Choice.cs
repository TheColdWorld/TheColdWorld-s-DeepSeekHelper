namespace TheColdWorld.DeepSeekHelper.arguments.response;

public readonly record struct Choice
{
    public readonly string Finish_reason { get; }
    public readonly uint Index { get; }
    public readonly Message Message { get; }
    TokenUseage TokenUseage { get; }
    public Choice(string finish_reason, uint index, Message message,TokenUseage useage)
    {
        Finish_reason = finish_reason;
        Index = index;
        Message = message;
        TokenUseage= useage;
    }
}
