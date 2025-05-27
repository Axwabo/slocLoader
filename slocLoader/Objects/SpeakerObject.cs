using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class SpeakerObject : slocGameObject
{

    public SpeakerObject() : this(0)
    {
    }

    public SpeakerObject(int instanceId) : base(instanceId) => Type = ObjectType.Speaker;

    public byte ControllerId;

    public bool Spatial = true;

    public float Volume = 1;

    public float MinDistance = 1f;

    public float MaxDistance = 15f;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.Write(ControllerId);
        writer.Write(Spatial);
        writer.Write(Volume);
        writer.Write(MinDistance);
        writer.Write(MaxDistance);
    }

}
