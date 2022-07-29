using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminToys;
using Exiled.API.Features;
using Mirror;
using slocLoader.Objects;
using slocLoader.Readers;
using UnityEngine;

namespace slocLoader {

    public static class API {

        private static PrimitiveObjectToy _primitivePrefab;
        private static LightSourceToy _lightPrefab;

        public static readonly IObjectReader DefaultReader = new Ver2Reader();

        private static readonly Dictionary<int, IObjectReader> VersionReaders = new() {
            {1, new Ver1Reader()},
            {2, new Ver2Reader()}
        };

        public static event Action PrefabsLoaded;

        public static bool TryGetReader(int version, out IObjectReader reader) => VersionReaders.TryGetValue(version, out reader);

        public static IObjectReader GetReader(int version) => TryGetReader(version, out var reader) ? reader : DefaultReader;

        public static List<slocGameObject> ReadObjects(Stream stream, bool autoClose = true) {
            var objects = new List<slocGameObject>();
            var binaryReader = new BinaryReader(stream);
            var version = binaryReader.ReadInt32();
            var reader = GetReader(version);
            var count = binaryReader.ReadInt32();
            for (var i = 0; i < count; i++) {
                var obj = ReadObject(binaryReader, version, reader);
                if (obj is {IsValid: true})
                    objects.Add(obj);
            }

            if (autoClose)
                binaryReader.Close();
            return objects;
        }

        public static List<slocGameObject> ReadObjectsFromFile(string path) => ReadObjects(File.OpenRead(path));

        public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) => SpawnObjects(objects, out _, position, rotation);

