using Mirror;
using slocLoader.Objects;

namespace slocLoader;

public static partial class API
{

    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) =>
        SpawnObjects(objects, out _, position, rotation);

    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
    {
        CreatedInstances.Clear();
        TpToSpawnedCache.Clear();
        try
        {
            var go = new GameObject
            {
                transform =
                {
                    position = position,
                    rotation = rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocObjectData>();
            NetworkServer.Spawn(go);
            spawnedAmount = 0;
            foreach (var o in objects)
            {
                var gameObject = o.SpawnObject(CreatedInstances.GetOrReturn(o.ParentId, go, o.HasParent));
                if (gameObject == null)
                    continue;
                spawnedAmount++;
                CreatedInstances[o.InstanceId] = gameObject;
            }

            PostProcessSpecialTriggerActions();
            return go;
        }
        finally
        {
            CreatedInstances.Clear();
            TpToSpawnedCache.Clear();
        }
    }

    public static GameObject SpawnObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

    public static GameObject SpawnObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

    public static GameObject SpawnObject(this slocGameObject obj, GameObject parent = null, bool throwOnError = true)
    {
        var o = CreateObject(obj, parent, throwOnError);
        if (o == null)
        {
            if (throwOnError)
                throw new ArgumentOutOfRangeException(nameof(obj.Type), obj.Type, "Unknown object type");
            return null;
        }

        if (!o.TryGetComponent(out slocObjectData data) || data.ShouldBeSpawnedOnClient)
            NetworkServer.Spawn(o);
        return o;
    }

}
