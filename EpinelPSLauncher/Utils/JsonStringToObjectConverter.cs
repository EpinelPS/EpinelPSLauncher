using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Utils
{
    public class JsonStringToObjectConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions opts)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string‑encoded JSON.");
            string json = reader.GetString()!;
            return JsonSerializer.Deserialize<T>(json, opts);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions opts)
        {
            string json = JsonSerializer.Serialize(value, opts);
            writer.WriteStringValue(json);
        }
    }
}
