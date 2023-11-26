using slocLoader.Objects;
using slocLoader.Readers;

namespace slocLoader;

public static partial class API
{

    #region Reader Declarations

    public static readonly IObjectReader DefaultReader = new Ver5Reader();

    private static readonly Dictionary<ushort, IObjectReader> VersionReaders = new()
    {
        {1, new Ver1Reader()},
        {2, new Ver2Reader()},
        {3, new Ver3Reader()},
        {4, new Ver4Reader()},
        {5, DefaultReader}
    };

    public static bool TryGetReader(ushort version, out IObjectReader reader) => VersionReaders.TryGetValue(version, out reader);

    public static IObjectReader GetReader(ushort version) => TryGetReader(version, out var reader) ? reader : DefaultReader;

    #endregion

    #region Read

    // starting from v3, the version is only a ushort instead of a uint
    private static ushort ReadVersionSafe(BufferedStream buffered, BinaryReader binaryReader)
    {
        var newVersion = binaryReader.ReadUInt16();
        var oldVersion = binaryReader.ReadUInt16();
        if (oldVersion is 0)
            return (ushort) (newVersion | (uint) oldVersion << 16);
        var newPos = buffered.Get<int>("_readPos") - sizeof(ushort);
        buffered.Set("_readPos", newPos); // rewind the buffer by two bytes, so the whole stream won't be malformed data
        return newVersion;
    }

    public static List<slocGameObject> ReadObjects(Stream stream, bool autoClose = true)
    {
        var objects = new List<slocGameObject>();
        using var buffered = new BufferedStream(stream, 4);
        var binaryReader = new BinaryReader(buffered);
        var version = ReadVersionSafe(buffered, binaryReader);
        if (!VersionReaders.ContainsKey(version))
            Log.Warn($"Unknown sloc version: {version}\nAttempting to read it using the default reader.");
        var reader = GetReader(version);
        var header = reader.ReadHeader(binaryReader);
        var objectCount = header.ObjectCount;
        for (var i = 0; i < objectCount; i++)
        {
            var obj = ReadObject(binaryReader, header, version, reader);
            if (obj is {IsValid: true})
                objects.Add(obj);
        }

        if (autoClose)
            binaryReader.Close();
        return objects;
    }

    public static List<slocGameObject> ReadObjectsFromFile(string path) => ReadObjects(File.OpenRead(path));

    #endregion

}
