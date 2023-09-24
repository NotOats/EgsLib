using System;
using System.Collections.Generic;
using System.Linq;

namespace EgsLib.ConfigFiles.Ecf
{
    public readonly struct PropertyDectoractor<T> : IEquatable<PropertyDectoractor<T>>
    {
        // Default (empty) value
        public readonly static PropertyDectoractor<T> Default = new PropertyDectoractor<T>(default, null, null, null);

        // Value
        public T Value { get; }

        // Properties
        public bool? Display { get; }
        public string Type { get; }
        public string Formatter { get; }

        internal PropertyDectoractor(T value, bool? display, string type, string formatter)
        {
            Value = value;
            Display = display;
            Type = type;
            Formatter = formatter;
        }

        internal PropertyDectoractor(string ecfValue, Func<string, T> converter)
        {
            Value = default;
            Display = null;
            Type = null;
            Formatter = null;

            if (string.IsNullOrEmpty(ecfValue))
                return;

            var parts = ecfValue.SplitWithQuotes(',').Select(s => s.Trim()).ToArray();
            Value = converter(parts[0]);

            foreach (var part in parts.Skip(1))
            {
                var entry = part.SplitWithQuotes(':').Select(s => s.Trim()).ToArray();
                if (entry.Length != 2)
                    throw new FormatException("Value's property isn't correctly formatted");

                switch (entry[0])
                {
                    case "type": Type = entry[1]; break;
                    case "display": Display = entry[1] == "true"; break;
                    case "formatter": Formatter = entry[1]; break;
                }
            }
        }

        public override string ToString()
        {
            var parts = new List<string> { Value.ToString() };

            if (!string.IsNullOrWhiteSpace(Type))
                parts.Add($"type: {Type}");

            if (Display.HasValue)
                parts.Add($"display: {Display.Value}");

            if (!string.IsNullOrWhiteSpace(Formatter))
                parts.Add($"formatter: {Formatter}");

            return string.Join(", ", parts);
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyDectoractor<T> value && Equals(value);
        }

        public bool Equals(PropertyDectoractor<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value) &&
                   Display == other.Display &&
                   Type == other.Type &&
                   Formatter == other.Formatter;
        }

        public override int GetHashCode()
        {
            int hashCode = -498236327;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + Display.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Formatter);
            return hashCode;
        }

        public static bool operator ==(PropertyDectoractor<T> left, PropertyDectoractor<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PropertyDectoractor<T> left, PropertyDectoractor<T> right)
        {
            return !(left == right);
        }
    }
}
