using EgsLib;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScenarioDumper.Converters
{
    internal class Vector3Converter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            if (typeToConvert.GetGenericTypeDefinition() != typeof(Vector3<>))
                return false;

            return true;
        }


        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var argType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(Vector3ConverterInner<>).MakeGenericType(new[] { argType });

            var converter = (JsonConverter)Activator.CreateInstance(
                converterType,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null)!;

            return converter;
        }

        private class Vector3ConverterInner<T> : JsonConverter<Vector3<T>>
        {
            private readonly JsonConverter<T> _valueConverter;
            private readonly Type _valueType;

            public Vector3ConverterInner(JsonSerializerOptions options)
            {
                _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
                _valueType = typeof(T);
            }

            public override Vector3<T> ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }

            public override void WriteAsPropertyName(Utf8JsonWriter writer, Vector3<T> value, JsonSerializerOptions options)
            {
                writer.WritePropertyName($"[X: {value.X}, Y: {value.Y}, Z: {value.Z}]");
            }

            public override Vector3<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                T? x = default;
                T? y = default;
                T? z = default;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (x == null || y == null || z == null)
                            throw new JsonException();

                        return new Vector3<T>(x, y, z);
                    }

                    // Get the key
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException();

                    var name = reader.GetString() ?? throw new JsonException();

                    // Get the value
                    reader.Read();
                    var value = _valueConverter.Read(ref reader, _valueType, options);

                    switch(name.ToLower())
                    {
                        case "x":
                            x = value; break;
                        case "y": 
                            y = value; break;
                        case "z":
                            z = value; break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Vector3<T> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                WriteObjectProperty(writer, options, "X", value.X);
                WriteObjectProperty(writer, options, "Y", value.Y);
                WriteObjectProperty(writer, options, "Z", value.Z);

                writer.WriteEndObject();
            }

            private void WriteObjectProperty(Utf8JsonWriter writer, JsonSerializerOptions options, string propertyName, T value)
            {
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

                _valueConverter.Write(writer, value, options);
            }
        }
    }

}
