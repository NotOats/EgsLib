using System;
using System.Collections.Generic;

namespace EgsLib.Blueprints
{
    public readonly struct PropertyDetails : IEquatable<PropertyDetails>
    {
        public PropertyName Name { get; }
        public PropertyType Type { get; }
        public object Value { get; }

        public PropertyDetails(PropertyName name, PropertyType type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name}<{Type}>: {Value ?? "N/A"}";
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyDetails details && Equals(details);
        }

        public bool Equals(PropertyDetails other)
        {
            return Name == other.Name &&
                   Type == other.Type &&
                   EqualityComparer<object>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            int hashCode = 1168257605;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static bool operator ==(PropertyDetails left, PropertyDetails right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PropertyDetails left, PropertyDetails right)
        {
            return !(left == right);
        }
    }

    public enum PropertyName
    {
        AddDigoutBox = 1,
        FlattenTerrain,
        GroundYOffset,
        RotationToFaceNorth,
        Powered,
        Rotation,
        GroupName,
        ChangedBuild,
        ChangedDate,
        CreatorPlayerName,
        CreatorPlayerId,
        ChangedPlayerName,
        ChangedPlayerId,
        CameraYOffset,
        CameraZOffset,
        DisplayName,
        GroundYOffsetFloat,
        PivotPoint,
        KeepTopSoil,
        RotationSensitivity,
        Tags,
        RLInContainer,
        RLOutContainer,
        BlueprintPartsCount
    }

    public enum PropertyType : byte
    {
        String,
        Bool,
        Int,
        Single,
        Vector3,
        Long
    }
}
