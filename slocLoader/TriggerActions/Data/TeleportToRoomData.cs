using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class TeleportToRoomData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

        [field: SerializeField]
        public Vector3 PositionOffset { get; set; }

        [field: SerializeField]
        public string Room { get; set; }

        public TeleportToRoomData(Vector3 positionOffset, string room) {
            PositionOffset = positionOffset;
            Room = room;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.WriteVector(PositionOffset);
            writer.Write(Room);
        }

    }

}
