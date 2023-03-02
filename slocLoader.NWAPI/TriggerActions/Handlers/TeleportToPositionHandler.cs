using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleportToPositionHandler : UniversalTriggerActionHandler<TeleportToPositionData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        protected override void HandlePlayer(ReferenceHub player, TeleportToPositionData data) =>
            player.OverridePosition(data.Position, data.Options);

        protected override void HandleItem(ItemPickupBase pickup, TeleportToPositionData data) => HandleComponent(pickup, data);

        protected override void HandleToy(AdminToyBase toy, TeleportToPositionData data) => HandleComponent(toy, data);

        protected override void HandleRagdoll(BasicRagdoll ragdoll, TeleportToPositionData data) => HandleComponent(ragdoll, data);

        private static void HandleComponent(Component component, TeleportToPositionData data) => component.transform.position = data.Position;

    }

}
