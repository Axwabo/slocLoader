using System;
using System.IO;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class KillPlayerData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.Player;

        public override TriggerActionType ActionType => TriggerActionType.KillPlayer;

        public string cause;

        public KillPlayerData(string cause) => this.cause = cause;

        protected override void WriteData(BinaryWriter writer) => writer.Write(cause);

    }

}
