using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMessages
{
    internal static class JsonConverter
    {
        private static readonly JsonSerializerSettings _JsonSettings = new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss", Formatting = Formatting.Indented };

        static JsonConverter()
        {
            _JsonSettings.Converters.Add(new MessageTypeConverter());
            _JsonSettings.Converters.Add(new TimeCreationConverter());
            _JsonSettings.Converters.Add(new BaseMessageCreationConverter());
        }

        public static T ConvertTo<T>(String json)
        {
            return JsonConvert.DeserializeObject<T>(json, _JsonSettings);
        }

        public static String ConvertToJson<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _JsonSettings);
        }
    }

    internal class MessageTypeConverter : JsonConverter<BaseMessage.MessageTypes>
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;

        public override BaseMessage.MessageTypes ReadJson(JsonReader reader, Type objectType, BaseMessage.MessageTypes existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return BaseMessage.MessageTypes.TimedMessage;
        }

        public override void WriteJson(JsonWriter writer, BaseMessage.MessageTypes value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    internal class TimeCreationConverter : JsonConverter<Time>
    {
        public override Time ReadJson(JsonReader reader, Type objectType, Time existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            String val = (String)reader.Value;
            if (Time.TryParse(val, out Time time))
                return time;

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, Time value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    internal class BaseMessageCreationConverter : JsonCreationConverter<BaseMessage>
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected override BaseMessage Create(Type objectType, JObject jObject)
        {
            if (!jObject.ContainsKey("MessageType"))
                return null;

            BaseMessage.MessageTypes msgType = (BaseMessage.MessageTypes)Enum.Parse(typeof(BaseMessage.MessageTypes), jObject.Value<String>("MessageType"));

            switch(msgType)
            {
                case BaseMessage.MessageTypes.TimedMessage:
                    return new TimedMessage();
                case BaseMessage.MessageTypes.FixedTimedMessage:
                    return new FixedTimedMessage();
                default:
                    return null;
            }
        }
    }

    public abstract class JsonCreationConverter<T> : Newtonsoft.Json.JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            using (JsonReader targetReader = jObject.CreateReader())
            {
                CopySettings(reader, targetReader);
                serializer.Populate(targetReader, target);
            }

            return target;
        }

        private static void CopySettings(JsonReader source, JsonReader target)
        {
            target.Culture = source.Culture;
            target.DateFormatString = source.DateFormatString;
            target.DateParseHandling = source.DateParseHandling;
            target.DateTimeZoneHandling = source.DateTimeZoneHandling;
            target.FloatParseHandling = source.FloatParseHandling;
            target.MaxDepth = source.MaxDepth;
            target.SupportMultipleContent = source.SupportMultipleContent;
        }
    }
}
