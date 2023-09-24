using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EgsLib.ConfigFiles.Ecf
{
    internal class PropertyDectoractorTypeConverter : TypeConverter
    {
        private readonly static Regex TypeMatch = new Regex(@"type: ([^,|^\n]+)", RegexOptions.Compiled);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            var genericType = ReadType(str);
            var propDecType = typeof(PropertyDectoractor<>).MakeGenericType(genericType);

            var obj = Activator.CreateInstance(
                propDecType, 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, 
                null, 
                new object[] { str }, 
                null);

            return obj;
        }

        private static Type ReadType(string str)
        {
            var match = TypeMatch.Match(str);
            if (!match.Success || match.Groups.Count != 2)
                throw new FormatException("ECF property line does not have a type");

            var typeString = match.Groups[1].Value;
            switch (typeString)
            {
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
            }

            throw new FormatException($"ECF property line has invalid type '{typeString}'");
        }
    }
}
