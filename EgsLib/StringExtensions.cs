using System.Collections.Generic;
using System.Linq;

namespace EgsLib
{
    internal static class StringExtensions
    {
        public static IEnumerable<string> SplitWithQuotes(this string str, params char[] separators)
        {
            var part = "";
            var inQuotes = str.StartsWith("\"");

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
    }
}
