using UnityEngine;

namespace Labyrinth
{
    using Bolt;

    public static class Extensions
    {
        /// inserts a into the frist 8 bits of a short and b into the last 8
        public static short Combine(this byte a, byte b)
        {
            return (short)((a << 8) | (b));
        }

        /// inserts a into the frist 16 bits of an int and b into the last 16
        public static int Combine(this ushort a, ushort b)
        {
            return ((a << 16) | (b));
        }

        public static Vector2 ReadVector2(this Reader reader)
        {
            Vector2 value;
            value.x = reader.ReadFloat();
            value.y = reader.ReadFloat();
            return value;
        }

        public static void Write(this Writer writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public static Vector2Int ReadVector2Int(this Reader reader)
        {
            Vector2Int value = Vector2Int.zero;
            value.x = reader.ReadInt();
            value.y = reader.ReadInt();
            return value;
        }

        public static void Write(this Writer writer, Vector2Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public static Vector3 ReadVector3(this Reader reader)
        {
            Vector3 value;
            value.x = reader.ReadFloat();
            value.y = reader.ReadFloat();
            value.z = reader.ReadFloat();
            return value;
        }

        public static void Write(this Writer writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static Vector3Int ReadVector3Int(this Reader reader)
        {
            Vector3Int value = Vector3Int.zero;
            value.x = reader.ReadInt();
            value.y = reader.ReadInt();
            value.z = reader.ReadInt();
            return value;
        }

        public static void Write(this Writer writer, Vector3Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static Vector4 ReadVector4(this Reader reader)
        {
            Vector4 value;
            value.x = reader.ReadFloat();
            value.y = reader.ReadFloat();
            value.z = reader.ReadFloat();
            value.w = reader.ReadFloat();
            return value;
        }

        public static void Write(this Writer writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static Quaternion ReadQuaternion(this Reader reader)
        {
            Quaternion value;
            value.x = reader.ReadFloat();
            value.y = reader.ReadFloat();
            value.z = reader.ReadFloat();
            value.w = reader.ReadFloat();
            return value;
        }

        public static void Write(this Writer writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        static Extensions()
        {
            Extension.Generic<Vector2>.SetRead((ref Reader reader) =>
            {
                return reader.ReadVector2();
            });
            Extension.Generic<Vector3>.SetRead((ref Reader reader) =>
            {
                return reader.ReadVector3();
            });
            Extension.Generic<Vector4>.SetRead((ref Reader reader) =>
            {
                return reader.ReadVector4();
            });
            Extension.Generic<Quaternion>.SetRead((ref Reader reader) =>
            {
                return reader.ReadQuaternion();
            });
            Extension.Generic<Vector2Int>.SetRead((ref Reader reader) =>
            {
                return reader.ReadVector2Int();
            });
            Extension.Generic<Vector3Int>.SetRead((ref Reader reader) =>
            {
                return reader.ReadVector3Int();
            });

            Extension.Generic<Vector2>.SetWrite((ref Writer writer, Vector2 value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Vector3>.SetWrite((ref Writer writer, Vector3 value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Vector4>.SetWrite((ref Writer writer, Vector4 value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Quaternion>.SetWrite((ref Writer writer, Quaternion value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Vector2Int>.SetWrite((ref Writer writer, Vector2Int value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Vector3Int>.SetWrite((ref Writer writer, Vector3Int value) =>
            {
                writer.Write(value);
            });
        }
    }
}
