using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class TeleportToRoomData : BaseTriggerActionData {

        public override TargetType TargetType => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

        public Vector3 positionOffset;

        public string room;

        public TeleportToRoomData(Vector3 positionOffset, string room) {
            this.positionOffset = positionOffset;
            this.room = room;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.WriteVector(positionOffset);
            writer.Write(room);
        }

    }

}
