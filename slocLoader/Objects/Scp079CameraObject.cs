using slocLoader.Extensions;
using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class Scp079CameraObject : slocGameObject
{

    public Scp079CameraObject(Scp079CameraType cameraType, int instanceId = 0) : base(instanceId)
    {
        CameraType = cameraType;
        Type = ObjectType.Scp079Camera;
    }

    public string Label;

    public Scp079CameraType CameraType;

    public float VerticalMinimum;

    public float VerticalMaximum;

    public float HorizontalMinimum;

    public float HorizontalMaximum;

    public float ZoomMinimum;

    public float ZoomMaximum;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.WriteNullableString(Label);
        writer.Write((byte) CameraType);
        writer.Write(VerticalMinimum);
        writer.Write(VerticalMaximum);
        writer.Write(HorizontalMinimum);
        writer.Write(HorizontalMaximum);
        writer.Write(ZoomMinimum);
        writer.Write(ZoomMaximum);
    }

}
