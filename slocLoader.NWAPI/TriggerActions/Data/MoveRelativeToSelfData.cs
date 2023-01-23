using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class MoveRelativeToSelfData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

        [field: SerializeField]
        public Vector3 Offset { get; set; }

        public MoveRelativeToSelfData(Vector3 offset) => Offset = offset;

        protected override void WriteData(BinaryWriter writer) => writer.WriteVector(Offset);

    }

}
