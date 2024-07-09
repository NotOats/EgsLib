using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using EgsLib.Extensions;

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
        #region Common Fields/Properties
        /// <summary>
        /// Attemps to read the Id field of the object, retuns -1 if it's not found
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetId(this IEcfObject obj)
        {
            if (ReadField(obj, "Id", out int value))
                return value;

            return -1;
        }

        /// <summary>
        /// Attempts to read the Name field of the object, returns null if it's not found
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetName(this IEcfObject obj)
        {
            if (ReadField(obj, "Name", out string value))
                return value;

            return null;
        }
        #endregion

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

            // Only remove quotes if the underlying type isn't a PropertyDecorator which requires quotes in place
            if (CheckUnderlyingType(type, typeof(PropertyDecorator<>)))
                str = str.Trim();
            else
                str = str.Trim(' ', '"');

            if (!str.ConvertType(type, out object output))
                return false;

            value = output;
            return true;
        }

        private static bool CheckUnderlyingType(Type objType, Type underlyingType)
        {
            // Compare names due to possible generics
            if (objType.Name == underlyingType.Name)
                return true;

            if(objType.IsGenericType && objType.GenericTypeArguments.Any(t => CheckUnderlyingType(t, underlyingType)))
                return true;

            return false;
        }
    }
}