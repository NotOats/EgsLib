using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace EgsLib.Extensions
{
    internal static class StringExtensions
    {
        private readonly static ConcurrentDictionary<Type, TypeConverter> ConverterCache = new ConcurrentDictionary<Type, TypeConverter>();

        public static T ConvertType<T>(this string str)
        {
            if (ConvertType<T>(str, out var output))
            {
                return output;
            }

            throw new FormatException("Failed to convert type");
        }

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
                output = converter.ConvertFromInvariantString(str);
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

            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (separators.Contains(c) && !inQuotes)
                {
                    yield return sb.ToString();

                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            yield return sb.ToString();
        }

        public static string AsNullIfEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str) ? str : null;
        }

        public static string SliceAndTrim(this string str, int start, int end)
        {
            for(; start < str.Length -  1; start++)
            {
                if (!char.IsWhiteSpace(str[start]))
                    break;
            }

            for (; end - 1 >= start; end--)
            {
                if (!char.IsWhiteSpace(str[end - 1]))
                    break;
            }

#if NETSTANDARD2_1_OR_GREATER
            var span = str.AsSpan();
            return span[start..end].ToString();
#else
            return str.Substring(start, end - start);
#endif
        }
    }
}
