using slocLoader.Extensions;
using slocLoader.Objects;

namespace slocLoader.Readers;

public sealed class Ver5Reader : IObjectReader
{

    public slocHeader ReadHeader(BinaryReader stream) => Ver3Reader.ReadHeaderStatic(stream, 5);

    public slocGameObject Read(BinaryReader stream, slocHeader header)
    {
        var objectType = (ObjectType) stream.ReadByte();
        return objectType switch
        {
            ObjectType.Structure => ReadStructure(stream),
            ObjectType.Cube
                or ObjectType.Sphere
                or ObjectType.Cylinder
                or ObjectType.Plane
                or ObjectType.Capsule
                or ObjectType.Quad => Ver4Reader.ReadPrimitive(stream, objectType, header),
            ObjectType.Light => Ver3Reader.ReadLight(stream, header),
            ObjectType.Empty => Ver2Reader.ReadEmpty(stream),
            _ => null
        };
    }

    public static StructureObject ReadStructure(BinaryReader stream)
    {
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        stream.ReadByteWithBool(out var type, out var removeDefaultLoot);
        return new StructureObject(instanceId, (StructureObject.StructureType) type)
        {
            ParentId = parentId,
            Transform = transform,
            RemoveDefaultLoot = removeDefaultLoot
        };
    }

}
