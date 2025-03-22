

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.arguments;
using TheColdWorld.DeepSeekHelper.arguments.request;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper;


public sealed class Dialogue : IDisposable
{
    public static readonly Uri api_port = new("https://api.deepseek.com/chat/completions");
    //Properties
    public Model Model => model;
    public LinkedList<Message> Messages => messages;
    public short Frequency_penalty
    {
        get => frequency_penalty;
        set
        {
            if (frequency_penalty is not >= -2 and <= 2) throw new ArgumentOutOfRangeException(nameof(frequency_penalty), frequency_penalty, "the range should in [-2,2]");
            frequency_penalty = value;
        }
    }
    public short Presence_penalty
    {
        get => presence_penalty;
        set
        {
            if (presence_penalty is not >= -2 and <= 2) throw new ArgumentOutOfRangeException(nameof(presence_penalty), presence_penalty, "the range should in [-2,2]");
            presence_penalty = value;
        }
    }
    public int Max_Token
    {
        get => max_tokens;
        set
        {
            if (max_tokens is not > 1 and <= 8192) throw new ArgumentOutOfRangeException(nameof(max_tokens), max_tokens, "the range should in [1,8192]");
            max_tokens = value;
        }
    }
    public float Temperature
    {
        get => temperature;
        set
        {
            if (temperature is not >= 0 and <= 2) throw new ArgumentOutOfRangeException(nameof(temperature), temperature, "the range should in [0,2]");
            temperature = value;
        }
    }
    readonly Model model;
    readonly LinkedList<Message> messages;
    short frequency_penalty;
    short presence_penalty;
    int max_tokens;
    float temperature;
    //private 
    bool inputlock = false;
    private readonly byte[] api_key;
    //encrypty
    private readonly byte[] _nonce;
    private readonly byte[] _tag;
    private readonly byte[] _aeskey;
    /// <param name="unencrypted_api_key">key byte(WILL CLEAR!!!)</param>
    public Dialogue(byte[] unencrypted_api_key, Func<byte[]> get_aes_key, Model model)
    {
        _aeskey = EnsureAESkey(get_aes_key);
        _nonce = new byte[12];
        RandomNumberGenerator.Fill(_nonce);
#if NET8_0_OR_GREATER
        using AesGcm aes = new(_aeskey, 12);
#else
        using AesGcm aes = new(_aeskey);
#endif
        api_key = new byte[unencrypted_api_key.Length];
        _tag = new byte[12];
        aes.Encrypt(_nonce, unencrypted_api_key, api_key, _tag);
        CryptographicOperations.ZeroMemory(unencrypted_api_key);
        this.model = model;
        this.messages = new();
        frequency_penalty = 0; presence_penalty = 0; max_tokens = 4096; temperature = 1;
    }
    public Dialogue(string api_key, Func<byte[]> get_aes_key, Model model) : this(System.Text.Encoding.UTF8.GetBytes(api_key), get_aes_key, model) { }
    /// <param name="unencrypted_api_key">key byte(WILL CLEAR!!!)</param>
    public Dialogue(byte[] unencrypted_api_key, Func<byte[]> get_aes_key, Model model, short frequency_penalty = 0, short presence_penalty = 0, int max_tokens = 4096, float temperature = 1) : this(unencrypted_api_key, get_aes_key, model)
    {
        if (frequency_penalty is not >= -2 and <= 2) throw new ArgumentOutOfRangeException(nameof(frequency_penalty), frequency_penalty, "the range should in [-2,2]");
        if (temperature is not >= 0 and <= 2) throw new ArgumentOutOfRangeException(nameof(temperature), temperature, "the range should in [0,2]");
        if (presence_penalty is not >= -2 and <= 2) throw new ArgumentOutOfRangeException(nameof(presence_penalty), presence_penalty, "the range should in [-2,2]");
        if (max_tokens is not > 1 and <= 8192) throw new ArgumentOutOfRangeException(nameof(max_tokens), max_tokens, "the range should in [1,8192]");
        this.frequency_penalty = frequency_penalty;
        this.presence_penalty = presence_penalty;
        this.max_tokens = max_tokens;
        this.temperature = temperature;
    }
    public Dialogue(string api_key, Func<byte[]> get_aes_key, Model model, short frequency_penalty = 0, short presence_penalty = 0, int max_tokens = 4096,  float temperature = 1) : this(System.Text.Encoding.UTF8.GetBytes(api_key), get_aes_key, model, frequency_penalty, presence_penalty, max_tokens,  temperature) { }
    private string Get_api_key()
    {
#if NETSTANDARD2_1
        using AesGcm aes = new(_aeskey);
#else
        using AesGcm aes = new(_aeskey, 12);
#endif
        byte[] decrypted = new byte[api_key.Length];
        aes.Decrypt(_nonce, api_key, _tag, decrypted);
        string token = Encoding.UTF8.GetString(decrypted);
        CryptographicOperations.ZeroMemory(decrypted);
        return token;
    }
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(api_key);
        CryptographicOperations.ZeroMemory(_nonce);
        CryptographicOperations.ZeroMemory(_tag);
        CryptographicOperations.ZeroMemory(_aeskey);
    }
    public static byte[] EnsureAESkey(Func<byte[]> AESkeyProvider)
    {
        if (AESkeyProvider == null) throw new ArgumentNullException(nameof(AESkeyProvider));
        byte[] bytes = AESkeyProvider();
        if (bytes.Length is 16 or 24 or 32)
        {
            return bytes;
        }
        else
        {
            CryptographicOperations.ZeroMemory(bytes);
            throw new ArgumentException("Invaild key", nameof(AESkeyProvider));
        }
    }
    public void AddAssistantContent(string content)
    {
        if (model == Model.deepseek_reasoner)
        {
            if (messages.Count == 0)
            {

            }
            if (messages.Last!.Value.Role == Role.assistant)
            {
                throw new InvalidOperationException("deepseek-reasoner does not support successive user or assistant messages . You should interleave the user/assistant messages in the message sequence.");
            }
        }
        messages.AddLast(new Message(Role.assistant,content));
    }
    public void AddAssistantContent(string content,string reasoning_content)
    {
        if (model == Model.deepseek_reasoner)
        {
            if (messages.Count == 0)
            {

            }
            if (messages.Last!.Value.Role == Role.assistant)
            {
                throw new InvalidOperationException("deepseek-reasoner does not support successive user or assistant messages . You should interleave the user/assistant messages in the message sequence.");
            }
        }
        messages.AddLast(new Message(Role.assistant, content,reasoning_content));
    }
    public void AddSystemContent(string content)
    {
        if (model == Model.deepseek_reasoner && messages.Count != 0) throw new InvalidOperationException("The system message of deepseek-reasoner must be put on the beginning of the message sequence.");
        messages.AddLast(new Message(Role.system,content));
    }
    public HttpRequestMessage CreateSendingMessage(string userContent,bool stream)
    {
        if (inputlock && model == Model.deepseek_reasoner) throw new InvalidOperationException("deepseek-reasoner does not support successive user or assistant messages . You should interleave the user/assistant messages in the message sequence.");
        HttpRequestMessage r = new()
        {
            Method = HttpMethod.Post,
            Content = CreateHttpContent(userContent,stream),
            RequestUri = api_port,
        };
        r.Headers.Connection.Add("keep-alive");
        r.Headers.Authorization = new("Bearer", Get_api_key());
        r.Headers.Accept.Add(new(stream
            ? "text/event-stream" 
            : "application/json"));
        return r;
    }
    private StringContent CreateHttpContent(string userInput,bool stream)
    {
        using MemoryStream bufferWriter = new();
        using Utf8JsonWriter writer = new(bufferWriter);
        writer.WriteStartObject();
        writer.WritePropertyName("model");
        ModelConverter.Instance.Write(writer, model, null!);
        writer.WriteNumber("frequency_penalty", frequency_penalty);
        writer.WriteNumber("max_tokens", max_tokens);
        writer.WriteNumber("presence_penalty", presence_penalty);
        writer.WriteNumber("temperature", temperature);
        writer.WriteBoolean("stream", stream);
        if (stream) writer.WriteBoolean("include_usage", true);
        writer.WritePropertyName("messages");
        writer.WriteStartArray();
        messages.AddLast(new Message(Role.user,userInput));
        foreach (var item in messages)
        {
            MessageConverter.Instance.Write(writer, item, null!);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return new StringContent(System.Text.Encoding.UTF8.GetString(bufferWriter.ToArray()), Encoding.UTF8, "application/json");
    }
}
