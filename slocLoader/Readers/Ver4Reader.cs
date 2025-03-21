using slocLoader.Objects;
using slocLoader.TriggerActions;

#pragma warning disable CS0618

namespace slocLoader.Readers;

public sealed class Ver4Reader : IObjectReader
{

    public slocHeader ReadHeader(BinaryReader stream) => Ver3Reader.ReadHeaderStatic(stream, 4);

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
            ObjectType.Light => Ver3Reader.ReadLight(stream, header),
            ObjectType.Empty => Ver2Reader.ReadEmpty(stream),
            _ => null
        };
    }

    public static slocGameObject ReadPrimitive(BinaryReader stream, ObjectType type, slocHeader header)
    {
        var primitive = Ver3Reader.ReadPrimitive(stream, type, header);
        if (!primitive.ColliderMode.IsTrigger() && !header.HasAttribute(slocAttributes.ExportAllTriggerActions))
            return primitive;
        var reader = ActionManager.GetReader(header.Version);
        primitive.TriggerActions = ActionManager.ReadActions(stream, reader);
        return primitive;
    }

}
