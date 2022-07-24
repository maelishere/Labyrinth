﻿using UnityEngine;

namespace Labyrinth
{
    using Bolt;
    using Labyrinth.Runtime;

    public static class Extensions
    {
        /// inserts a into the frist 8 bits of a ushort and b into the last 8
        internal static ushort Combine(this byte a, byte b)
        {
            ushort value = a;
            value <<= 8;
            value |= b;
            return value;
        }

        /// inserts a into the frist 16 bits of an uint and b into the last 16
        internal static uint Combine(this ushort a, ushort b)
        {
            uint value = a;
            value <<= 16;
            value |= b;
            return value;
        }

        // FNV Hash (gcc optimization: 32 bit FNV-1a)
        // url: http://www.isthe.com/chongo/tech/comp/fnv/index.html
        internal static uint Hash(this string value)
        {
            uint hash = 0x01000193; //16777619
            foreach (char c in value)
            {
                /* xor the bottom with the current octet */
                hash ^= c;

                /* multiply by the 64 bit FNV magic prime mod 2^32 */
                /*hash *= 0x01000193; same as:*/
                hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
            }
            return hash;
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

        public static Instance ReadInstance(this Reader reader)
        {
            Instance.Find(reader.ReadInt(), out Instance instance);
            return instance;
        }

        public static void WriteInstance(this Writer writer, Instance instance)
        {
            writer.Write(instance?.identity ?? Identity.Any);
        }

        // when defining your own Custom Writer/Reader 
        //      if you use a Static Constructor and you get an exception
        //      use [RuntimeInitializeOnLoadMethod] or Awake
        //      static contrustors don't get called something (not sure why)
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
            Extension.Generic<Instance>.SetRead((ref Reader reader) =>
            {
                return reader.ReadInstance();
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
            Extension.Generic<Vector3Int>.SetWrite((ref Writer writer, Vector3Int value) =>
            {
                writer.Write(value);
            });
            Extension.Generic<Instance>.SetWrite((ref Writer writer, Instance instance) =>
            {
                writer.WriteInstance(instance);
            });
        }
    }
}
