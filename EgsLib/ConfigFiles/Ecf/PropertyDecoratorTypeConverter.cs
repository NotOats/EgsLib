using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles.Ecf
{
    internal class PropertyDecoratorTypeConverter : TypeConverter
    {
        private readonly static Regex TypeMatch = new Regex(@"type: ([^,|^\n]+)", RegexOptions.Compiled);

        private readonly static IReadOnlyDictionary<Type, string> TypeMap = new Dictionary<Type, string>
        {
            {typeof(int), "int"},
            {typeof(float), "float"},
            {typeof(bool), "bool"},
            {typeof(string), "string"}
        };

        private readonly string _innerTypeName;

        public PropertyDecoratorTypeConverter(Type type)
        {
            if(type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(PropertyDecorator<>)
                && type.GetGenericArguments().Length == 1)
            {
                var innerType = type.GetGenericArguments()[0];
                if (!TypeMap.TryGetValue(innerType, out string value))
                    throw new NotSupportedException($"{type.Name} is not supported in PropertyDecorator<T>");

                _innerTypeName = value;
            }
            else
            {
                throw new ArgumentException("Incompatible type", nameof(type));
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            return CreateDecorator(str);
        }

        private object CreateDecorator(string str)
        {
            // Default to generic type in PropertyDecorator
            var type = _innerTypeName;

            // Attempt to override with type in str
            var match = TypeMatch.Match(str);
            if (match.Success && match.Groups.Count == 2)
                type = match.Groups[1].Value;

            switch (type)
            {
                case "int":
                    return new PropertyDecorator<int>(str);

                case "float":
                    return new PropertyDecorator<float>(str);

                case "bool":
                    return new PropertyDecorator<bool>(str);

                case "string":
                    return new PropertyDecorator<string>(str);
            }

#if DEBUG
            Console.WriteLine($"ECF property line has invalid type '{type}'");
#endif
            throw new FormatException($"ECF property line has invalid type '{type}'");
        }
    }
}
