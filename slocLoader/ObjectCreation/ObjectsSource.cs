using System.Collections;
using slocLoader.Objects;

namespace slocLoader.ObjectCreation;

public readonly struct ObjectsSource : IEnumerable<slocGameObject>
{

    public readonly IEnumerable<slocGameObject> Objects;

    public IEnumerator<slocGameObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ObjectsSource(IEnumerable<slocGameObject> objects) => Objects = objects;

    public static ObjectsSource From(IEnumerable<slocGameObject> objects) => new(objects);

    public static ObjectsSource FromFile(string path) => new(API.ReadObjectsFromFile(path));

    public static ObjectsSource FromStream(Stream stream, bool autoClose = true) => new(API.ReadObjects(stream, autoClose));

    public static implicit operator ObjectsSource(List<slocGameObject> objects) => new(objects);

    public static implicit operator ObjectsSource(slocGameObject[] objects) => new(objects);

    public static implicit operator ObjectsSource(string filePath) => FromFile(filePath);

    public static implicit operator ObjectsSource(Stream stream) => FromStream(stream);

}
