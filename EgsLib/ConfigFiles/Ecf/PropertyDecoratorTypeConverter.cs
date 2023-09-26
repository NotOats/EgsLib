using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles.Ecf
{
    internal class PropertyDecoratorTypeConverter : TypeConverter
    {
        private readonly static Regex TypeMatch = new Regex(@"type: ([^,|^\n]+)", RegexOptions.Compiled);

        private readonly string _innerType;

        public PropertyDecoratorTypeConverter(Type type)
        {
            if(type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(PropertyDecorator<>)
                && type.GetGenericArguments().Length == 1)
            {
                _innerType = type.GetGenericArguments()[0].Name;
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
            var type = _innerType;

            // Attempt to override with type in str
            var match = TypeMatch.Match(str);
            if (match.Success && match.Groups.Count == 2)
                type = match.Groups[1].Value;


            switch (type)
            {
                case "int":     // Pulled from ecf string
                case "Int32":   // Pulled from ctor
                    return new PropertyDecorator<int>(str);

                case "float":   // Pulled from ecf string
                case "Single":  // Pulled from ctor
                    return new PropertyDecorator<float>(str);
            }

#if DEBUG
            Console.WriteLine($"ECF property line has invalid type '{type}'");
#endif
            throw new FormatException($"ECF property line has invalid type '{type}'");
        }
    }
}
