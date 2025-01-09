using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;


namespace WeArt.Messages
{
    public class ListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, options)
                    ?? new List<T>();
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    public class EnumConverter<T> : JsonConverter<T> where T : Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return default;  // Default is the first element of Enum
            }

            string enumValue = reader.GetString()?.Trim();  

            if (string.IsNullOrEmpty(enumValue))
            {
                return default;  
            }

            foreach (var enumName in Enum.GetNames(typeof(T)))
            {
                if (string.Equals(enumValue, enumName, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)Enum.Parse(typeof(T), enumName);
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
