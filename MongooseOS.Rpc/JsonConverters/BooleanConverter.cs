using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MongooseOS.Rpc.JsonConverters
{
    class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch(reader.TokenType)
            {
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return reader.GetBoolean();
                case JsonTokenType.String:
                    string value = reader.GetString();
                    string chkValue = value.ToLower();
                    if (chkValue.Equals("true"))
                    {
                        return true;
                    }
                    if (value.ToLower().Equals("false"))
                    {
                        return false;
                    }
                    break;

            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }

    }
}
