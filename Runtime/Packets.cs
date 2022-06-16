﻿using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;

    public static class Packets
    {
        public struct Sync
        {
            public Sync(int identity, short signature)
            {
                Identity = identity;
                Signature = signature;
            }

            public int Identity { get; }
            public short Signature { get; }
        }

        public static Sync ReadSync(this Reader reader)
        {
            return new Sync(reader.ReadInt(), reader.ReadShort());
        }

        public static void WriteSync(this Writer writer, int identity, short signature)
        {
            writer.Write(identity);
            writer.Write(signature);
        }

        public struct Call
        {
            public Call(int target, int identity, short procedure)
            {
                Target = target;
                Identity = identity;
                Procedure = procedure;
            }

            public int Target { get; }
            public int Identity { get; }
            public short Procedure { get; }
        }

        public static Call ReadCall(this Reader reader)
        {
            return new Call(reader.ReadInt(), reader.ReadInt(), reader.ReadShort());
        }

        public static void WriteCall(this Writer writer, int target, int identity, short procedure)
        {
            writer.Write(target);
            writer.Write(identity);
            writer.Write(procedure);
        }

        public struct Spawn
        {
            public Spawn(int asset, int identity, int authority, Vector3 position, Vector3 rotation)
            {
                Asset = asset;
                Identity = identity;
                Authority = authority;
                Position = position;
                Rotation = rotation;
            }

            public int Asset { get; }
            public int Identity { get; }
            public int Authority { get; }
            public Vector3 Position { get; }
            public Vector3 Rotation { get; }
        }

        public static Spawn ReadSpawn(this Reader reader)
        {
            return new Spawn(reader.ReadInt(), reader.ReadInt(), 
                reader.ReadInt(), reader.ReadVector3(), reader.ReadVector3());
        }

        public static void WriteSpawn(this Writer writer, Entity entity)
        {
            writer.Write(entity.m_asset);
            writer.Write(entity.identity.Value);
            writer.Write(entity.authority.Value);
            writer.Write(entity.transform.position);
            writer.Write(entity.transform.rotation.eulerAngles);
        }

        public struct Cease
        {
            public Cease(int identity)
            {
                Identity = identity;
            }

            public int Identity { get; }
        }

        public static Cease ReadCease(this Reader reader)
        {
            return new Cease(reader.ReadInt());
        }

        public static void WriteCease(this Writer writer, int identity)
        {
            writer.Write(identity);
        }
    }
}