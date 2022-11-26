using System.IO;
using slocLoader.Objects;
using UnityEngine;

namespace slocLoader.Readers {

    public sealed class Ver3Reader : IObjectReader {

        public slocHeader ReadHeader(BinaryReader stream) {
            var count = stream.ReadObjectCount();
            var attributes = (slocAttributes) stream.ReadByte();
            var colliderCreationMode = attributes.HasFlagFast(slocAttributes.DefaultColliderMode)
                ? (PrimitiveObject.ColliderCreationMode) stream.ReadByte()
                : PrimitiveObject.ColliderCreationMode.Unset;
            return new slocHeader(count,
                attributes,
                colliderCreationMode
            );
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
                ObjectType.Empty => Ver2Reader.ReadEmpty(stream),
                _ => null
            };
        }

        public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type, slocHeader header) {
            var instanceId = stream.ReadInt32();
            var parentId = stream.ReadInt32();
            var slocTransform = stream.ReadTransform();
            var color = ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
            var colliderCreationMode = (PrimitiveObject.ColliderCreationMode) stream.ReadByte();
            var creationMode = colliderCreationMode is PrimitiveObject.ColliderCreationMode.Unset && header.HasAttribute(slocAttributes.DefaultColliderMode)
                ? header.DefaultColliderMode
                : colliderCreationMode;
            return new PrimitiveObject(instanceId, type) {
                ParentId = parentId,
                Transform = slocTransform,
                MaterialColor = color,
                ColliderMode = creationMode
            };
        }

        public static slocGameObject ReadLight(BinaryReader stream, slocHeader header) {
            var instanceId = stream.ReadInt32();
            var parentId = stream.ReadInt32();
            var transform = stream.ReadTransform();
            var lightColor = ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
            var shadows = stream.ReadBoolean();
            var range = stream.ReadSingle();
            var intensity = stream.ReadSingle();
            return new LightObject(instanceId) {
                ParentId = parentId,
                Transform = transform,
                LightColor = lightColor,
                Shadows = shadows,
                Range = range,
                Intensity = intensity,
            };
        }

        public static Color ReadColor(BinaryReader stream, bool lossy) => lossy ? stream.ReadLossyColor() : stream.ReadColor();

    }

}
