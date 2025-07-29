using slocLoader.Objects;
using slocLoader.Readers;
using slocLoader.TriggerActions.Data;

namespace slocLoader;

public static partial class API
{

    #region BinaryReader Extensions

    public static slocGameObject ReadObject(this BinaryReader stream, slocHeader header, ushort version = 0, IObjectReader objectReader = null)
    {
        objectReader ??= GetReader(version);
        return objectReader.Read(stream, header);
    }

    public static slocTransform ReadTransform(this BinaryReader reader) => new()
    {
        Position = reader.ReadVector(),
        Scale = reader.ReadVector(),
        Rotation = reader.ReadQuaternion()
    };

    public static Vector3 ReadVector(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Quaternion ReadQuaternion(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Color ReadColor(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Color ReadLossyColor(this BinaryReader reader)
    {
        var color = reader.ReadInt32();
        var red = color >> 24 & 0xFF;
        var green = color >> 16 & 0xFF;
        var blue = color >> 8 & 0xFF;
        var alpha = color & 0xFF;
        return new(red * ColorDivisionMultiplier, green * ColorDivisionMultiplier, blue * ColorDivisionMultiplier, alpha * ColorDivisionMultiplier);
    }

    public static int ReadObjectCount(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        return count < 0 ? 0 : count;
    }

    public static float ReadShortAsFloat(this BinaryReader reader) => reader.ReadInt16() * TeleporterImmunityData.ShortToFloatMultiplier;

    #endregion

    #region BinaryWriter Extensions

    public static void WriteVector(this BinaryWriter writer, Vector3 vector3)
    {
        writer.Write(vector3.x);
        writer.Write(vector3.y);
        writer.Write(vector3.z);
    }

    public static void WriteQuaternion(this BinaryWriter writer, Quaternion quaternion)
    {
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
        writer.Write(quaternion.w);
    }

    public static void WriteColor(this BinaryWriter writer, Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    public static void WriteFloatAsShort(this BinaryWriter writer, float value) => writer.Write((ushort) Mathf.Floor(value * TeleporterImmunityData.FloatToShortMultiplier));

    #endregion

    #region Bit Math

    [Obsolete("Collider modes have been replaced by primitive flags.", true)]
    public static PrimitiveObject.ColliderCreationMode CombineSafe(PrimitiveObject.ColliderCreationMode a, PrimitiveObject.ColliderCreationMode b) =>
        (PrimitiveObject.ColliderCreationMode) CombineSafe((byte) a, (byte) b);

    [Obsolete("Collider modes have been replaced by primitive flags.", true)]
    public static void SplitSafe(PrimitiveObject.ColliderCreationMode combined, out PrimitiveObject.ColliderCreationMode a, out PrimitiveObject.ColliderCreationMode b)
    {
        SplitSafe((byte) combined, out var x, out var y);
        a = (PrimitiveObject.ColliderCreationMode) x;
        b = (PrimitiveObject.ColliderCreationMode) y;
    }

    public static byte CombineSafe(byte a, byte b) => (byte) (a << 4 | b);

    public static void SplitSafe(byte combined, out byte a, out byte b)
    {
        a = (byte) (combined >> 4);
        b = (byte) (combined & 0xF);
    }

    #endregion

}
