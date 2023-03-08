using System.IO;
using slocLoader.Objects;

namespace slocLoader.Readers {

    public sealed class Ver1Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) => new(1, stream.ReadObjectCount());

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube
                    or ObjectType.Sphere
                    or ObjectType.Cylinder
                    or ObjectType.Plane
                    or ObjectType.Capsule
                    or ObjectType.Quad => ReadPrimitive(stream, objectType),
                ObjectType.Light => ReadLight(stream),
                _ => null
            };
        }

        public static PrimitiveObject ReadPrimitive(BinaryReader stream, ObjectType type) {
            var transform = stream.ReadTransform();
            var materialColor = stream.ReadColor();
            return new PrimitiveObject(0, type) {
                Transform = transform,
                MaterialColor = materialColor
            };
        }

        public static LightObject ReadLight(BinaryReader stream) {
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
