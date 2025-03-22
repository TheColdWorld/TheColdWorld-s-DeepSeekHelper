using System;
using System.Collections.Generic;

using System.Text;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper.codec;

namespace TheColdWorld.DeepSeekHelper.arguments.response;

public record class StreamChucks(string id,  int created,string model)
{

    public readonly string id = id;
    public readonly int created = created;
    public readonly string model = model;
    public bool IsDone { get; protected set; }
    public TokenUseage Useage => !IsDone ? throw new InvalidOperationException("Chuck is not completed!") : chuckChoices.Last!.Value.useage!.Value;
    readonly LinkedList<Chuck.Choice> chuckChoices = new();
    public Return GetReturn()
    {
        if (!IsDone) throw new InvalidOperationException("chuck is not completed!");
        StringBuilder? reasoning_content = null; StringBuilder content = new(chuckChoices.Count*5);
        lock (chuckChoices)
        {
            if (chuckChoices.First!.Value.is_reasoning_content) { reasoning_content = new(chuckChoices.Count*5); }
            foreach (var item in chuckChoices)
            {
                if (item.is_reasoning_content) reasoning_content!.Append(item.content);
                else content.Append(item.content);
            }
        }
        return new Return(id, 
            [new Choice(chuckChoices.Last!.Value.finish_reason!,0u,reasoning_content is null ? new(Role.assistant, content.ToString()) : new(Role.assistant, content.ToString(),reasoning_content.ToString()), chuckChoices.Last!.Value.useage!.Value)]
            ,created);
    }
    public bool AddChuck(Chuck chuck)
    {
        if( IsDone ) return false;
        try
        {
            if (chuck.id != id  ||chuck.created != created)return false;
            chuckChoices.AddLast(chuck.choice);
        }
        catch (Exception)
        {
            return false;
        }
        if (chuck.choice.finish_reason != null && chuck.choice.useage is not null)
        {
           IsDone = true;
        }
        return true;
    }
    public bool AddChuck(Utf8JsonReader jsonReader) => AddChuck(new Chuck(ref jsonReader));
    public class Chuck
    {
        public readonly string id;
        public readonly string system_fingerprint;
        public readonly int created;
        public readonly string model;
        public readonly Choice choice;
        public Chuck(ref Utf8JsonReader jsonReader)
        {
            if (jsonReader.TokenType != JsonTokenType.StartObject) jsonReader.Read();
            if (jsonReader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException( JsonTokenType.StartObject, jsonReader.TokenType);
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonTokenType.EndObject) break;
                if (jsonReader.TokenType == JsonTokenType.PropertyName)
                {
                    string name = jsonReader.GetString()!;
                    jsonReader.Read();
                    switch (name)
                    {
                        case "id":
                            {
                                if (jsonReader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("id", JsonTokenType.String, jsonReader.TokenType);
                                id = jsonReader.GetString()!;
                                break;
                            }
                        case "system_fingerprint":
                            {
                                if (jsonReader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("system_fingerprint", JsonTokenType.String, jsonReader.TokenType);
                                system_fingerprint = jsonReader.GetString()!;
                                break;
                            }
                        case "model":
                            {
                                if (jsonReader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("model", JsonTokenType.String, jsonReader.TokenType);
                                model = jsonReader.GetString()!;
                                break;
                            }
                        case "created":
                            {
                                if (jsonReader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("created", JsonTokenType.Number, jsonReader.TokenType);
                                created = jsonReader.GetInt32();
                                break;
                            }
                        case "choices":
                            {
                                choice = ReadChoice(ref jsonReader);
                                break;
                            }
                        case "object":
                            {
                                if (jsonReader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("object", JsonTokenType.String, jsonReader.TokenType);
                                if (jsonReader.GetString() != "chat.completion.chunk") throw new JsonException($"Invaild object type:{jsonReader.GetString()}");
                                break;
                            }
                        default:
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{name}(Chuck.root)");
#endif
                            continue;
                    }
                }

            }
            if(id is null || system_fingerprint is null || model is null || choice is null) throw new JsonException("Incomplete JSON data");
        }

        private static Choice ReadChoice(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray) reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) throw new InvalidJsonTokenTypeException( JsonTokenType.StartArray, reader.TokenType);
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException( JsonTokenType.StartObject, reader.TokenType);
            uint? index = null; string? finish_reason = null; string? content = null; string? reasoning_content = null;TokenUseage? useage = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string name = reader.GetString()!;
                    reader.Read();
                    switch (name)
                    {
                        default:
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{name}(Choice.root)");
#endif
                            break;
                        case "index":
                            {
                                if (reader.TokenType != JsonTokenType.Number) throw new InvalidJsonTokenTypeException("index",JsonTokenType.Number,reader.TokenType);
                                index = reader.GetUInt32();
                                break;
                            }
                        case "finish_reason":
                            {
                                if (reader.TokenType == JsonTokenType.Null) break;
                                if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("finish_reason", JsonTokenType.String, reader.TokenType);
                                finish_reason = reader.GetString();
                                break;
                            }
                        case "usage":
                            {
                                if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException("usage", JsonTokenType.StartObject, reader.TokenType);
                                useage = ChatCompletionDecoder.ReadTokenUseage(ref reader); 
                                break;
                            }
                        case "delta":
                            {
                                if (reader.TokenType != JsonTokenType.StartObject) throw new InvalidJsonTokenTypeException("delta", JsonTokenType.StartObject, reader.TokenType);
                                while (reader.Read())
                                {
                                    if (reader.TokenType == JsonTokenType.EndObject) break;
                                    if (reader.TokenType == JsonTokenType.PropertyName)
                                    {
                                        string deltaProp = reader.GetString()!; 
                                        reader.Read();
                                        switch (deltaProp)
                                        {
                                            default:
#if DEBUG
                                                System.Diagnostics.Debug.WriteLine($"[TheColdWorld's DeepSeekHelper]Skipped protety:{deltaProp}(Choice.delta)");
#endif
                                                break;
                                            case "content":
                                                {
                                                    if (reader.TokenType == JsonTokenType.Null) continue;
                                                    if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("content", JsonTokenType.String, reader.TokenType);
                                                    content = reader.GetString()!;
                                                    break;
                                                }
                                            case "reasoning_content":
                                                {
                                                    if (reader.TokenType == JsonTokenType.Null) continue;
                                                    if (reader.TokenType != JsonTokenType.String) throw new InvalidJsonTokenTypeException("reasoning_content", JsonTokenType.String, reader.TokenType);
                                                    reasoning_content = reader.GetString();
                                                    break;
                                                }
                                        }
                                    }
                                }
                                if(content is null && reasoning_content is null ) throw new JsonException("empty delta"); 
                                break;
                            }
                    }
                }
            }
            while (reader.Read()) if (reader.TokenType == JsonTokenType.EndArray) break;
            if((content is null && reasoning_content is null) || index is null) throw new JsonException("Incomplete JSON data");
            if ((content is not null && reasoning_content is not null)) throw new JsonException("Invaild JSON data");
            else
            {
                bool is_reasoning_content= content is null;
                 return new(is_reasoning_content ? reasoning_content! : content!, finish_reason, is_reasoning_content, index!.Value, useage);
            }
        }
        public record class Choice
        {
            public Choice(string content, string? finish_reason, bool is_reasoning_content, uint index,TokenUseage? useage=null)
            {
                this.content = content;
                this.finish_reason = finish_reason;
                this.index = index;
                this.is_reasoning_content = is_reasoning_content;
                this.useage= useage;
            }
            public Choice(string content, string? finish_reason, bool is_reasoning_content, uint index)
            {
                this.content = content;
                this.finish_reason = finish_reason;
                this.index = index;
                this.is_reasoning_content = is_reasoning_content;
            }
            public TokenUseage? useage;
            public string content { get; }
            public bool is_reasoning_content { get; }
            public uint index { get; }
            public string? finish_reason { get; }
        }
    }
}