using slocLoader.Objects;

namespace slocLoader.Readers;

public readonly struct slocHeader
{

    public readonly ushort Version;

    public readonly int ObjectCount;

    public readonly slocAttributes Attributes;

    public readonly PrimitiveObject.ColliderCreationMode DefaultColliderMode;

    public slocHeader(ushort version, int objectCount, slocAttributes attributes = slocAttributes.None, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset)
    {
        ObjectCount = objectCount;
        Version = version;
        Attributes = attributes;
        DefaultColliderMode = defaultColliderMode;
    }

    public slocHeader(ushort version, int objectCount, byte attributes, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset)
    {
        ObjectCount = objectCount;
        Version = version;
        Attributes = (slocAttributes) attributes;
        DefaultColliderMode = defaultColliderMode;
    }

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(ObjectCount);
        writer.Write((byte) Attributes);
        if (Attributes.HasFlagFast(slocAttributes.DefaultColliderMode))
            writer.Write((byte) DefaultColliderMode);
    }

}
