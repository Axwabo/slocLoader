using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public sealed class Ver2Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) => new (stream.ReadObjectCount());

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube => ReadPrimitive(stream, objectType),
                ObjectType.Sphere => ReadPrimitive(stream, objectType),
                ObjectType.Cylinder => ReadPrimitive(stream, objectType),
                ObjectType.Plane => ReadPrimitive(stream, objectType),
                ObjectType.Capsule => ReadPrimitive(stream, objectType),
                ObjectType.Light => ReadLight(stream),
                ObjectType.Empty => ReadEmpty(stream),
                _ => null
            };
        }

        public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type) => new PrimitiveObject(stream.ReadInt32(), type) {
            ParentId = stream.ReadInt32(),
            Transform = stream.ReadTransform(),
            MaterialColor = stream.ReadColor()
        };

        public static slocGameObject ReadLight(BinaryReader stream) => new LightObject(stream.ReadInt32()) {
            ParentId = stream.ReadInt32(),
            Transform = stream.ReadTransform(),
            LightColor = stream.ReadColor(),
            Shadows = stream.ReadBoolean(),
            Range = stream.ReadSingle(),
            Intensity = stream.ReadSingle(),
        };

        public static slocGameObject ReadEmpty(BinaryReader stream) => new EmptyObject(stream.ReadInt32()) {
            ParentId = stream.ReadInt32(),
            Transform = stream.ReadTransform()
        };

    }

}
