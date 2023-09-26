using System;
using System.Collections.Generic;
using System.Reflection;

namespace EgsLib.ConfigFiles.Ecf.Attributes
{
    internal class ConverterAttribute : Attribute
    {
        public delegate bool ConverterDelegate(string input, out object output, Type type);

        public ConverterDelegate Converter { get; }

        public ConverterAttribute(Type objectType = null, string functionName = null)
        {
            if (objectType == null != (objectType == null))
                throw new ArgumentException("converterType and converterFunction - both must be null or some other value");

            if (objectType == null && string.IsNullOrWhiteSpace(functionName))
                return;

            var method = objectType.GetMethod(functionName, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new ArgumentException($"function is not a (static) method on converterType", nameof(functionName));

            var del = method.CreateDelegate(typeof(ConverterDelegate))
                ?? throw new ArgumentException("converter must be a ConverterDelegate", nameof(functionName));

            Converter = (ConverterDelegate)del;
        }

        protected static IReadOnlyDictionary<PropertyInfo, TAttribute> ReadProperties<TObject, TAttribute>() where TAttribute : ConverterAttribute
        {
            var result = new Dictionary<PropertyInfo, TAttribute>();

            foreach (var property in typeof(TObject).GetProperties())
            {
                var attr = property.GetCustomAttribute<TAttribute>();
                if (attr == null)
                    continue;

                if (!property.CanWrite)
                    throw new Exception($"{typeof(TAttribute).Name} does not have a setter");

                result.Add(property, attr);
            }

            return result;
        }
    }
}
