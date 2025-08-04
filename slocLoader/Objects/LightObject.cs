using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class LightObject : slocGameObject
{

    private const int BitMask = 0b00001111;
    private const int BitShift = 4;

    public static byte LightEnumsToByte(LightShadows shadows, LightType type)
    {
        var shadowsBits = (byte) shadows & BitMask;
        var typeBits = (byte) type & BitMask;
        return (byte) (shadowsBits | typeBits << BitShift);
    }

    public static void ByteToLightEnums(byte data, out LightShadows shadows, out LightType type)
    {
        shadows = (LightShadows) (data & BitMask);
        type = (LightType) (data >> BitShift & BitMask);
    }

    public LightObject() : this(0)
    {
    }

    public LightObject(int instanceId) : base(instanceId) => Type = ObjectType.Light;

    public Color LightColor = Color.white;

    [Obsolete("Use ShadowType instead.", true)]
    public bool Shadows = true;

    public float Range = 5;

    public float Intensity = 1;

    public LightShadows ShadowType = LightShadows.Soft;

    public float ShadowStrength = 1;

    public LightType LightType = LightType.Point;

    public float SpotAngle;

    public float InnerSpotAngle;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        if (header.HasAttribute(slocAttributes.LossyColors))
            writer.Write(LightColor.ToLossyColor());
        else
            writer.WriteColor(LightColor);
        writer.Write(Range);
        writer.Write(Intensity);
        writer.Write(LightEnumsToByte(ShadowType, LightType));
        if (ShadowType != LightShadows.None)
            writer.WriteFloatAsShort(ShadowStrength);
        if (LightType != LightType.Spot)
            return;
        writer.WriteFloatAsShort(SpotAngle);
        writer.WriteFloatAsShort(InnerSpotAngle);
    }

}
