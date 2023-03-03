using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public interface ITriggerActionHandler {

        TargetType Targets { get; }

        TriggerActionType ActionType { get; }

        void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener);

    }

}
