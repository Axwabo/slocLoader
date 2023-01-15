using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.FirstPersonControl;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleportToPositionHandler : UniversalTriggerActionHandler<TeleportToPositionData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        protected override void HandlePlayer(ReferenceHub player, TeleportToPositionData data) =>
            player.TryOverridePosition(data.position, Vector3.zero);

        protected override void HandleItem(ItemPickupBase pickup, TeleportToPositionData data) => pickup.transform.position = data.position;

        protected override void HandleToy(AdminToyBase toy, TeleportToPositionData data) => toy.transform.position = data.position;

    }

}
