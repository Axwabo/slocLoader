using System.Text;

namespace slocLoader.Extensions;

public static class BinaryWriterExtensions
{

    public const int BoolBit = 0b1000_0000;

    public static void WriteNullableString(this BinaryWriter writer, string value)
    {
        if (value == null)
        {
            writer.Write((ushort) 0);
            return;
        }

        if (value.Length == 0)
        {
            writer.Write((ushort) 1);
            return;
        }

        writer.Write((ushort) (value.Length + 1));
        writer.Write(Encoding.UTF8.GetBytes(value));
    }

    public static void WriteByteWithBool(this BinaryWriter writer, byte value, bool boolean)
        => writer.Write((byte) (value | (boolean ? BoolBit : 0)));

}
