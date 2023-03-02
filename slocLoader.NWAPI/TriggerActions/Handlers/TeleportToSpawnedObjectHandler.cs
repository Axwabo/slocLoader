using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleportToSpawnedObjectHandler : UniversalTriggerActionHandler<RuntimeTeleportToSpawnedObjectData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        protected override bool ValidateData(RuntimeTeleportToSpawnedObjectData data) => data.Target != null;

        protected override void HandlePlayer(ReferenceHub player, RuntimeTeleportToSpawnedObjectData data) =>
            player.OverridePosition(data.ToWorldSpacePosition(data.Target.transform), data.Options);

        protected override void HandleItem(ItemPickupBase pickup, RuntimeTeleportToSpawnedObjectData data) => HandleComponent(pickup, data);

        protected override void HandleToy(AdminToyBase toy, RuntimeTeleportToSpawnedObjectData data) => HandleComponent(toy, data);

        protected override void HandleRagdoll(BasicRagdoll ragdoll, RuntimeTeleportToSpawnedObjectData data) => HandleComponent(ragdoll, data);

        private static void HandleComponent(Component component, RuntimeTeleportToSpawnedObjectData data) =>
            component.transform.position = data.ToWorldSpacePosition(data.Target.transform);

    }

}
