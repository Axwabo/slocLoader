using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminToys;
using Axwabo.Helpers;
using Axwabo.Helpers.Pools;
using Mirror;
using PluginAPI.Core;
using slocLoader.Objects;
using slocLoader.Readers;
using slocLoader.TriggerActions;
using UnityEngine;

namespace slocLoader {

    public static class API {

        public const ushort slocVersion = 4;

        public const float ColorDivisionMultiplier = 1f / 255f;

        #region Prefabs

        public static PrimitiveObjectToy PrimitivePrefab { get; private set; }
        public static LightSourceToy LightPrefab { get; private set; }

        internal static void LoadPrefabs() {
            foreach (var prefab in NetworkClient.prefabs.Values) {
                if (prefab.TryGetComponent(out PrimitiveObjectToy primitive))
                    PrimitivePrefab = primitive;
                if (prefab.TryGetComponent(out LightSourceToy light))
                    LightPrefab = light;
            }

            if (PrimitivePrefab == null || LightPrefab == null) {
                Log.Error("Either the primitive or light prefab is null. This should not happen!");
                return;
            }

            InvokeEvent();
        }

        private static void InvokeEvent() {
            if (PrefabsLoaded == null)
                return;
            foreach (var subscriber in PrefabsLoaded.GetInvocationList()) {
                try {
                    subscriber.DynamicInvoke();
                } catch (Exception e) {
                    var method = subscriber.Method;
                    var exception = e is TargetInvocationException {InnerException: var inner} ? inner : e;
                    Log.Error($"An exception was thrown by {method.DeclaringType?.FullName}::{method.Name} upon the invocation of PrefabsLoaded:\n{exception}");
                }
            }
        }

        internal static void UnsetPrefabs() {
            PrimitivePrefab = null;
            LightPrefab = null;
        }

        public static event Action PrefabsLoaded;

        #endregion

        #region Reader Declarations

        public static readonly IObjectReader DefaultReader = new Ver4Reader();

        private static readonly Dictionary<ushort, IObjectReader> VersionReaders = new() {
            {1, new Ver1Reader()},
            {2, new Ver2Reader()},
            {3, new Ver3Reader()},
            {4, new Ver4Reader()}
        };

        public static bool TryGetReader(ushort version, out IObjectReader reader) => VersionReaders.TryGetValue(version, out reader);

        public static IObjectReader GetReader(ushort version) => TryGetReader(version, out var reader) ? reader : DefaultReader;

        #endregion

        #region Read

        // starting from v3, the version is only a ushort instead of a uint
        private static ushort ReadVersionSafe(BufferedStream buffered, BinaryReader binaryReader) {
            var newVersion = binaryReader.ReadUInt16();
            var oldVersion = binaryReader.ReadUInt16();
            if (oldVersion is 0)
                return (ushort) (newVersion | ((uint) oldVersion << 16));
            var newPos = buffered.Get<int>("_readPos") - sizeof(ushort);
            buffered.Set("_readPos", newPos); // rewind the buffer by two bytes, so the whole stream won't be malformed data
            return newVersion;
        }

