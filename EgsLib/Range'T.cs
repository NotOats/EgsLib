using System;
using System.Collections.Generic;

namespace EgsLib
{
    // Mostly from https://stackoverflow.com/a/5343033

    /// <summary>
    /// Represents a range between two values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Range<T> : IEquatable<Range<T>> where T : IComparable<T>
    {
        public static readonly Range<T> Default = new Range<T>(default, default);


        /// <summary>Minimum value of the range.</summary>

        public T Minimum { get; }

        /// <summary>Maximum value of the range.</summary>
        public T Maximum { get; }

        /// <summary>
        /// Creates a new Range with the specified <see cref="Minimum"/> and <see cref="Maximum"/> values
        /// </summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="maximum">Maximum value</param>
        /// <param name="validate">Run <see cref="IsValid"/> on initialization</param>
        public Range(T minimum, T maximum, bool validate = true)
        {
            Minimum = minimum;
            Maximum = maximum;

            if (validate)
                IsValid();
        }

        /// <summary>Determines if the range is valid.</summary>
        /// <returns>True if range is valid, else false</returns>
        public bool IsValid()
        {
            return Minimum.CompareTo(Maximum) <= 0;
        }

        /// <summary>Determines if the provided value is inside the range.</summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public bool ContainsValue(T value)
        {
            return (Minimum.CompareTo(value) <= 0) && (Maximum.CompareTo(value) <= 0);
        }

        /// <summary>Determines if this Range is inside the bounds of another range.</summary>
        /// <param name="range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public bool IsInsideRange(Range<T> range)
        {
            return IsValid() && range.IsValid() && range.ContainsValue(Minimum) && range.ContainsValue(Maximum);
        }

        /// <summary>Determines if another range is inside the bounds of this range.</summary>
        /// <param name="range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public bool ContainsRange(Range<T> range)
        {
            return IsValid() && range.IsValid() && ContainsValue(range.Minimum) && ContainsValue(range.Maximum);
        }

        public override string ToString()
        {
            return $"[{Minimum} - {Maximum}]";
        }

        public override bool Equals(object obj)
        {
            return obj is Range<T> range && Equals(range);
        }

        public bool Equals(Range<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Minimum, other.Minimum) &&
                   EqualityComparer<T>.Default.Equals(Maximum, other.Maximum);
        }

        public override int GetHashCode()
        {
            int hashCode = 913158992;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Minimum);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Maximum);
            return hashCode;
        }

        public static bool operator ==(Range<T> left, Range<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right)
        {
            return !(left == right);
        }
    }
}
