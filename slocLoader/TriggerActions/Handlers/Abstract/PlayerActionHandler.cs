using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class PlayerActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.Player;

        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener) {
            if (data is TData t && interactingObject.TryGetComponent(out ReferenceHub hub))
                HandlePlayer(hub, t, listener);
        }

        protected abstract void HandlePlayer(ReferenceHub player, TData data, TriggerListener listener);

    }

}
