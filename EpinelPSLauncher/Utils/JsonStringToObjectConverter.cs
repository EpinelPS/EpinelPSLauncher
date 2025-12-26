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
            var converter = opts.TypeInfoResolverChain[0].GetTypeInfo(typeToConvert, opts);
            if (converter == null) throw new InvalidOperationException();

            return (T?)JsonSerializer.Deserialize(json, converter);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions opts)
        {
            var converter = opts.TypeInfoResolverChain[0].GetTypeInfo(typeof(T), opts);
            if (converter == null) throw new InvalidOperationException();

            string json = JsonSerializer.Serialize(value, converter);
            writer.WriteStringValue(json);
        }
    }
}
