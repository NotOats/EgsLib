using System;
using System.Collections.Generic;

namespace EgsLib.ConfigFiles.Ecf
{
    public interface IEcfObject
    {
        /// <summary>
        /// The object's type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Fields that describe the object, found on the same line as object Type
        /// </summary>
        IReadOnlyDictionary<string, string> Fields { get; }

        /// <summary>
        /// Properties of the object
        /// </summary>
        IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary>
        /// Child objects
        /// </summary>
        IReadOnlyCollection<IEcfChild> Children { get; }
    }

    public static class EcfObjectExtensions
    {
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

            // Sometimes properties have encapsulating quotes
            str = str.Trim(' ', '"');

            if (!str.ConvertType(type, out object output))
                return false;

            value = output;
            return true;
        }

        public static bool ReadProperty<T>(this IEcfObject obj, string name, out T value) where T : IConvertible
        {
            if (ReadProperty(obj, name, out object raw, typeof(T)))
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

            // Sometimes properties have encapsulating quotes
            str = str.Trim(' ', '"');

            if (!str.ConvertType(type, out object output))
                return false;

            value = output;
            return true;
        }
    }
}