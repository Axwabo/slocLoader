using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class TeleportToPositionData : BaseTeleportData {

        public override Enums.TargetType PossibleTargets => Enums.TargetType.All;

        public override Enums.TriggerActionType ActionType => Enums.TriggerActionType.TeleportToPosition;

        public TeleportToPositionData(Vector3 position) => Position = position;

    }

}
