using System.IO;
using slocLoader.Objects;
using UnityEngine;

namespace slocLoader.Readers {

    public class Ver3Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) {
            var count = stream.ReadInt32();
            var attributes = (slocAttributes) stream.ReadByte();
            return new(count, attributes, attributes.HasFlagFast(slocAttributes.ForcedColliderMode) ? (PrimitiveObject.ColliderCreationMode) stream.ReadByte() : PrimitiveObject.ColliderCreationMode.Unset);
        }

        public slocGameObject Read(BinaryReader stream, slocHeader header) {
            var objectType = (ObjectType) stream.ReadByte();
            return objectType switch {
                ObjectType.Cube => ReadPrimitive(stream, objectType, header),
                ObjectType.Sphere => ReadPrimitive(stream, objectType, header),
                ObjectType.Cylinder => ReadPrimitive(stream, objectType, header),
                ObjectType.Plane => ReadPrimitive(stream, objectType, header),
                ObjectType.Capsule => ReadPrimitive(stream, objectType, header),
                ObjectType.Light => ReadLight(stream, header),
                ObjectType.Empty => ReadEmpty(stream),
                _ => null
            };
        }

        public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type, slocHeader header) {
            var instanceId = stream.ReadInt32();
            var parentId = stream.ReadInt32();
            var slocTransform = stream.ReadTransform();
            var color = ReadColor(stream, header.Attributes.HasFlagFast(slocAttributes.LossyColors));
            var creationMode = header.Attributes.HasFlagFast(slocAttributes.ForcedColliderMode) ? header.DefaultColliderMode : (PrimitiveObject.ColliderCreationMode) stream.ReadByte();
            return new PrimitiveObject(instanceId, type) {
                ParentId = parentId,
                Transform = slocTransform,
                MaterialColor = color,
                ColliderMode = creationMode
            };
        }

        public static slocGameObject ReadLight(BinaryReader stream, slocHeader header) => new LightObject(stream.ReadInt32()) {
            ParentId = stream.ReadInt32(),
            Transform = stream.ReadTransform(),
            LightColor = ReadColor(stream, header.Attributes.HasFlagFast(slocAttributes.LossyColors)),
            Shadows = stream.ReadBoolean(),
            Range = stream.ReadSingle(),
            Intensity = stream.ReadSingle(),
        };

        private static Color ReadColor(BinaryReader stream, bool lossy) => lossy ? stream.ReadLossyColor() : stream.ReadColor();

        public static slocGameObject ReadEmpty(BinaryReader stream) => new EmptyObject(stream.ReadInt32()) {
            ParentId = stream.ReadInt32(),
            Transform = stream.ReadTransform()
        };

    }

}
