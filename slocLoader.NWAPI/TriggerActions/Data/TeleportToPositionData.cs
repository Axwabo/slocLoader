using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class TeleportToPositionData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

        public Vector3 position;

        public TeleportToPositionData(Vector3 position) => this.position = position;

        protected override void WriteData(BinaryWriter writer) => writer.WriteVector(position);

    }

}
