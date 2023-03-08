using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class TeleportToRoomData : BaseTeleportData {

        public override Enums.TargetType PossibleTargets => Enums.TargetType.All;

        public override Enums.TriggerActionType ActionType => Enums.TriggerActionType.TeleportToRoom;

        [field: SerializeField]
        public string Room { get; set; }

        public TeleportToRoomData(string room, Vector3 offset) {
            Room = room;
            Position = offset;
        }

        protected override void WriteAdditionalData(BinaryWriter writer) => writer.Write(Room);

    }

}
