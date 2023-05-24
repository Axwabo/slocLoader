using slocLoader.Objects;

namespace slocLoader.Readers;

public sealed class Ver3Reader : IObjectReader
{

    public slocHeader ReadHeader(BinaryReader stream) => ReadHeaderStatic(stream, 3);

    public slocGameObject Read(BinaryReader stream, slocHeader header)
    {
        var objectType = (ObjectType) stream.ReadByte();
        return objectType switch
        {
            ObjectType.Cube
                or ObjectType.Sphere
                or ObjectType.Cylinder
                or ObjectType.Plane
                or ObjectType.Capsule
                or ObjectType.Quad => ReadPrimitive(stream, objectType, header),
            ObjectType.Light => ReadLight(stream, header),
            ObjectType.Empty => Ver2Reader.ReadEmpty(stream),
            _ => null
        };
    }

    public static slocHeader ReadHeaderStatic(BinaryReader stream, ushort version)
    {
        var count = stream.ReadObjectCount();
        var attributes = (slocAttributes) stream.ReadByte();
        var colliderCreationMode = attributes.HasFlagFast(slocAttributes.DefaultColliderMode)
            ? (PrimitiveObject.ColliderCreationMode) stream.ReadByte()
            : PrimitiveObject.ColliderCreationMode.Unset;
        return new slocHeader(
            version,
            count,
            attributes,
            colliderCreationMode
        );
    }

    public static PrimitiveObject ReadPrimitive(BinaryReader stream, ObjectType type, slocHeader header)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var slocTransform = stream.ReadTransform();
        var color = ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
        var colliderCreationMode = (PrimitiveObject.ColliderCreationMode) stream.ReadByte();
        var creationMode = colliderCreationMode is PrimitiveObject.ColliderCreationMode.Unset && header.HasAttribute(slocAttributes.DefaultColliderMode)
            ? header.DefaultColliderMode
            : colliderCreationMode;
        return new PrimitiveObject(instanceId, type)
        {
            ParentId = parentId,
            Transform = slocTransform,
            MaterialColor = color,
            ColliderMode = creationMode
        };
    }

    public static LightObject ReadLight(BinaryReader stream, slocHeader header)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        var lightColor = ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
        var shadows = stream.ReadBoolean();
        var range = stream.ReadSingle();
        var intensity = stream.ReadSingle();
        return new LightObject(instanceId)
        {
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
