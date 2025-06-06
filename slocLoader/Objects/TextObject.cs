using AdminToys;
using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class TextObject : slocGameObject
{

    public static Vector2 DefaultDisplaySize => TextToy.DefaultDisplaySize;

    public TextObject(string format, int instanceId = 0) : base(instanceId) => Format = format;

    public string Format;

    public string[] Arguments = [];

    public Vector2 DisplaySize = DefaultDisplaySize;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.Write(Format);
        writer.Write(Arguments.Length);
        foreach (var argument in Arguments)
            writer.Write(argument);
        writer.Write(DisplaySize.x);
        writer.Write(DisplaySize.y);
    }

}
