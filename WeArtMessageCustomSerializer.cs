/**
*	WEART - Serializer helper
*	https://www.weart.it/
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using WeArt.Core;
using WeArt.Utils;

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

        public override bool Serialize(IWeArtMessage message, out string data)
        {
            try
            {
                if(message is IWeArtMessageCustomSerialization serializableMessage)
                {
                    data = string.Join(separator.ToString(), serializableMessage.Serialize());
                    return true;
                }
                
                var messageType = message.GetType();
                var reflectionData = ReflectionData.GetFrom(messageType);

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

        public override bool Deserialize(string data, out IWeArtMessage message)
        {
            try
            {
                string[] split = data.Split(separator);
                var messageID = split[0];

                var reflectionData = ReflectionData.GetFrom(messageID);

                message = (IWeArtMessage)Activator.CreateInstance(reflectionData.Type);

                if (message is IWeArtMessageCustomSerialization deserializableMessage)
                    return deserializableMessage.Deserialize(split);
                
                for (int i = 1; i < split.Length; i++)
                {
                    var field = reflectionData.Fields[i - 1];
                    var value = Deserialize(split[i], field.FieldType);
                    field.SetValue(message, value);
                }
                return true;
            }
            catch (Exception)
            {
                message = null;
                return false;
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
                        .FirstOrDefault().type;

                    reflectionData = new ReflectionData(messageID, messageType);
                    _cache[messageID] = reflectionData;
                }
                return reflectionData;
            }
        }
    }
}