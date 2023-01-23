using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class TeleportToPositionData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        [field: SerializeField]
        public Vector3 Position { get; set; }

        public TeleportToPositionData(Vector3 position) => Position = position;

        protected override void WriteData(BinaryWriter writer) => writer.WriteVector(Position);

    }

}
