using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class UniversalTriggerActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.All;

        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is not TData t)
                return;
            if (obj.TryGetComponent(out ReferenceHub hub))
                HandlePlayerInternal(hub, t);
            else if (obj.TryGetComponent(out ItemPickupBase pickup))
                HandleItemInternal(pickup, t);
            else if (obj.TryGetComponent(out AdminToyBase toy))
                HandleToyInternal(toy, t);
            else if (obj.TryGetComponent(out BasicRagdoll ragdoll))
                HandleRagdollInternal(ragdoll, t);
        }

        private void HandlePlayerInternal(ReferenceHub hub, TData data) {
            if (data.SelectedTargets.Is(TargetType.Player))
                HandlePlayer(hub, data);
        }

        private void HandleItemInternal(ItemPickupBase pickup, TData data) {
            if (data.PossibleTargets.Is(TargetType.Pickup))
                HandleItem(pickup, data);
        }

        private void HandleToyInternal(AdminToyBase toy, TData data) {
            if (data.PossibleTargets.Is(TargetType.Toy))
                HandleToy(toy, data);
        }
        
        private void HandleRagdollInternal(BasicRagdoll ragdoll, TData data) {
            if (data.PossibleTargets.Is(TargetType.Ragdoll))
                HandleRagdoll(ragdoll, data);
        }

        protected abstract void HandlePlayer(ReferenceHub player, TData data);

        protected abstract void HandleItem(ItemPickupBase pickup, TData data);

        protected abstract void HandleToy(AdminToyBase toy, TData data);

        protected abstract void HandleRagdoll(BasicRagdoll ragdoll, TData data);

    }

}