        public static List<slocGameObject> ReadObjects(Stream stream, bool autoClose = true) {
            var objects = new List<slocGameObject>();
            using var buffered = new BufferedStream(stream, 4);
            var binaryReader = new BinaryReader(buffered);
            var version = ReadVersionSafe(buffered, binaryReader);
            if (!VersionReaders.ContainsKey(version))
                Log.Warning($"Unknown sloc version: {version}\nAttempting to read it using the default reader.");
            var reader = GetReader(version);
            var header = reader.ReadHeader(binaryReader);
            var objectCount = header.ObjectCount;
            for (var i = 0; i < objectCount; i++) {
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

        #region Create

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

        public static GameObject CreateObject(this slocGameObject obj, GameObject parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default, bool throwOnError = true) {
            var transform = obj.Transform;
            return obj switch {
                PrimitiveObject primitive => CreatePrimitive(parent, positionOffset, rotationOffset, primitive, transform),
                LightObject light => CreateLight(parent, positionOffset, rotationOffset, transform, light),
                EmptyObject => CreateEmpty(parent, transform),
                _ => throwOnError ? throw new IndexOutOfRangeException($"Unknown object type {obj.Type}") : null
            };
        }

        private static GameObject CreatePrimitive(GameObject parent, Vector3 positionOffset, Quaternion rotationOffset, PrimitiveObject primitive, slocTransform transform) {
            if (PrimitivePrefab == null)
                throw new InvalidOperationException("Primitive prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
            var toy = UnityEngine.Object.Instantiate(PrimitivePrefab, positionOffset, rotationOffset);
            var colliderMode = primitive.GetNonUnsetColliderMode();
            var primitiveType = primitive.Type.ToPrimitiveType();
            var o = toy.gameObject;
            var sloc = o.AddComponent<slocObjectData>();
            sloc.HasColliderOnClient = colliderMode is PrimitiveObject.ColliderCreationMode.ClientOnly or PrimitiveObject.ColliderCreationMode.Both;
            if (colliderMode is PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger or PrimitiveObject.ColliderCreationMode.ServerOnlyNonSpawned or PrimitiveObject.ColliderCreationMode.NoColliderNonSpawned)
                sloc.ShouldBeSpawnedOnClient = false;
            if (colliderMode is not (PrimitiveObject.ColliderCreationMode.NoCollider or PrimitiveObject.ColliderCreationMode.ClientOnly or PrimitiveObject.ColliderCreationMode.NoColliderNonSpawned))
                o.AddProperCollider(primitiveType, colliderMode.IsTrigger());
            AddActionHandlers(o, primitive);
            toy.PrimitiveType = primitiveType;
            toy.SetAbsoluteTransformFrom(parent);
            toy.SetLocalTransform(transform);
            toy.Scale = AdminToyPatch.GetScale(transform.Scale, sloc.HasColliderOnClient);
            toy.MaterialColor = primitive.MaterialColor;
            return o;
        }

        private static GameObject CreateLight(GameObject parent, Vector3 positionOffset, Quaternion rotationOffset, slocTransform transform, LightObject light) {
            if (LightPrefab == null)
                throw new InvalidOperationException("Light prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
            var toy = UnityEngine.Object.Instantiate(LightPrefab, positionOffset, rotationOffset);
            toy.SetAbsoluteTransformFrom(parent);
            toy.SetLocalTransform(transform);
            toy.LightColor = light.LightColor;
            toy.LightShadows = light.Shadows;
            toy.LightRange = light.Range;
            toy.LightIntensity = light.Intensity;
            toy.Scale = transform.Scale;
            return toy.gameObject;
        }

        private static GameObject CreateEmpty(GameObject parent, slocTransform transform) {
            var emptyObject = new GameObject("Empty");
            emptyObject.SetAbsoluteTransformFrom(parent);
            emptyObject.SetLocalTransform(transform);
            return emptyObject;
        }

        private static void AddActionHandlers(GameObject o, PrimitiveObject primitive) {
            if (primitive.TriggerActions is not {Length: not 0})
                return;
            var list = ListPool<HandlerDataPair>.Shared.Rent();
            foreach (var action in primitive.TriggerActions) {
                if (action.SelectedTargets is not TargetType.None && ActionManager.TryGetHandler(action.ActionType, out var handler))
                    list.Add(new HandlerDataPair(action, handler));
            }

            if (list.Count > 0)
                o.AddComponent<TriggerListener>().ActionHandlers = list.ToArray();
            ListPool<HandlerDataPair>.Shared.Return(list);
        }

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
            var createdInstances = new Dictionary<int, GameObject>();
            foreach (var o in objects) {
                var parent = o.HasParent && createdInstances.TryGetValue(o.ParentId, out var parentInstance) ? parentInstance : go;
                var gameObject = o.CreateObject(parent, throwOnError: false);
                if (gameObject == null)
                    continue;
                createdAmount++;
                createdInstances[o.InstanceId] = gameObject;
            }

            return go;
        }

        public static GameObject CreateObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => CreateObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

        public static GameObject CreateObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default) => CreateObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

        #endregion

        #region Spawn

        public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) => SpawnObjects(objects, out _, position, rotation);

        public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) {
            var go = new GameObject {
                transform = {
                    position = position,
                    rotation = rotation
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocObjectData>();
            NetworkServer.Spawn(go);
            spawnedAmount = 0;
            var createdInstances = new Dictionary<int, GameObject>();
            foreach (var o in objects) {
                var parent = o.HasParent && createdInstances.TryGetValue(o.ParentId, out var parentInstance) ? parentInstance : go;
                var gameObject = o.SpawnObject(parent, throwOnError: false);
                if (gameObject == null)
                    continue;
                spawnedAmount++;
                createdInstances[o.InstanceId] = gameObject;
            }

            return go;
        }

        public static GameObject SpawnObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
            => SpawnObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

        public static GameObject SpawnObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
            => SpawnObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

        public static GameObject SpawnObject(this slocGameObject obj, GameObject parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default, bool throwOnError = true) {
            var o = CreateObject(obj, parent, positionOffset, rotationOffset, throwOnError);
            if (o == null) {
                if (throwOnError)
                    throw new ArgumentOutOfRangeException(nameof(obj.Type), obj.Type, "Unknown object type");
                return null;
            }

            if (!o.TryGetComponent(out slocObjectData data) || data.ShouldBeSpawnedOnClient)
                NetworkServer.Spawn(o);
            return o;
        }

        #endregion

        #region BinaryReader Extensions

        public static slocGameObject ReadObject(this BinaryReader stream, slocHeader header, ushort version = 0, IObjectReader objectReader = null) {
            objectReader ??= GetReader(version);
            return objectReader.Read(stream, header);
        }

        public static slocTransform ReadTransform(this BinaryReader reader) => new() {
            Position = reader.ReadVector(),
            Scale = reader.ReadVector(),
            Rotation = reader.ReadQuaternion()
        };

        public static Vector3 ReadVector(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Quaternion ReadQuaternion(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Color ReadColor(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Color ReadLossyColor(this BinaryReader reader) {
            var color = reader.ReadInt32();
            var red = (color >> 24) & 0xFF;
            var green = (color >> 16) & 0xFF;
            var blue = (color >> 8) & 0xFF;
            var alpha = color & 0xFF;
            return new(red * ColorDivisionMultiplier, green * ColorDivisionMultiplier, blue * ColorDivisionMultiplier, alpha * ColorDivisionMultiplier);
        }

        public static int ReadObjectCount(this BinaryReader reader) {
            var count = reader.ReadInt32();
            return count < 0 ? 0 : count;
        }

        #endregion

        #region BinaryWriter Extensions

        public static void WriteVector(this BinaryWriter writer, Vector3 vector3) {
            writer.Write(vector3.x);
            writer.Write(vector3.y);
            writer.Write(vector3.z);
        }

        public static void WriteQuaternion(this BinaryWriter writer, Quaternion quaternion) {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }

        public static void WriteColor(this BinaryWriter writer, Color color) {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        #endregion

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

        public static void SetLocalTransform(this Component component, slocTransform transform) {
            if (component != null)
                SetLocalTransform(component.gameObject, transform);
        }

        public static void SetAbsoluteTransformFrom(this GameObject o, GameObject parent) {
            if (parent != null)
                o.transform.SetParent(parent.transform, false);
        }

        public static void SetLocalTransform(this GameObject o, slocTransform transform) {
            if (o == null)
                return;
            var t = o.transform;
            t.localScale = transform.Scale;
            t.localPosition = transform.Position;
            t.localRotation = transform.Rotation;
        }

        public static IEnumerable<GameObject> WithAllChildren(this GameObject o) => o.GetComponentsInChildren<Transform>().Select(e => e.gameObject);

        public static bool HasFlagFast(this slocAttributes attributes, slocAttributes flag) => (attributes & flag) == flag;

        public static int ToRgbRange(this float f) => Mathf.FloorToInt(Mathf.Clamp01(f) * 255f);

        public static int ToLossyColor(this Color color) => color.r.ToRgbRange() << 24 | color.g.ToRgbRange() << 16 | color.b.ToRgbRange() << 8 | color.a.ToRgbRange();

        public static Collider AddProperCollider(this GameObject o, PrimitiveType type, bool isTrigger) {
            Collider collider = type switch {
                PrimitiveType.Cube => o.AddComponent<BoxCollider>(),
                PrimitiveType.Sphere => o.AddComponent<SphereCollider>(),
                PrimitiveType.Capsule or PrimitiveType.Cylinder => o.AddComponent<CapsuleCollider>(),
                PrimitiveType.Plane => o.AddComponent<BoxCollider>(),
                PrimitiveType.Quad => o.AddComponent<BoxCollider>(),
                _ => null
            };
            if (collider && isTrigger)
                collider.isTrigger = true;
            return collider;
        }

        public static bool IsTrigger(this PrimitiveObject.ColliderCreationMode colliderMode) => colliderMode is PrimitiveObject.ColliderCreationMode.Trigger or PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger;

        public static bool HasAttribute(this slocHeader header, slocAttributes attribute) => (header.Attributes & attribute) == attribute;

    }

}
