using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public sealed class Ver1Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) => new(stream.ReadObjectCount());

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube => ReadPrimitive(stream, objectType),
                ObjectType.Sphere => ReadPrimitive(stream, objectType),
                ObjectType.Cylinder => ReadPrimitive(stream, objectType),
                ObjectType.Plane => ReadPrimitive(stream, objectType),
                ObjectType.Capsule => ReadPrimitive(stream, objectType),
                ObjectType.Light => ReadLight(stream),
                _ => null
            };
        }

        public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type) {
            var transform = stream.ReadTransform();
            var materialColor = stream.ReadColor();
            return new PrimitiveObject(0, type) {
                Transform = transform,
                MaterialColor = materialColor
            };
        }

        public static slocGameObject ReadLight(BinaryReader stream) {
            var transform = stream.ReadTransform();
            var lightColor = stream.ReadColor();
            var shadows = stream.ReadBoolean();
            var range = stream.ReadSingle();
            var intensity = stream.ReadSingle();
            return new LightObject(0) {
                Transform = transform,
                LightColor = lightColor,
                Shadows = shadows,
                Range = range,
                Intensity = intensity,
            };
        }

    }

}
