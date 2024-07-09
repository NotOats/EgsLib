using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EgsLib.Extensions
{
    internal static class StringExtensions
    {
        private readonly static ConcurrentDictionary<Type, TypeConverter> ConverterCache = new ConcurrentDictionary<Type, TypeConverter>();

        public static bool ConvertType<T>(this string str, out T output)
        {
            if (ConvertType(str, typeof(T), out object outputObj))
            {
                output = (T)outputObj;
                return true;
            }

            output = default;
            return false;
        }

        public static bool ConvertType(this string str, Type outputType, out object output)
        {
            if (outputType == typeof(string))
            {
                output = str;
                return true;
            }

            if (!ConverterCache.TryGetValue(outputType, out TypeConverter converter))
            {
                converter = TypeDescriptor.GetConverter(outputType);
                ConverterCache.TryAdd(outputType, converter);
            }

            if (converter.CanConvertFrom(typeof(string)))
            {
                output = converter.ConvertFrom(str);
                return true;
            }

            output = default;
            return false;
        }

        public static IEnumerable<string> SplitWithQuotes(this string str, params char[] separators)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            if (separators.Contains('"'))
                throw new ArgumentException("double quotes can't be a separator", nameof(separators));

            var part = "";
            var inQuotes = false;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (separators.Contains(c) && !inQuotes)
                {
                    yield return part;
                    part = "";
                }
                else
                {
                    part += c;
                }
            }

            yield return part;
        }

        public static string AsNullIfEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str) ? str : null;
        }
    }
}
