using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles.Ecf
{
    internal class PropertyDecoratorTypeConverter : TypeConverter
    {
        private readonly static Regex TypeMatch = new Regex(@"type: ([^,|^\n]+)", RegexOptions.Compiled);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            return CreateDecorator(str);
        }

        private static object CreateDecorator(string str)
        {
            // TODO: Figure out what the default really is for this
            var type = "int";

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
            }

#if DEBUG
            Console.WriteLine($"ECF property line has invalid type '{type}'");
#endif
            throw new FormatException($"ECF property line has invalid type '{type}'");
        }
    }
}
