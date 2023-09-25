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
            var match = TypeMatch.Match(str);
            if (!match.Success || match.Groups.Count != 2)
                throw new FormatException("ECF property line does not have a type");

            var typeString = match.Groups[1].Value;
            switch (typeString)
            {
                case "int":
                    return new PropertyDecorator<int>(str);
                case "float":
                    return new PropertyDecorator<float>(str);
            }

#if DEBUG
            Console.WriteLine($"ECF property line has invalid type '{typeString}'");
#endif
            throw new FormatException($"ECF property line has invalid type '{typeString}'");
        }
    }
}
