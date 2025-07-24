using EgsLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace EgsLib.Blueprints
{
    public readonly struct PropertyDetails : IEquatable<PropertyDetails>
    {
        public PropertyName Name { get; }
        public PropertyType Type { get; }
        public object Value { get; }
        /// <summary>
        /// Metadata associated with this <see cref="PropertyDetails"/>. 
        /// Unused but preserved for serialization operations.
        /// </summary>
        public string Metadata { get; }

        public PropertyDetails(PropertyName name, PropertyType type, object value, string metadata)
        {
            Name = name;
            Type = type;
            Value = value;
            Metadata = metadata;
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

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Int32)Name);
            writer.Write((Int32)Type << 24);
            switch (Type)
            {
                case PropertyType.String:
                    writer.Write((string)Value);
                    break;
                case PropertyType.Bool:
                    writer.Write((bool)Value);
                    writer.Write(Metadata);
                    break;
                case PropertyType.Int:
                    writer.Write((int)Value);
                    writer.Write(Metadata);
                    break;
                case PropertyType.Single:
                    writer.Write((float)Value);
                    writer.Write(Metadata);
                    break;
                case PropertyType.Vector3:
                    writer.WriteSingleVector3((Vector3<float>)Value);
                    writer.Write(Metadata);
                    break;
                case PropertyType.Long:
                    writer.Write((long)Value);
                    writer.Write(Metadata);
                    break;
            }
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
