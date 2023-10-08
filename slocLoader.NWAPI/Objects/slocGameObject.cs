using slocLoader.Readers;

namespace slocLoader.Objects;

public abstract class slocGameObject
{

    protected slocGameObject(int instanceId) => InstanceId = instanceId;

    public readonly int InstanceId;

    public int ParentId = 0;

    public bool HasParent => ParentId != InstanceId;

    public ObjectType Type { get; protected set; } = ObjectType.None;

    public slocTransform Transform = new();
    public byte MovementSmoothing;

    public virtual bool IsValid => Type != ObjectType.None;

    public void WriteTo(BinaryWriter writer, slocHeader header)
    {
        writer.Write((byte) Type);
        writer.Write(InstanceId);
        writer.Write(ParentId);
        Transform.WriteTo(writer);
        WriteData(writer, header);
    }

    protected virtual void WriteData(BinaryWriter writer, slocHeader header)
    {
    }

}
