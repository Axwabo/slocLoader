using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class PlayerActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.Player;

        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is TData t && obj.TryGetComponent(out ReferenceHub hub))
                HandlePlayer(hub, t);
        }

        protected abstract void HandlePlayer(ReferenceHub player, TData data);

    }

}
