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

    public Vector2 VerticalConstraint;

    public Vector2 HorizontalConstraint;

    public Vector2 ZoomConstraint;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.WriteNullableString(Label);
        writer.Write((byte) CameraType);
        writer.WriteFloatAsShort(VerticalConstraint.x);
        writer.WriteFloatAsShort(VerticalConstraint.y);
        writer.WriteFloatAsShort(HorizontalConstraint.x);
        writer.WriteFloatAsShort(HorizontalConstraint.y);
        writer.WriteFloatAsShort(ZoomConstraint.x);
        writer.WriteFloatAsShort(ZoomConstraint.y);
    }

}
