using slocLoader.Extensions;
using slocLoader.Objects;

namespace slocLoader.Readers;

public readonly struct slocHeader
{

    public readonly ushort Version;

    public readonly int ObjectCount;

    public readonly slocAttributes Attributes;

    [Obsolete("Use DefaultFlags instead.")]
    public readonly PrimitiveObject.ColliderCreationMode DefaultColliderMode;

    public readonly PrimitiveObjectFlags DefaultFlags;

    [Obsolete]
    public slocHeader(ushort version, int objectCount, slocAttributes attributes = slocAttributes.None, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset)
    {
        ObjectCount = objectCount;
        Version = version;
        Attributes = attributes;
        DefaultColliderMode = defaultColliderMode;
        DefaultFlags = Attributes.HasFlagFast(slocAttributes.DefaultColliderMode)
            ? ColliderModeCompatibility.GetPrimitiveFlags(defaultColliderMode)
            : PrimitiveObjectFlags.None;
    }

    [Obsolete]
    public slocHeader(ushort version, int objectCount, byte attributes, PrimitiveObject.ColliderCreationMode defaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset)
        : this(version, objectCount, (slocAttributes) attributes, defaultColliderMode)
    {
    }

    public slocHeader(ushort version, int objectCount, slocAttributes attributes, PrimitiveObjectFlags defaultFlags)
    {
        Version = version;
        ObjectCount = objectCount;
        Attributes = attributes;
#pragma warning disable CS0618 // Type or member is obsolete
        DefaultColliderMode = PrimitiveObject.ColliderCreationMode.Unset;
#pragma warning restore CS0618 // Type or member is obsolete
        DefaultFlags = defaultFlags;
    }

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(ObjectCount);
        writer.Write((byte) Attributes);
        if (Attributes.HasFlagFast(slocAttributes.DefaultFlags))
            writer.Write((byte) DefaultFlags);
    }

}
