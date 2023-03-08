using System.IO;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class TeleportToRoomData : BaseTeleportData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

        [field: SerializeField]
        public string Room { get; set; }

        public TeleportToRoomData(string room, Vector3 offset) {
            Room = room;
            Position = offset;
        }

        protected override void WriteAdditionalData(BinaryWriter writer) => writer.Write(Room);

    }

}
