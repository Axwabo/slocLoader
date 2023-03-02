using AdminToys;
using Axwabo.Helpers.Config;
using InventorySystem.Items.Pickups;
using PlayerRoles.FirstPersonControl;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleportToRoomHandler : UniversalTriggerActionHandler<TeleportToRoomData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        protected override void HandlePlayer(ReferenceHub player, TeleportToRoomData data) {
            if (TryCalculatePosition(data, out var pos))
                player.TryOverridePosition(pos, Vector3.zero);
        }

        protected override void HandleItem(ItemPickupBase pickup, TeleportToRoomData data) => HandleComponent(pickup, data);

        protected override void HandleToy(AdminToyBase toy, TeleportToRoomData data) => HandleComponent(toy, data);

        protected override void HandleRagdoll(BasicRagdoll ragdoll, TeleportToRoomData data) => HandleComponent(ragdoll, data);

        private static bool TryCalculatePosition(TeleportToRoomData data, out Vector3 vector) {
            var point = new MapPointByName(data.Room, data.Position);
            if (!data.Options.HasFlagFast(TeleportOptions.WorldSpaceTransform))
                return point.TryGetWorldPose(out vector, out _);
            var transform = point.RoomTransform();
            if (!transform) {
                vector = Vector3.zero;
                return false;
            }

            vector = transform.position + data.Position;
            return true;
        }

        private static void HandleComponent(Component component, TeleportToRoomData data) {
            if (TryCalculatePosition(data, out var pos))
                component.transform.position = pos;
        }

    }

}
