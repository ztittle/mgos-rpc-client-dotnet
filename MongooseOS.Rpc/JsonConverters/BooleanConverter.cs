using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MongooseOS.Rpc.JsonConverters
{
    class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            string chkValue = value.ToLower();
            if (chkValue.Equals("true") || chkValue.Equals("yes") || chkValue.Equals("1"))
            {
                return true;
            }
            if (value.ToLower().Equals("false") || chkValue.Equals("no") || chkValue.Equals("0"))
            {
                return false;
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }

    }
}
