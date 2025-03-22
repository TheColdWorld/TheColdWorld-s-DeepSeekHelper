using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.arguments.response;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper;

public sealed class DeepSeekHelper : IDisposable
{
    public delegate void onStreamOutput(string content);
    public delegate void onException(Exception exception);
    public delegate void onTokenUseage(TokenUseage useage);
    readonly HttpClient client = new() { Timeout = Timeout.InfiniteTimeSpan };
    private CancellationTokenSource requestCancellationTokenSource = new();
    public arguments.response.Return Complete(string content, Dialogue dialogue, bool stream,onException? onException=null,Action? beforeReasonging=null, onStreamOutput? onReasoning = null, Action? onEndOfReasoning = null, onStreamOutput? onOutPut = null, Action? afterOutput = null, onTokenUseage? onTokenUseage=null, CancellationToken cancellationToken = default)
    {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,requestCancellationTokenSource.Token);
        return stream
            ? StraemCompleteAsync(content, dialogue,onException, beforeReasonging, onReasoning, onEndOfReasoning, onOutPut,afterOutput,onTokenUseage, cts.Token).GetAwaiter().GetResult()
            : CompleteAsync(content, dialogue, cts.Token).GetAwaiter().GetResult();

    }
    public async Task<arguments.response.Return> StraemCompleteAsync(string content, Dialogue dialogue, onException? onException = null, Action? beforeReasonging = null, onStreamOutput? onReasoning = null, Action? onEndOfReasoning = null, onStreamOutput? onOutPut = null,Action? afterOutput =null,onTokenUseage? onTokenUseage=null, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage r = await client.SendAsync(dialogue.CreateSendingMessage(content, true), cancellationToken);
        HttpRequestException e;bool reasoning_flag = true;bool output_flag = true;
        ResourceManager resourceManager = new("TheColdWorld.DeepSeekHelper.Properties.Resources", typeof(DeepSeekHelper).Assembly);
        switch (r.StatusCode)
        {
            case System.Net.HttpStatusCode.OK:
                {
                    try
                    {
#if NET9_0_OR_GREATER
                        using var stream = await r.Content.ReadAsStreamAsync(cancellationToken);
#else
                        using var stream = await r.Content.ReadAsStreamAsync();
#endif

                        using StreamReader reader = new(stream) ;
                        StringBuilder eventData = new ();
                        StreamChucks chucks=null!;
                        string? line;
#if NET9_0_OR_GREATER
                        while (stream.CanRead && (line = await reader.ReadLineAsync(cancellationToken)) != null)
#else
                        while (stream.CanRead && (line = await reader.ReadLineAsync()) != null)
#endif
                        {
                            if (line.StartsWith("data:"))
                            {
                                if (line[5..].Trim() == "[DONE]") break;
                                eventData.AppendLine(line[5..].Trim());
                            }
                            else if(string.IsNullOrWhiteSpace(line))
                            {
                                if (chucks is not null && chucks.IsDone) break;
                                if (eventData.Length > 0 )
                                {
                                    Utf8JsonReader jsonReader = new(Encoding.UTF8.GetBytes(eventData.ToString()));
                                    StreamChucks.Chuck chuck = new(ref jsonReader);
                                    chucks ??= new(chuck.id,chuck.created,chuck.model);
                                    chucks.AddChuck(chuck);
                                    if (chuck.choice.is_reasoning_content) {
                                        if (reasoning_flag) {beforeReasonging?.Invoke(); reasoning_flag = false; }
                                        onReasoning?.Invoke(chuck.choice.content); 
                                    }
                                    else
                                    {
                                        if(output_flag)
                                        {
                                            onEndOfReasoning?.Invoke();
                                            output_flag = false;
                                        }
                                        onOutPut?.Invoke(chuck.choice.content);
                                    }
                                        eventData.Clear();
                                }
                            }
                            else if (line.StartsWith(": keep-alive")) continue;
                        }
                        if (chucks is null)
                        {
                            e = new HttpRequestException("Empty content");
                            onException?.Invoke(e);
                            throw e;
                        }
                        if(!chucks.IsDone)
                        {
                            e = new HttpRequestException("Uncompleted content");
                            onException?.Invoke(e);
                            throw e;
                        }
                        afterOutput?.Invoke();
                        onTokenUseage?.Invoke(chucks.Useage);
                        return chucks.GetReturn();
                    }
                    catch (Exception ex)
                    {
                        onException?.Invoke(ex);
                        throw;
                    }
                }
            case System.Net.HttpStatusCode.BadRequest:
               e = new("Internal error");
                onException?.Invoke(e);
                throw e;
            case System.Net.HttpStatusCode.Unauthorized: 
                e = new HttpRequestException(resourceManager.GetString("AuthenticationFails"));
                onException?.Invoke(e);
                throw e;
            case System.Net.HttpStatusCode.PaymentRequired:
                e = new HttpRequestException(resourceManager.GetString("InsufficientBalance"));
                onException?.Invoke(e);
                throw e;
            case System.Net.HttpStatusCode.TooManyRequests:
                e = new HttpRequestException(resourceManager.GetString("RateLimitReached"));
                onException?.Invoke(e);
                throw e;
            case System.Net.HttpStatusCode.InternalServerError:
                e = new HttpRequestException(resourceManager.GetString("ServerError"));
                onException?.Invoke(e);
                throw e;
            case System.Net.HttpStatusCode.ServiceUnavailable:
                e = new HttpRequestException("ServerOverloaded");
                onException?.Invoke(e);
                throw e;
            default: { r.EnsureSuccessStatusCode(); e= new HttpRequestException($"Invaild status code{r.StatusCode}"); onException?.Invoke(e);throw e; }
        }
    }
    public async Task<arguments.response.Return> CompleteAsync(string content, Dialogue dialogue, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage r = await client.SendAsync(dialogue.CreateSendingMessage(content, false), cancellationToken);
        ComponentResourceManager resourceManager = new(typeof(DeepSeekHelper));
        switch (r.StatusCode)
        {
            case System.Net.HttpStatusCode.OK:
                {
                    Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(await r.Content.ReadAsStringAsync() ?? throw new HttpRequestException("empty result")));
                    return ChatCompletionDecoder.DecodeNoStreamReturn(ref reader);
                }
            case System.Net.HttpStatusCode.BadRequest:
                throw new HttpRequestException("internal error"); 
            case System.Net.HttpStatusCode.Unauthorized:
                throw new HttpRequestException(resourceManager.GetString("AuthenticationFails"));
            case System.Net.HttpStatusCode.PaymentRequired: 
                throw new HttpRequestException(resourceManager.GetString("InsufficientBalance"));
            case System.Net.HttpStatusCode.TooManyRequests:
                throw new HttpRequestException(resourceManager.GetString("RateLimitReached"));
            case System.Net.HttpStatusCode.InternalServerError:
                throw new HttpRequestException(resourceManager.GetString("ServerError"));
            case System.Net.HttpStatusCode.ServiceUnavailable:
                throw new HttpRequestException("ServerOverloaded");
            default: { r.EnsureSuccessStatusCode(); throw new HttpRequestException($"Invaild status code{r.StatusCode}"); }
        }
    }

    public void Dispose()
    {
        requestCancellationTokenSource.Cancel();
        client.Dispose();
        requestCancellationTokenSource?.Dispose();
    }
}
