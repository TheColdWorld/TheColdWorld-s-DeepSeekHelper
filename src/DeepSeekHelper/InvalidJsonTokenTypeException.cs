using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace TheColdWorld.DeepSeekHelper;
/// <summary>
/// Thrown when a JSON token type does not match the expected type
/// </summary>
public class InvalidJsonTokenTypeException :JsonException
{
    public InvalidJsonTokenTypeException(string propertyName, JsonTokenType exceptedTokenType, JsonTokenType actualTokenType) : base($"Expected {exceptedTokenType} in property \'{propertyName}\',Actual: {actualTokenType}") { ExpectedTokenType = exceptedTokenType;ActualTokenType = actualTokenType;PropertyName = propertyName; }
    public InvalidJsonTokenTypeException( JsonTokenType exceptedTokenType, JsonTokenType actualTokenType) :base($"Expected {exceptedTokenType} , Actual: {actualTokenType}") { ExpectedTokenType = exceptedTokenType;ActualTokenType = actualTokenType; }
    public InvalidJsonTokenTypeException(string propertyName, JsonTokenType exceptedTokenType, JsonTokenType actualTokenType,Exception innerException) : base($"Expected {exceptedTokenType} in \'{propertyName}\',Actual: {actualTokenType}",innerException) { ExpectedTokenType = exceptedTokenType; ActualTokenType = actualTokenType; PropertyName = propertyName; }
    public InvalidJsonTokenTypeException(JsonTokenType exceptedTokenType, JsonTokenType actualTokenType, Exception innerException) : base($"Expected {exceptedTokenType} , Actual: {actualTokenType}",innerException) { ExpectedTokenType = exceptedTokenType; ActualTokenType = actualTokenType; }
    public JsonTokenType ExpectedTokenType { get; }
    public JsonTokenType ActualTokenType { get; }
    public string? PropertyName { get; }
}
