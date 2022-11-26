using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public sealed class Ver2Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) => new(stream.ReadObjectCount());

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube => ReadPrimitive(stream, objectType),
                ObjectType.Sphere => ReadPrimitive(stream, objectType),
                ObjectType.Cylinder => ReadPrimitive(stream, objectType),
                ObjectType.Plane => ReadPrimitive(stream, objectType),
                ObjectType.Capsule => ReadPrimitive(stream, objectType),
                ObjectType.Light => Ver1Reader.ReadLight(stream),
                ObjectType.Empty => ReadEmpty(stream),
                _ => null
            };
        }

        public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type) {
            var instanceId = stream.ReadInt32();
            var parentId = stream.ReadInt32();
            var transform = stream.ReadTransform();
            var materialColor = stream.ReadColor();
            return new PrimitiveObject(instanceId, type) {
                ParentId = parentId,
                Transform = transform,
                MaterialColor = materialColor
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