        public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) {
            var go = new GameObject {
                transform = {
                    position = position,
                    rotation = rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocSpawnedObject>();
            spawnedAmount = 0;
            var createdInstances = new Dictionary<int, GameObject>();
            foreach (var o in objects) {
                var parent = o.HasParent && createdInstances.TryGetValue(o.ParentId, out var parentInstance) ? parentInstance : go;
                var gameObject = o.SpawnObject(parent, throwOnError: false);
                Log.Debug(o.Type + " -> " + parent.name + " | " + gameObject.transform.lossyScale);
                if (gameObject == null)
                    continue;
                spawnedAmount++;
                createdInstances[o.InstanceId] = gameObject;
            }

            /*foreach (var entry in createdInstances.ToList()) {
                var pair = entry.Value;
                var sloc = pair.Key;
                var o = pair.Value;
                var parent = sloc.HasParent && createdInstances.TryGetValue(sloc.ParentId, out var parentInstance) ? parentInstance.Value : go;
                o.SetAbsoluteTransformFrom(parent);
                Log.Debug(sloc.Type + " " + sloc.Transform.Scale + " " + parent.transform.lossyScale + " " + parent.transform.localScale);
                // o.transform.localScale = Divide(sloc.Transform.Scale, parent.transform.localScale);
            }

            foreach (var entry in createdInstances.ToList()) {
                var pair = entry.Value;
                var sloc = pair.Key;
                var o = pair.Value;
                var parent = sloc.HasParent && createdInstances.TryGetValue(sloc.ParentId, out var parentInstance) ? parentInstance.Value : go;
                o.SetAbsoluteTransformFrom(parent);
                Log.Debug(sloc.Type + " " + sloc.Transform.Scale + " " + parent.transform.lossyScale + " " + parent.transform.localScale);
                // o.transform.localScale = Divide(sloc.Transform.Scale, parent.transform.localScale);
            }*/

            NetworkServer.Spawn(go);
            return go;
        }

        public static GameObject SpawnObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => SpawnObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

        public static GameObject SpawnObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => SpawnObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

        public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) => CreateObjects(objects, out _, position, rotation);

        public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, out int createdAmount, Vector3 position, Quaternion rotation = default) {
            var go = new GameObject {
                transform = {
                    position = position,
                    rotation = rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            createdAmount = 0;
            var createdInstances = new Dictionary<int, KeyValuePair<slocGameObject, GameObject>>();
            foreach (var o in objects) {
                var gameObject = o.CreateObject(null, throwOnError: false);
                if (gameObject == null)
                    continue;
                createdAmount++;
                createdInstances[o.InstanceId] = new(o, gameObject);
            }

            foreach (var entry in createdInstances.ToList()) {
                var pair = entry.Value;
                var sloc = pair.Key;
                var o = pair.Value;
                var parent = sloc.HasParent && createdInstances.TryGetValue(sloc.ParentId, out var parentInstance) ? parentInstance.Value : go;
                o.SetAbsoluteTransformFrom(parent);
                Log.Debug(sloc.Transform.Scale + " " + parent.transform.lossyScale + " " + parent.transform.localScale);
                o.transform.localScale = Divide(sloc.Transform.Scale, parent.transform.localScale);
            }

            return go;
        }

        public static GameObject CreateObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => CreateObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

        public static GameObject CreateObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => CreateObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

        public static GameObject SpawnObject(this slocGameObject obj, GameObject parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default, bool throwOnError = true) {
            var o = CreateObject(obj, parent, positionOffset, rotationOffset, throwOnError);
            if (o == null) {
                if (throwOnError)
                    throw new ArgumentOutOfRangeException(nameof(obj.Type), obj.Type, "Unknown object type");
                return null;
            }

            NetworkServer.Spawn(o.gameObject);
            return o;
        }

        public static GameObject CreateObject(this slocGameObject obj, GameObject parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default, bool throwOnError = true) {
            switch (obj) {
                case PrimitiveObject primitive: {
                    if (_primitivePrefab == null)
                        throw new NullReferenceException("Primitive prefab is not set! Make sure to spawn objects after the map is generated.");
                    var toy = UnityEngine.Object.Instantiate(_primitivePrefab, positionOffset, rotationOffset);
                    toy.SetAbsoluteTransformFrom(parent);
                    toy.SetLocalTransform(obj.Transform, out var scale);
                    toy.NetworkScale = scale;
                    toy.NetworkPrimitiveType = primitive.Type.ToPrimitiveType();
                    toy.MaterialColor = primitive.MaterialColor;
                    var o = toy.gameObject;
                    o.AddComponent<UseLossyScale>();
                    return o;
                }
                case LightObject light: {
                    if (_primitivePrefab == null)
                        throw new NullReferenceException("Light prefab is not set! Make sure to spawn objects after the map is generated.");
                    var toy = UnityEngine.Object.Instantiate(_lightPrefab, positionOffset, rotationOffset);
                    toy.SetAbsoluteTransformFrom(parent);
                    toy.SetLocalTransform(obj.Transform, out var scale);
                    toy.NetworkLightColor = light.LightColor;
                    toy.NetworkLightShadows = light.Shadows;
                    toy.NetworkLightRange = light.Range;
                    toy.NetworkLightIntensity = light.Intensity;
                    toy.NetworkScale = scale;
                    return toy.gameObject;
                }
                case EmptyObject _: {
                    var emptyObject = new GameObject("Empty");
                    emptyObject.AddComponent<NetworkIdentity>();
                    emptyObject.SetAbsoluteTransformFrom(parent);
                    emptyObject.SetLocalTransform(obj.Transform, out _);
                    return emptyObject;
                }
                default:
                    if (throwOnError)
                        throw new ArgumentOutOfRangeException(nameof(obj.Type), obj.Type, "Unknown object type");
                    return null;
            }
        }

        public static slocGameObject CreateDefaultObject(this ObjectType type) => type switch {
            ObjectType.Cube
                or ObjectType.Sphere
                or ObjectType.Cylinder
                or ObjectType.Plane
                or ObjectType.Capsule
                or ObjectType.Quad => new PrimitiveObject(0, type),
            ObjectType.Light => new LightObject(0),
            ObjectType.Empty => new EmptyObject(0),
            _ => null
        };

        public static slocGameObject ReadObject(this BinaryReader stream, int version = 0, IObjectReader objectReader = null) {
            objectReader ??= GetReader(version);
            return objectReader.Read(stream);
        }

        public static slocTransform ReadTransform(this BinaryReader reader) => new() {
            Position = reader.ReadVector(),
            Scale = reader.ReadVector(),
            Rotation = reader.ReadQuaternion()
        };

        public static Vector3 ReadVector(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Quaternion ReadQuaternion(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Color ReadColor(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static PrimitiveType ToPrimitiveType(this ObjectType type) => type switch {
            ObjectType.Cube => PrimitiveType.Cube,
            ObjectType.Sphere => PrimitiveType.Sphere,
            ObjectType.Capsule => PrimitiveType.Capsule,
            ObjectType.Cylinder => PrimitiveType.Cylinder,
            ObjectType.Plane => PrimitiveType.Plane,
            ObjectType.Quad => PrimitiveType.Quad,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A non-primitive type was supplied")
        };

        public static void SetAbsoluteTransformFrom(this Component component, GameObject parent) {
            if (component != null)
                SetAbsoluteTransformFrom(component.gameObject, parent);
        }

        public static void SetLocalTransform(this Component component, slocTransform transform, out Vector3 scale) {
            if (component != null)
                SetLocalTransform(component.gameObject, transform, out scale);
            else
                scale = Vector3.one;
        }

        public static void SetAbsoluteTransformFrom(this GameObject o, GameObject parent) {
            if (parent != null)
                o.transform.SetParent(parent.transform, false);
        }

        public static void SetLocalTransform(this GameObject o, slocTransform transform, out Vector3 scale) {
            if (o == null) {
                scale = Vector3.one;
                return;
            }

            var t = o.transform;
            var parent = t.parent;
            // t.SetParent(null, false);
            t.localScale = transform.Scale; //Divide(transform.Scale, parent == null ? Vector3.one : parent.transform.lossyScale);
            // t.parent = parent;
            t.localPosition = transform.Position;
            t.localRotation = transform.Rotation;
            scale = t.localScale;
        }

        public static Vector3 Divide(Vector3 a, Vector3 b) {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        internal static void LoadPrefabs() {
            foreach (var prefab in NetworkClient.prefabs.Values) {
                if (prefab.TryGetComponent(out PrimitiveObjectToy primitive))
                    _primitivePrefab = primitive;
                if (prefab.TryGetComponent(out LightSourceToy light))
                    _lightPrefab = light;
            }

            if (_primitivePrefab == null || _lightPrefab == null)
                return;
            try {
                PrefabsLoaded?.Invoke();
            } catch (Exception e) {
                Log.Error(e);
                Log.Debug($"Methods that may cause this issue:\n{string.Join("\n", PrefabsLoaded?.GetInvocationList().Select(x => $"{x.Method.DeclaringType?.FullName}::{x.Method.Name}") ?? Enumerable.Empty<string>())}");
            }
        }

        internal static void UnsetPrefabs() {
            _primitivePrefab = null;
            _lightPrefab = null;
        }

        public static bool ShouldUseLossyScale(this AdminToyBase toy) => toy.TryGetComponent(out UseLossyScale _);

    }

}
