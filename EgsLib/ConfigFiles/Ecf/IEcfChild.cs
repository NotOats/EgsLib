using System;
using System.Collections.Generic;
using System.Threading;

namespace EgsLib.ConfigFiles.Ecf
{
    public interface IEcfChild
    {
        string Name { get; }
        IReadOnlyDictionary<string, string> Properties { get; }
    }

    public static class EcfChildExtensions
    {
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

            // Sometimes properties have encapsulating quotes
            str = str.Trim(' ', '"');

            if (!str.ConvertType(type, out object output))
                return false;

            value = output;
            return true;
        }
    }
}