using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public abstract class UniversalTriggerActionHandler : ITriggerActionHandler {

        public TargetType Targets => TargetType.All;
        
        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (obj.TryGetComponent(out ReferenceHub hub))
                HandlePlayerInternal(hub, data);
            else if (obj.TryGetComponent(out ItemPickupBase pickup))
                HandleItemInternal(pickup, data);
            else if (obj.TryGetComponent(out AdminToyBase toy))
                HandleToyInternal(toy, data);
        }

        private void HandlePlayerInternal(ReferenceHub hub, BaseTriggerActionData data) {
            if (data is null || data.TargetType.Is(TargetType.Player))
                HandlePlayer(hub, data);
        }

        private void HandleItemInternal(ItemPickupBase pickup, BaseTriggerActionData data) {
            if (data is null || data.TargetType.Is(TargetType.Pickup))
                HandleItem(pickup, data);
        }

        private void HandleToyInternal(AdminToyBase toy, BaseTriggerActionData data) {
            if (data is null || data.TargetType.Is(TargetType.Toy))
                HandleToy(toy, data);
        }

        protected abstract void HandlePlayer(ReferenceHub player, BaseTriggerActionData data);

        protected abstract void HandleItem(ItemPickupBase pickup, BaseTriggerActionData data);

        protected abstract void HandleToy(AdminToyBase toy, BaseTriggerActionData data);

    }

}
