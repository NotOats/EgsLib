using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace EgsLib.ConfigFiles.Ecf
{
    internal static class EcfExtensions
    {
        #region IEcfObject
        public static bool ReadField<T>(this IEcfObject obj, string name, out T value) where T : IConvertible
        {
            if (ReadField(obj, name, out object raw, typeof(T)))
            {
                value = (T)raw;
                return true;
            }

            value = default;
            return false;
        }

        public static bool ReadField(this IEcfObject obj, string name, out object value, Type type)
        {
            value = default;

            if (!obj.Fields.TryGetValue(name, out string str) || str == null)
                return false;

            if (!ConvertValue(str, out object output, type))
                return false;

            value = output;
            return true;
        }

        public static bool ReadProperty<T>(this IEcfObject obj, string name, out T value) where T : IConvertible
        {
            if(ReadProperty(obj, name, out object raw, typeof(T)))
            {
                value = (T)raw;
                return true;
            }

            value = default;
            return false;
        }

        public static bool ReadProperty(this IEcfObject obj, string name, out object value, Type type)
        {
            value = default;

            if (!obj.Properties.TryGetValue(name, out string str) || str == null)
                return false;

            if (!ConvertValue(str, out object output, type))
                return false;

            value = output;
            return true;
        }
        #endregion


        #region IEcfChild
        public static bool ReadProperty<T>(this IEcfChild obj, string name, out T value) where T : IConvertible
        {
            if (ReadProperty(obj, name, out object raw, typeof(T)))
            {
                value = (T)raw;
                return true;
            }

            value = default;
            return false;
        }

        public static bool ReadProperty(this IEcfChild obj, string name, out object value, Type type)
        {
            value = default;

            if (!obj.Properties.TryGetValue(name, out string str) || str == null)
                return false;

            if (!ConvertValue(str, out object output, type))
                return false;

            value = output;
            return true;
        }
        #endregion

        private static Type FindType(Type input)
        {
            var u = Nullable.GetUnderlyingType(input);
            return u ?? input;
        }

        private static bool ConvertValue(string input, out object output, Type type)
        {
            // Sometimes properties have encapsulating quotes
            input = input.Trim(' ', '"');
            output = default;

            var converter = TypeDescriptor.GetConverter(type);
            if (!converter.CanConvertFrom(typeof(string)))
                return false;

            output = converter.ConvertFromString(input);
            return true;
        }
    }
}
