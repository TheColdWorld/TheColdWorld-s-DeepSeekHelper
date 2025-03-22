using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.arguments;
using TheColdWorld.DeepSeekHelper.arguments.response;

namespace TheColdWorld.DeepSeekHelper.codec;

public static class ChatCompletionDecoder
{
     public static Return DecodeNoStreamReturn(ref Utf8JsonReader reader)
    {
        if(reader.TokenType!=JsonTokenType.StartObject)reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException(JsonTokenType.StartObject, reader.TokenType);
        string? id=null;List<Choice> choices = new (3); int? created = null; 
        while(reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    string propname=reader.GetString()!;
                    reader.Read();
                    switch(propname)
                    {
                        case "id":id = reader.TokenType != JsonTokenType.String
                                ? throw new InvalidJsonTokenTypeException("id",JsonTokenType.String,reader.TokenType)
                                : reader.GetString(); 
                            break; 
                        case "object": if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("object",JsonTokenType.String,reader.TokenType);
                            else if (reader.GetString() != "chat.completion") throw new JsonException($"invaild object type:{reader.GetString()}"); 
                            else break;
                        case "created": created = reader.TokenType != JsonTokenType.Number
                            ? throw new InvalidJsonTokenTypeException("created",JsonTokenType.Number,reader.TokenType)
                            : reader.GetInt32();
                            break; 
                        case "choices":if (reader.TokenType != JsonTokenType.StartArray) throw new InvalidJsonTokenTypeException("choices", JsonTokenType.StartArray, reader.TokenType);
                            else while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    choices.Add(ReadChoice(ref reader));
                                }
                            break;
                        default:
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{propname}(Return.root)");
#endif
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /*
         while(reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if(reader.TokenType==JsonTokenType.PropertyName)
            {
                string name = reader.GetString()!;
                reader.Read();
                switch (name)
                {
                    case "usage":
                        {
                            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException($"Unexcepted Token Type:{reader.TokenType}");
                            usage = ReadTokenUseage(ref reader);
                            break;
                        }
                    case "model":reader.Skip(); break;
                    case "id":
                        {
                            if (reader.TokenType != JsonTokenType.String) throw new JsonException($"Unexcepted Token Type:{reader.TokenType}");
                            id = reader.GetString()!;
                            break;
                        }
                    case "created":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new JsonException($"Unexcepted Token Type:{reader.TokenType}");
                            created = reader.GetInt32();
                            break;
                        }
                    case "choices":
                        {
                            if(reader.TokenType != JsonTokenType.StartArray) throw new JsonException($"Unexcepted Token Type:{reader.TokenType}");
                            while(reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray) break;
                                if(reader.TokenType==JsonTokenType.StartObject)
                                {
                                    choices.Add(ReadChoice(ref reader));
                                }
                            }
                            break;
                        }
                    case "object":
                        {
                            if (reader.TokenType != JsonTokenType.String) throw new JsonException($"Unexcepted Token Type:{reader.TokenType}");
                            if(reader.GetString() != "chat.completion") throw new JsonException($"Unexcepted Response Type:{reader.GetString()}");
                            break;
                        }
                    default:reader.Skip();break;
                }
            }
        }
         */
        return id is null || choices.Count == 0|| created is null  
            ? throw new JsonException("Incomplete JSON data")
            : new Return(id!, choices!, created!.Value);
    }
    public static Choice ReadChoice(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject) reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException(JsonTokenType.StartObject, reader.TokenType);
        string? Finish_reason = null; uint? Index = null; Message? Message = null; TokenUseage? usage = null;
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    string propname = reader.GetString()!;
                    reader.Read();
                    switch (propname)
                    {
                        case "finish_reason": Finish_reason = reader.TokenType != JsonTokenType.String
                            ? throw new InvalidJsonTokenTypeException("finish_reason", JsonTokenType.String, reader.TokenType)
                            : reader.GetString(); 
                            break;
                        case "index":Index = reader.TokenType != JsonTokenType.Number
                            ? throw new InvalidJsonTokenTypeException("index",JsonTokenType.Number,reader.TokenType)
                            : reader.GetUInt32();
                            break;
                        case "message":Message = reader.TokenType != JsonTokenType.StartObject
                            ? throw new InvalidJsonTokenTypeException("message",JsonTokenType.StartObject,reader.TokenType)
                            : MessageConverter.Instance.Read(ref reader, null!, null!);
                            break;
                        case "usage":usage = reader.TokenType != JsonTokenType.StartObject
                            ? throw new InvalidJsonTokenTypeException("usage",JsonTokenType.StartObject,reader.TokenType)
                            : ReadTokenUseage(ref reader);
                            break;
                        default :
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{propname}(Choice.root)" );
#endif
                            break;
                    }
                    break;
                default:break;
            }
        }
        return Finish_reason is null || Index is null || Message is null || usage is null
            ? throw new JsonException("Incomplete JSON data")
            : new Choice(Finish_reason!, Index!.Value, Message!.Value,usage!.Value);
    }
    public static TokenUseage ReadTokenUseage(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject) reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException(JsonTokenType.StartObject,reader.TokenType);
        int? completion_tokens=null; int? prompt_tokens=null; int? prompt_cache_hit_tokens = null; int? prompt_cache_miss_tokens = null; int? total_tokens = null;int? reasoning_tokens = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string name = reader.GetString()!;
                reader.Read();
                switch (name)
                {
                    case "completion_tokens":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("completion_tokens", JsonTokenType.Number, reader.TokenType);
                            completion_tokens = reader.GetInt32();
                            break;
                        }
                    case "prompt_tokens":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("prompt_tokens", JsonTokenType.Number, reader.TokenType);
                            prompt_tokens = reader.GetInt32();
                            break;
                        }
                    case "prompt_cache_hit_tokens":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("prompt_cache_hit_tokens", JsonTokenType.Number, reader.TokenType);
                            prompt_cache_hit_tokens = reader.GetInt32();
                            break;
                        }
                    case "prompt_cache_miss_tokens":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("prompt_cache_miss_tokens", JsonTokenType.Number, reader.TokenType);
                            prompt_cache_miss_tokens = reader.GetInt32();
                            break;
                        }
                    case "total_tokens":
                        {
                            if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("total_tokens", JsonTokenType.Number, reader.TokenType);
                            total_tokens = reader.GetInt32();
                            break;
                        }
                    case "completion_tokens_details":
                        {
                            if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); continue; }
                            while (reader.Read())
                            {
                                if (reader.TokenType != JsonTokenType.EndObject) break;
                                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString()! == "reasoning_tokens")
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("reasoning_tokens", JsonTokenType.Number, reader.TokenType);
                                    reasoning_tokens = reader.GetInt32();
                                }
                            }
                            break;
                        }
                    default:
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{name}(TokenUseage.root)");
#endif
                        break;
                }
            }
        }
        return completion_tokens is null && prompt_tokens is null && prompt_cache_hit_tokens is null && prompt_cache_miss_tokens is null && total_tokens is null
            ? throw new JsonException("Incomplete JSON data")
            : reasoning_tokens is null
            ? new TokenUseage(completion_tokens!.Value, prompt_tokens!.Value, prompt_cache_hit_tokens!.Value, prompt_cache_miss_tokens!.Value, total_tokens!.Value)
            : new TokenUseage(completion_tokens!.Value, prompt_tokens!.Value, prompt_cache_hit_tokens!.Value, prompt_cache_miss_tokens!.Value, total_tokens!.Value,reasoning_tokens!.Value);
    }
}
