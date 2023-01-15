using AdminToys;
using Axwabo.Helpers.Config;
using InventorySystem.Items.Pickups;
using PlayerRoles.FirstPersonControl;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleportToRoomHandler : UniversalTriggerActionHandler<TeleportToRoomData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        protected override void HandlePlayer(ReferenceHub player, TeleportToRoomData data) {
            if (TryCalculatePosition(data, out var pos))
                player.TryOverridePosition(pos, Vector3.zero);
        }

        protected override void HandleItem(ItemPickupBase pickup, TeleportToRoomData data) {
            if (TryCalculatePosition(data, out var pos))
                pickup.transform.position = pos;
        }

        protected override void HandleToy(AdminToyBase toy, TeleportToRoomData data) {
            if (TryCalculatePosition(data, out var pos))
                toy.transform.position = pos;
        }

        private static bool TryCalculatePosition(TeleportToRoomData data, out Vector3 vector) => new MapPointByName(data.room, data.positionOffset).TryGetWorldPose(out vector, out _);

    }

}
