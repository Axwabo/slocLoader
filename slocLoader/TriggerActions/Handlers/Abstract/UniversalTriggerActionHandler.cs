using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class UniversalTriggerActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.All;

        public abstract TriggerActionType ActionType { get; }

        protected virtual bool ValidateData(GameObject interactingObject, TData data, TriggerListener listener) => true;

        public void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener) {
            if (data is not TData t || !ValidateData(interactingObject, t, listener))
                return;
            if (interactingObject.TryGetComponent(out ReferenceHub hub))
                HandlePlayerInternal(hub, t, listener);
            else if (interactingObject.TryGetComponent(out ItemPickupBase pickup))
                HandleItemInternal(pickup, t, listener);
            else if (interactingObject.TryGetComponent(out AdminToyBase toy))
                HandleToyInternal(toy, t, listener);
            else if (interactingObject.TryGetComponent(out BasicRagdoll ragdoll))
                HandleRagdollInternal(ragdoll, t, listener);
        }

        private void HandlePlayerInternal(ReferenceHub hub, TData data, TriggerListener listener) {
            if (data.SelectedTargets.Is(TargetType.Player))
                HandlePlayer(hub, data, listener);
        }

        private void HandleItemInternal(ItemPickupBase pickup, TData data, TriggerListener listener) {
            if (data.PossibleTargets.Is(TargetType.Pickup))
                HandleItem(pickup, data, listener);
        }

        private void HandleToyInternal(AdminToyBase toy, TData data, TriggerListener listener) {
            if (data.PossibleTargets.Is(TargetType.Toy))
                HandleToy(toy, data, listener);
        }

        private void HandleRagdollInternal(BasicRagdoll ragdoll, TData data, TriggerListener listener) {
            if (data.PossibleTargets.Is(TargetType.Ragdoll))
                HandleRagdoll(ragdoll, data, listener);
        }

        protected abstract void HandlePlayer(ReferenceHub player, TData data, TriggerListener listener);

        protected abstract void HandleItem(ItemPickupBase pickup, TData data, TriggerListener listener);

        protected abstract void HandleToy(AdminToyBase toy, TData data, TriggerListener listener);

        protected abstract void HandleRagdoll(BasicRagdoll ragdoll, TData data, TriggerListener listener);

    }

}
