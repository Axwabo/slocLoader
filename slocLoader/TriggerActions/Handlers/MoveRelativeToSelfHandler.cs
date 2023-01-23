using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.FirstPersonControl;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class MoveRelativeToSelfHandler : UniversalTriggerActionHandler<MoveRelativeToSelfData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        protected override void HandlePlayer(ReferenceHub player, MoveRelativeToSelfData data) {
            if (player.roleManager.CurrentRole is not IFpcRole fpc)
                return;
            var module = fpc.FpcModule;
            player.TryOverridePosition(module.Position + module.MouseLook.TargetCamRotation * data.Offset, Vector3.zero);
        }

        protected override void HandleItem(ItemPickupBase pickup, MoveRelativeToSelfData data) => HandleComponent(pickup, data);

        protected override void HandleToy(AdminToyBase toy, MoveRelativeToSelfData data) => HandleComponent(toy, data);
        
        protected override void HandleRagdoll(BasicRagdoll ragdoll, MoveRelativeToSelfData data) => HandleComponent(ragdoll, data);

        private static void HandleComponent(Component component, MoveRelativeToSelfData data) {
            var t = component.transform;
            t.position = t.TransformPoint(data.Offset);
        }

    }

}
