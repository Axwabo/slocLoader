using System;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using Axwabo.Helpers;
using Axwabo.Helpers.Pools;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using MapGeneration;
using MEC;
using Mirror;
using slocLoader.AutoObjectLoader;
using UnityEngine;
using Version = System.Version;

namespace slocLoader {

    public class slocPlugin : Plugin<Config> {

        public override string Name => "slocLoader";
        public override string Prefix => "sloc";
        public override string Author => "Axwabo";
        public override Version Version { get; } = new(2, 0, 0);
        public override Version RequiredExiledVersion { get; } = new(5, 2, 0);

        private Harmony _harmony;
        private CoroutineHandle _unscaledUpdate;

        private static readonly List<PrimitiveObjectToy> NoColliders = new();

        internal static readonly Dictionary<NetworkConnection, List<PrimitiveObjectToy>> UnscaledObjects = new();

        public override void OnEnabled() {
            _harmony = new Harmony("Axwabo.slocLoader");
            try {
                _harmony.PatchAll();
                Log.Info("Patching succeeded.");
            } catch (Exception e) {
                Log.Error("Patching failed! Nested object scaling will behave weirdly!\n" + e);
            }

            API.UnsetPrefabs();
            if (Config.AutoLoad)
                API.PrefabsLoaded += AutomaticObjectLoader.LoadObjects;
            if (SeedSynchronizer.MapGenerated) {
                API.LoadPrefabs();
                SpawnDefault();
            }

            Exiled.Events.Handlers.Map.Generated += MapGenerated;
            Exiled.Events.Handlers.Player.Verified += Verified;
            API.PrefabsLoaded += SpawnDefault;
            _unscaledUpdate = Timing.RunCoroutine(UpdateUnscaled(), Segment.LateUpdate);
            base.OnEnabled();
        }

        public override void OnDisabled() {
            _harmony.UnpatchAll();
            Timing.KillCoroutines(_unscaledUpdate);
            API.UnsetPrefabs();
            Exiled.Events.Handlers.Map.Generated -= MapGenerated;
            Exiled.Events.Handlers.Player.Verified -= Verified;
            API.PrefabsLoaded -= AutomaticObjectLoader.LoadObjects;
            API.PrefabsLoaded -= SpawnDefault;
            base.OnDisabled();
        }

        private static IEnumerator<float> UpdateUnscaled() {
            while (!Round.IsEnded) {
                yield return Timing.WaitForOneFrame;
                var list = UnscaledObjects.ToList();
                foreach (var pair in list) {
                    var connection = pair.Key;
                    UnscaledObjects.Remove(connection);
                    if (!connection.isReady)
                        continue;
                    foreach (var toy in pair.Value) {
                        typeof(NetworkBehaviour).SetProp(toy, "syncVarDirtyBits", 4UL | 16UL);
                        // connection.ReSyncSyncVar(typeof(AdminToyBase), toy, "NetworkScale");
                    }
                }
            }
        }

        private static void MapGenerated() {
            NoColliders.Clear();
            AdminToyPatch.DesiredScale.Clear();
            API.LoadPrefabs();
        }

        private void SpawnDefault() {
            if (Config.EnableAutoSpawn)
                AutomaticObjectLoader.SpawnObjects(Config.AutoSpawnByRoomName.Cast<IAssetLocation>().Concat(Config.AutoSpawnByRoomType.Cast<IAssetLocation>()).Concat(Config.AutoSpawnByLocation.Cast<IAssetLocation>()));
        }

        private static void Verified(VerifiedEventArgs ev) {
            // Timing.RunCoroutine(SetProperColliders(ev.Player, ev.Player.ReferenceHub.networkIdentity.connectionToClient));
        }

        public static IEnumerator<float> SetProperColliders(Player evPlayer, NetworkConnectionToClient connection) {
            var list = ListPool<PrimitiveObjectToy>.Shared.Rent(NoColliders);
            yield return Timing.WaitForSeconds(2);
            evPlayer.SetRole(RoleType.NtfCaptain);
            yield return Timing.WaitForSeconds(1);
            yield return Timing.WaitForSeconds(1);
            foreach (var toy in list) {
                var type = toy.PrimitiveType;
                var owner = NetworkWriterPool.GetWriter();
                var observer = NetworkWriterPool.GetWriter();
                var behaviorIndex = (byte) toy.netIdentity.NetworkBehaviours.IndexOf(toy);
                owner.WriteByte(behaviorIndex);
                var start = owner.Position;
                owner.WriteInt32(0);
                var secondZero = owner.Position;
                toy.SerializeObjectsDelta(owner);
                CustomSyncVarGenerator(owner);
                var serializedPosition = owner.Position;
                owner.Position = start;
                owner.WriteInt32(serializedPosition - secondZero);
                owner.Position = serializedPosition;
                if (toy.syncMode == SyncMode.Observers)
                    goto send;
                var segment = owner.ToArraySegment();
                observer.WriteBytes(segment.Array, start, owner.Position - start);
                send:
                connection.Send(new UpdateVarsMessage {
                    netId = toy.netId,
                    payload = owner.ToArraySegment()
                });
                NetworkWriterPool.Recycle(owner);
                NetworkWriterPool.Recycle(observer);

                void CustomSyncVarGenerator(NetworkWriter targetWriter) {
                    targetWriter.WriteUInt64(16);
                    targetWriter.WriteInt32((int) (type == PrimitiveType.Sphere ? PrimitiveType.Cube : type));
                }

                // connection.SendFakeSyncVarGeneric<PrimitiveObjectToy, PrimitiveType>(toy, "NetworkPrimitiveType", type == PrimitiveType.Sphere ? PrimitiveType.Cube : type);
                Log.Debug("primitve type changed");
            }

            yield return Timing.WaitForSeconds(1);
            foreach (var toy in list) {
                connection.SendFakeSyncVarGeneric<AdminToyBase, Vector3>(toy, "NetworkScale", Vector3.zero);
                Log.Debug("scale set to zero");
            }

            yield return Timing.WaitForSeconds(1);
            foreach (var toy in list) {
                connection.ReSyncSyncVar(toy, "NetworkPrimitiveType");
                Log.Debug("primitve type changed back");
            }

            yield return Timing.WaitForSeconds(1);
            foreach (var toy in list) {
                connection.ReSyncSyncVar(typeof(AdminToyBase), toy, "NetworkScale");
                Log.Debug("scale set back to normal");
            }

            ListPool<PrimitiveObjectToy>.Shared.Return(list);
            Log.Debug("re-synced all colliders");
        }

        public static void SetDesiredScale(bool clientSideCollider, PrimitiveObjectToy toy, Vector3 scale) {
            if (clientSideCollider)
                return;
            AdminToyPatch.DesiredScale[toy.GetInstanceID()] = scale;
            if (ShouldCreateColliderOnClient(toy))
                NoColliders.Add(toy);
        }

        public static bool ShouldCreateColliderOnClient(PrimitiveObjectToy toy) => !NoColliders.Contains(toy);

    }

}
