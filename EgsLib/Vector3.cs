using System;
using System.Collections.Generic;

namespace EgsLib
{
    public readonly struct Vector3<T> : IEquatable<Vector3<T>>
    {
        public static Vector3<T> Default = new Vector3<T>(default, default, default);

        public T X { get; }
        public T Y { get; }
        public T Z { get; }

        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"[X: {X}, Y: {Y}, Z: {Z}]";
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3<T> other && Equals(other);
        }

        public bool Equals(Vector3<T> other)
        {
            return EqualityComparer<T>.Default.Equals(X, other.X) &&
                   EqualityComparer<T>.Default.Equals(Y, other.Y) &&
                   EqualityComparer<T>.Default.Equals(Z, other.Z);
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(X);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Y);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Z);
            return hashCode;
        }

        public static bool operator ==(Vector3<T> left, Vector3<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3<T> left, Vector3<T> right)
        {
            return !(left == right);
        }
    }
}
