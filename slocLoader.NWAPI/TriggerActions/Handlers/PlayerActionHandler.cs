using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public abstract class PlayerActionHandler<T> : ITriggerActionHandler where T : BaseTriggerActionData {

        public TargetType Targets => TargetType.Player;

        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is T t && obj.TryGetComponent(out ReferenceHub hub))
                HandlePlayer(hub, t);
        }

        protected abstract void HandlePlayer(ReferenceHub player, T data);

    }

}
