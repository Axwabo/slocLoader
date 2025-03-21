using slocLoader.Extensions;
using slocLoader.Objects;

namespace slocLoader.Readers;

public readonly struct CommonObjectProperties
{

    public const int MinVersion = 6;

    public static CommonObjectProperties FromStream(BinaryReader stream, slocHeader header)
    {
        if (header.Version < MinVersion)
            throw new InvalidOperationException($"{nameof(CommonObjectProperties)} is only supported from version 6 and up.");
        var instanceId = stream.ReadInt32();
        var parentId = stream.ReadInt32();
        var transform = stream.ReadTransform();
        if (!header.HasAttribute(slocAttributes.NamesAndTags))
            return new CommonObjectProperties(instanceId, parentId, transform);
        var name = stream.ReadNullableString();
        var tag = stream.ReadNullableString();
        return new CommonObjectProperties(instanceId, parentId, transform, name, tag);
    }

    public readonly int InstanceId;

    public readonly int ParentId;

    public readonly slocTransform Transform;

    public readonly string Name;

    public readonly string Tag;

    public CommonObjectProperties(int instanceId, int parentId, slocTransform transform, string name = null, string tag = null)
    {
        InstanceId = instanceId;
        ParentId = parentId;
        Transform = transform;
        Name = name;
        Tag = tag;
    }

}
