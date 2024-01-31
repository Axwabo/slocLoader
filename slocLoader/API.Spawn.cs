using Mirror;
using slocLoader.ObjectCreation;
using slocLoader.Objects;

namespace slocLoader;

public static partial class API
{

    #region Deprecated methods

    [Obsolete("Use SpawnObjects(ObjectSource, Vector3, Quaternion) instead")]
    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(objects, out _, position, rotation);

    [Obsolete("Use SpawnObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ObjectsSource.From(objects), new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out spawnedAmount);

    [Obsolete("Use SpawnObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject SpawnObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

    [Obsolete("Use SpawnObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject SpawnObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

    #endregion

    public static GameObject SpawnObject(this slocGameObject obj, GameObject parent = null, bool throwOnError = true)
    {
        var o = CreateObject(obj, parent, throwOnError);
        if (o == null)
        {
            if (throwOnError)
                throw new ArgumentOutOfRangeException(nameof(obj), $"Failed to create object of type {obj.Type}");
            return null;
        }

        var hasData = o.TryGetComponent(out slocObjectData data);
        if (hasData && !data.ShouldBeSpawnedOnClient)
            return o;
        if (hasData)
            SpawnObjectWithGlobalTransform(o);
        else
            NetworkServer.Spawn(o);
        return o;
    }

    public static bool ShouldSpawnWithGlobalTransform;

    public static void SpawnObjectWithGlobalTransform(GameObject gameObject)
    {
        ShouldSpawnWithGlobalTransform = true;
        try
        {
            NetworkServer.Spawn(gameObject);
        }
        finally
        {
            ShouldSpawnWithGlobalTransform = false;
        }
    }

    public static GameObject SpawnObjects(ObjectsSource source, CreateOptions options, out int spawnedAmount)
        => CreateOrSpawn(source, options, true, SpawnObject, out spawnedAmount);

    public static GameObject SpawnObjects(ObjectsSource source, CreateOptions options)
        => SpawnObjects(source, options, out _);

    public static GameObject SpawnObjects(ObjectsSource source, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(source, new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out spawnedAmount);

    public static GameObject SpawnObjects(ObjectsSource source, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(source, out _, position, rotation);

}
