using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            try
            {
                // Load the JSON data as a JArray
                var jsonArray = JArray.Load(reader);

                // Create a new byte array to store the result
                var byteArray = new byte[jsonArray.Count];

                // Iterate through the elements of the JArray and convert values to bytes
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    var byteValue = jsonArray[i].Value<byte>();
                    byteArray[i] = byteValue;
                }

                return byteArray;
            }
            catch
            {
                throw;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Check if the value is a byte array
            if (value is byte[] byteArray)
            {
                // Start writing the JSON array
                writer.WriteStartArray();

                // Write each byte value as a separate JSON integer
                foreach (var b in byteArray)
                {
                    writer.WriteValue(b);
                }

                // End the JSON array
                writer.WriteEndArray();
            }
            else
            {
                throw new NotSupportedException("Expected a byte array.");
            }
        }
    }
}
