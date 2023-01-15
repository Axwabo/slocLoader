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
            var pose = new Pose(module.Position, module.MouseLook.TargetCamRotation);
            player.TryOverridePosition(pose.position + pose.rotation * data.offset, Vector3.zero);
        }

        protected override void HandleItem(ItemPickupBase pickup, MoveRelativeToSelfData data) {
            var t = pickup.transform;
            pickup.transform.position = t.TransformPoint(data.offset);
        }

        protected override void HandleToy(AdminToyBase toy, MoveRelativeToSelfData data) {
            var t = toy.transform;
            t.position = t.TransformPoint(data.offset);
        }

    }

}
