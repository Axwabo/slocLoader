using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class MoveRelativeToSelfData : BaseTriggerActionData {

        public override TargetType TargetType => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

        public Vector3 offset;

        public MoveRelativeToSelfData(Vector3 offset) => this.offset = offset;

        protected override void WriteData(BinaryWriter writer) => writer.WriteVector(offset);

    }

}
