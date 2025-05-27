using slocLoader.Extensions;
using slocLoader.Objects;
using slocLoader.TriggerActions;

namespace slocLoader.Readers;

public sealed class Ver6Reader : IObjectReader
{

    public slocHeader ReadHeader(BinaryReader stream)
    {
        var count = stream.ReadObjectCount();
        var attributes = (slocAttributes) stream.ReadByte();
        var defaultFlags = attributes.HasFlagFast(slocAttributes.DefaultFlags)
            ? (PrimitiveObjectFlags) stream.ReadByte()
            : PrimitiveObjectFlags.None;
        return new slocHeader(6, count, attributes, defaultFlags);
    }

    public slocGameObject Read(BinaryReader stream, slocHeader header)
    {
        var type = (ObjectType) stream.ReadByte();
        return type switch
        {
            ObjectType.Light => ReadLight(stream, header),
            ObjectType.Structure => ReadStructure(stream, header),
            ObjectType.Empty => ReadEmpty(stream, header),
            ObjectType.Cube or
                ObjectType.Sphere or
                ObjectType.Capsule or
                ObjectType.Cylinder or
                ObjectType.Plane or
                ObjectType.Quad => ReadPrimitive(stream, type, header),
            ObjectType.Capybara => ReadCapybara(stream, header),
            ObjectType.Scp079Camera => ReadCamera(stream, header),
            ObjectType.Speaker => ReadSpeaker(stream, header),
            _ => null
        };
    }

    public static LightObject ReadLight(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var color = Ver3Reader.ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
        var range = stream.ReadSingle();
        var intensity = stream.ReadSingle();
        var lightData = stream.ReadByte();
        LightObject.ByteToLightEnums(lightData, out var lightShadows, out var lightType);
        var light = new LightObject(properties.InstanceId)
        {
            LightColor = color,
            Range = range,
            Intensity = intensity,
            ShadowType = lightShadows,
            LightType = lightType
        }.ApplyProperties(properties);
        if (lightShadows != LightShadows.None)
            light.ShadowStrength = stream.ReadShortAsFloat();
        if (lightType != LightType.Spot)
            return light;
        light.SpotAngle = stream.ReadShortAsFloat();
        light.InnerSpotAngle = stream.ReadShortAsFloat();
        return light;
    }

    public static StructureObject ReadStructure(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var typeData = stream.ReadByte();
        return new StructureObject(properties.InstanceId, (StructureObject.StructureType) (typeData & ~StructureObject.RemoveDefaultLootBit))
        {
            RemoveDefaultLoot = (typeData & StructureObject.RemoveDefaultLootBit) != 0
        }.ApplyProperties(properties);
    }

    public static EmptyObject ReadEmpty(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        return new EmptyObject(properties.InstanceId).ApplyProperties(properties);
    }

    public static PrimitiveObject ReadPrimitive(BinaryReader stream, ObjectType type, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var color = Ver3Reader.ReadColor(stream, header.HasAttribute(slocAttributes.LossyColors));
        var flags = (PrimitiveObjectFlags) stream.ReadByte();
        var primitive = new PrimitiveObject(properties.InstanceId, type)
        {
            MaterialColor = color,
            Flags = flags
        }.ApplyProperties(properties);
        if (!flags.HasFlagFast(PrimitiveObjectFlags.Trigger) && !header.HasAttribute(slocAttributes.ExportAllTriggerActions))
            return primitive;
        var reader = ActionManager.GetReader(header.Version);
        primitive.TriggerActions = ActionManager.ReadActions(stream, reader);
        return primitive;
    }

    public static CapybaraObject ReadCapybara(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var collidable = stream.ReadBoolean();
        return new CapybaraObject(properties.InstanceId) {Collidable = collidable}.ApplyProperties(properties);
    }

    public static Scp079CameraObject ReadCamera(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var label = stream.ReadNullableString();
        var type = (Scp079CameraType) stream.ReadByte();
        var vMin = stream.ReadShortAsFloat();
        var vMax = stream.ReadShortAsFloat();
        var hMin = stream.ReadShortAsFloat();
        var hMax = stream.ReadShortAsFloat();
        var zoomMin = stream.ReadShortAsFloat();
        var zoomMax = stream.ReadShortAsFloat();
        return new Scp079CameraObject(type, properties.InstanceId)
        {
            Label = label,
            VerticalMinimum = vMin,
            VerticalMaximum = vMax,
            HorizontalMinimum = hMin,
            HorizontalMaximum = hMax,
            ZoomMinimum = zoomMin,
            ZoomMaximum = zoomMax
        }.ApplyProperties(properties);
    }

    public static SpeakerObject ReadSpeaker(BinaryReader stream, slocHeader header)
    {
        var properties = CommonObjectProperties.FromStream(stream, header);
        var id = stream.ReadByte();
        var spatial = stream.ReadBoolean();
        var volume = stream.ReadSingle();
        var minDistance = stream.ReadSingle();
        var maxDistance = stream.ReadSingle();
        return new SpeakerObject(properties.InstanceId)
        {
            ControllerId = id,
            Spatial = spatial,
            Volume = volume,
            MinDistance = minDistance,
            MaxDistance = maxDistance
        }.ApplyProperties(properties);
    }

}
