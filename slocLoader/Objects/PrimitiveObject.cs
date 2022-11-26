using System;
using System.IO;
using slocLoader.Readers;
using UnityEngine;

namespace slocLoader.Objects {

    public sealed class PrimitiveObject : slocGameObject {

        public PrimitiveObject(int instanceId, ObjectType type) : base(instanceId) {
            if (type is ObjectType.None or ObjectType.Light)
                throw new ArgumentException("Invalid primitive type", nameof(type));
            Type = type;
        }

        public Color MaterialColor = Color.gray;

        public ColliderCreationMode ColliderMode { get; set; } = ColliderCreationMode.Both;

        public ColliderCreationMode GetNonUnsetColliderMode() => ColliderMode is ColliderCreationMode.Unset ? ColliderCreationMode.Both : ColliderMode;

        public override void WriteTo(BinaryWriter writer, slocHeader header) {
            base.WriteTo(writer, header);
            if (header.HasAttribute(slocAttributes.LossyColors)) {
                writer.Write(MaterialColor.ToLossyColor());
                writer.Write((byte) ColliderMode);
                return;
            }

            writer.Write(MaterialColor.r);
            writer.Write(MaterialColor.g);
            writer.Write(MaterialColor.b);
            writer.Write(MaterialColor.a);
            writer.Write((byte) ColliderMode);
        }

        public enum ColliderCreationMode : byte {

            Unset = 0,
            NoCollider = 1,
            ClientOnly = 2,
            ServerOnlySpawned = 3,
            Both = 4,
            Trigger = 5,
            ServerOnlyTrigger = 6,
            ServerOnlyNonSpawned = 7

        }

    }

}
