using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public class ByteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Deserialize the JSON object as a dictionary of key-value pairs
            var jsonObject = serializer.Deserialize<JObject>(reader);

            // Create a new byte array to store the result
            var byteArray = new byte[jsonObject.Count];

            // Iterate through the properties of the JObject and convert values to bytes
            foreach (var property in jsonObject.Properties())
            {
                var index = int.Parse(property.Name);
                var byteValue = property.Value.Value<byte>();
                byteArray[index] = byteValue;
            }

            return byteArray;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Not implemented for this example as we only deserialize
            // Possible implementation:
            // var byteArray = (byte[]) value;
            // writer.WriteValue(BitConverter.ToString(byteArray).Replace("-", ""));
            throw new NotImplementedException();
        }
    }
}
