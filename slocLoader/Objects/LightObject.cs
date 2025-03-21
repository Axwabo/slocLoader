using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class LightObject : slocGameObject
{

    private const int TwoBitMask = 0b00000011;

    public static byte LightEnumsToByte(LightShadows shadows, LightType type, LightShape shape)
    {
        var shadowsBits = (byte) shadows & TwoBitMask;
        var typeBits = (byte) type & TwoBitMask;
        var shapeBits = (byte) shape & TwoBitMask;
        return (byte) (shadowsBits | typeBits << 2 | shapeBits << 4);
    }

    public static void ByteToLightEnums(byte data, out LightShadows shadows, out LightType type, out LightShape shape)
    {
        shadows = (LightShadows) (data & TwoBitMask);
        type = (LightType) (data >> 2 & TwoBitMask);
        shape = (LightShape) (data >> 4 & TwoBitMask);
    }

    public LightObject() : this(0)
    {
    }

    public LightObject(int instanceId) : base(instanceId) => Type = ObjectType.Light;

    public Color LightColor = Color.white;

    [Obsolete("Use ShadowType instead.")]
    public bool Shadows = true;

    public float Range = 5;

    public float Intensity = 1;

    public LightShadows ShadowType = LightShadows.Soft;

    public float ShadowStrength = 1;

    public LightType LightType = LightType.Point;

    public LightShape Shape;

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
        writer.Write(LightEnumsToByte(ShadowType, LightType, Shape));
        if (ShadowType != LightShadows.None)
            writer.WriteFloatAsShort(ShadowStrength);
        if (LightType != LightType.Spot)
            return;
        writer.WriteFloatAsShort(SpotAngle);
        writer.WriteFloatAsShort(InnerSpotAngle);
    }

}
