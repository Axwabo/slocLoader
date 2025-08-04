using slocLoader.Objects;

#pragma warning disable CS0612

namespace slocLoader.Readers;

public sealed class Ver2Reader : IObjectReader
{

    public slocHeader ReadHeader(BinaryReader stream) => new(2, stream.ReadObjectCount());

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
                or ObjectType.Quad => ReadPrimitive(stream, objectType),
            ObjectType.Light => ReadLight(stream),
            ObjectType.Empty => ReadEmpty(stream),
            _ => null
        };
    }

    public static PrimitiveObject ReadPrimitive(BinaryReader stream, ObjectType type)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        var materialColor = stream.ReadColor();
        return new PrimitiveObject(instanceId, type)
        {
            ParentId = parentId,
            Transform = transform,
            MaterialColor = materialColor
        };
    }

    public static LightObject ReadLight(BinaryReader stream)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        var lightColor = stream.ReadColor();
        var shadows = stream.ReadBoolean();
        var range = stream.ReadSingle();
        var intensity = stream.ReadSingle();
        return new LightObject(instanceId)
        {
            ParentId = parentId,
            Transform = transform,
            LightColor = lightColor,
            ShadowType = shadows ? LightShadows.Soft : LightShadows.None,
            Range = range,
            Intensity = intensity,
        };
    }

    public static EmptyObject ReadEmpty(BinaryReader stream)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        return new EmptyObject(instanceId)
        {
            ParentId = parentId,
            Transform = transform
        };
    }

}
