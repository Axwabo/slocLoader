using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public sealed class Ver2Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) => new(stream.ReadObjectCount());

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube => Ver1Reader.ReadPrimitive(stream, objectType),
                ObjectType.Sphere => Ver1Reader.ReadPrimitive(stream, objectType),
                ObjectType.Cylinder => Ver1Reader.ReadPrimitive(stream, objectType),
                ObjectType.Plane => Ver1Reader.ReadPrimitive(stream, objectType),
                ObjectType.Capsule => Ver1Reader.ReadPrimitive(stream, objectType),
                ObjectType.Light => Ver1Reader.ReadLight(stream),
                ObjectType.Empty => ReadEmpty(stream),
                _ => null
            };
        }

        public static slocGameObject ReadEmpty(BinaryReader stream) {
            var instanceId = stream.ReadInt32();
            int parentId = stream.ReadInt32();
            var transform = stream.ReadTransform();
            return new EmptyObject(instanceId) {
                ParentId = parentId,
                Transform = transform
            };
        }

    }

}
