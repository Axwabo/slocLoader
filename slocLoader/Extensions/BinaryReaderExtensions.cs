using System.Text;

namespace slocLoader.Extensions;

public static class BinaryReaderExtensions
{

    public static string ReadNullableString(this BinaryReader reader)
    {
        var length = reader.ReadUInt16() - 1;
        return length switch
        {
            -1 => null,
            0 => "",
            _ => Encoding.UTF8.GetString(reader.ReadBytes(length))
        };
    }

    public static void ReadByteWithBool(this BinaryReader reader, out byte value, out bool boolean)
    {
        var b = reader.ReadByte();
        value = (byte) (b & ~BinaryWriterExtensions.BoolBit);
        boolean = (b & BinaryWriterExtensions.BoolBit) != 0;
    }

    public static void ReadTwoBools(this BinaryReader reader, out bool a, out bool b)
    {
        var value = reader.ReadByte();
        API.SplitSafe(value, out var byteA, out var byteB);
        a = byteA != 0;
        b = byteB != 0;
    }

}
