using System;
using System.IO;
using slocLoader.Readers;
using UnityEngine;

namespace slocLoader.Objects {

    public class PrimitiveObject : slocGameObject {

        public PrimitiveObject(int instanceId, ObjectType type) : base(instanceId) {
            if (type is ObjectType.None or ObjectType.Light)
                throw new ArgumentException("Invalid primitive type", nameof(type));
            Type = type;
        }

        public Color MaterialColor = Color.gray;
        private ColliderCreationMode _colliderMode = ColliderCreationMode.Both;

        public ColliderCreationMode ColliderMode {
            get => _colliderMode;
            set => _colliderMode = value is ColliderCreationMode.Unset ? _colliderMode : value;
        }

        public override void WriteTo(BinaryWriter writer, slocHeader header) {
            base.WriteTo(writer, header);
            if (header.Attributes.HasFlagFast(slocAttributes.LossyColors)) {
                writer.Write(MaterialColor.ToLossyColor());
                return;
            }

            writer.Write(MaterialColor.r);
            writer.Write(MaterialColor.g);
            writer.Write(MaterialColor.b);
            writer.Write(MaterialColor.a);
            if (!header.Attributes.HasFlagFast(slocAttributes.ForcedColliderMode))
                writer.Write((byte) ColliderMode);
        }

        public enum ColliderCreationMode : byte {

            Unset = 0,
            None = 1,
            ClientOnly = 2,
            ServerOnly = 3,
            Both = 4,
            Trigger = 5

        }

    }

}
