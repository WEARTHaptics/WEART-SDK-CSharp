using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WeArt.Messages
{
    /// <summary>
    /// Internal class used to serialize and deserialize messages to/from the middleware
    /// using a simple string concatenation strategy.
    /// </summary>
    internal class WeArtMessageCustomSerializer : WeArtMessageSerializer
    {
        private const char separator = ':';

        internal class WeArtMiddlewareMessageIDAttribute : Attribute
        {
            public string ID;

            public WeArtMiddlewareMessageIDAttribute(string id) => ID = id;
        }

        internal class JsonMessageTemplate
        {
            [JsonPropertyName("ts")]
            [JsonConverter(typeof(MillisecondsEpochConverter))]
            public DateTime Timestamp { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("data")]
            public object Data { get; set; }  // Can hold either DeviceStatus, MiddlewareStatusMessage, etc.
        }

        public override bool Serialize(IWeArtMessage message, out string data)
        {
            try
            {
                if (message is IWeArtMessageCustomSerialization serializableMessage)
                {
                    data = string.Join(separator.ToString(), serializableMessage.Serialize());
                    return true;
                }

                var messageType = message.GetType();
                var reflectionData = ReflectionData.GetFrom(messageType);

                if (message is WeArtJsonMessage jsonMessage)
                {
                    data = SerializeJsonMessage(reflectionData, jsonMessage);
                    return true;
                }

                var values = reflectionData.Fields
                    .Select(field => field.GetValue(message));

                var serializedValues = values
                    .Select(Serialize)
                    .Where(s => s != null)
                    .Prepend(reflectionData.Id);

                data = string.Join(separator.ToString(), serializedValues);
                return true;
            }
            catch (Exception)
            {
                data = string.Empty;
                return false;
            }
        }

        private string SerializeJsonMessage(ReflectionData reflectionData, WeArtJsonMessage jsonMessage)
        {
            // Configure JSON serializer options
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false  // Equivalent to Formatting.None in Newtonsoft
            };

            // Create JsonMessageTemplate with the appropriate data
            JsonMessageTemplate json = new JsonMessageTemplate
            {
                Type = reflectionData.Id,
                Timestamp = jsonMessage.Timestamp,
                Data = jsonMessage  // Assign the correct data object
            };

            // Serialize JsonMessageTemplate to JSON string
            return JsonSerializer.Serialize(json, jsonOptions);
        }

        public override bool Deserialize(string data, out IWeArtMessage message)
        {
            try
            {
                if (IsJsonMessage(data))
                    message = DeserializeJsonMessage(data);
                else
                    message = DeserializeCsvMessage(data);
            }
            catch (Exception)
            {
                message = null;
            }
            return message != null;
        }

        private bool IsJsonMessage(string data)
        {
            // Heuristic to see fast (without regex or parsing) if the data we received could be a json message
            // Old protocol messages does not start with "[" or "{" so it should be ok
            if (string.IsNullOrWhiteSpace(data)) return false;
            data = data.Trim();

            bool isObject = data.StartsWith("{") && data.EndsWith("}");
            bool isArray = data.StartsWith("[") && data.EndsWith("]");
            return isObject || isArray;
        }

        private IWeArtMessage DeserializeCsvMessage(string data)
        {
            string[] split = data.Split(separator);
            var messageID = split[0];

            var reflectionData = ReflectionData.GetFrom(messageID);
            var message = (IWeArtMessage)Activator.CreateInstance(reflectionData.Type);

            if (message is IWeArtMessageCustomSerialization deserializableMessage)
            {
                deserializableMessage.Deserialize(split);
            }
            else
            {
                for (int i = 1; i < split.Length; i++)
                {
                    var field = reflectionData.Fields[i - 1];
                    var value = Deserialize(split[i], field.FieldType);
                    field.SetValue(message, value);
                }
            }
            return message;
        }

        private IWeArtMessage DeserializeJsonMessage(string data)
        {
            try
            {
                JsonMessageTemplate json = JsonSerializer.Deserialize<JsonMessageTemplate>(data);
                ReflectionData reflectionData = ReflectionData.GetFrom(json.Type);

                // Message without parameters
                if (json.Data is null)
                {
                    var message = (WeArtJsonMessage)Activator.CreateInstance(reflectionData.Type);
                    message.Timestamp = json.Timestamp;
                    return message;
                }

                // Message with parameters
                return (IWeArtMessage)JsonSerializer.Deserialize(json.Data.ToString(), reflectionData.Type, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true
                });

            }
            catch (MessageTypeNotFound)
            {
                return null;
            }
        }

        private static string Serialize(object data)
        {
            if (data == null)
                return null;

            if (data is Enum)
                return data.ToString().ToUpper();

            if (data.GetType() != typeof(string) && data is IEnumerable enumerable)
            {
                var serializedValues = enumerable
                    .Cast<object>()
                    .Select(obj => Serialize(obj));

                return string.Join(separator.ToString(), serializedValues);
            }

            if (data is IConvertible convertible)
                return convertible.ToString(CultureInfo.InvariantCulture);

            return data.ToString();
        }

        private static object Deserialize(string data, Type type)
        {
            if (type.IsEnum)
                return Enum.Parse(type, data);
            if (typeof(IConvertible).IsAssignableFrom(type))
                return ((IConvertible)data).ToType(type, CultureInfo.InvariantCulture);

            return data;
        }

        internal class MessageTypeNotFound : Exception { }


        private struct ReflectionData
        {
            private static readonly Dictionary<string, ReflectionData> _cache = new Dictionary<string, ReflectionData>();

            public string Id;
            public Type Type;
            public FieldInfo[] Fields;

            public ReflectionData(string id, Type type) : this()
            {
                Id = id;
                Type = type;
                Fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .OrderBy(field => field.MetadataToken)
                    .ToArray();
            }

            public static ReflectionData GetFrom(Type messageType)
            {
                var messageID = messageType.GetCustomAttribute<WeArtMiddlewareMessageIDAttribute>().ID;

                if (!_cache.TryGetValue(messageID, out var reflectionData))
                {
                    reflectionData = new ReflectionData(messageID, messageType);
                    _cache[messageID] = reflectionData;
                }
                return reflectionData;
            }

            public static ReflectionData GetFrom(string messageID)
            {
                if (!_cache.TryGetValue(messageID, out var reflectionData))
                {
                    var messageType = typeof(WeArtMessageCustomSerializer).Assembly.GetTypes()
                        .Select(type => (type, attribute: type.GetCustomAttribute<WeArtMiddlewareMessageIDAttribute>()))
                        .Where(pair => pair.attribute != null && pair.attribute.ID == messageID)
                        .FirstOrDefault().type ?? throw new MessageTypeNotFound();

                    reflectionData = new ReflectionData(messageID, messageType);
                    _cache[messageID] = reflectionData;
                }
                return reflectionData;
            }
        }
    }

    public class MillisecondsEpochConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("Expected number token for Unix timestamp in milliseconds.");

            long milliseconds = reader.GetInt64();
            return _epoch.AddMilliseconds(milliseconds);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            long milliseconds = (long)(value - _epoch).TotalMilliseconds;
            writer.WriteNumberValue(milliseconds);
        }
    }
}