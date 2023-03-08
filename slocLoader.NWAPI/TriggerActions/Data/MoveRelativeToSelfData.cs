using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class MoveRelativeToSelfData : BaseTeleportData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

        public MoveRelativeToSelfData(Vector3 offset) => Position = offset;

    }

}
