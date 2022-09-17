using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public readonly struct slocHeader {

        public readonly int ObjectCount;

        public readonly slocAttributes Attributes;

        public readonly PrimitiveObject.ColliderCreationMode DefaultColliderMode;

        public slocHeader(int objectCount, slocAttributes attributes = slocAttributes.None, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset) {
            ObjectCount = objectCount;
            Attributes = attributes;
            DefaultColliderMode = defaultColliderMode;
        }

        public slocHeader(int objectCount, byte attributes, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset) {
            ObjectCount = objectCount;
            Attributes = (slocAttributes) attributes;
            DefaultColliderMode = defaultColliderMode;
        }

        public void WriteTo(BinaryWriter writer) {
            writer.Write(ObjectCount);
            writer.Write((byte) Attributes);
            if (Attributes.HasFlagFast(slocAttributes.ForcedColliderMode))
                writer.Write((byte) DefaultColliderMode);
        }

    }

}
