using System.IO;
using slocLoader.TriggerActions.Data;

namespace slocLoader.TriggerActions.Readers {

    public sealed class Ver4ActionDataReader : ITriggerActionDataReader {

        public BaseTriggerActionData Read(BinaryReader reader) {
            ActionManager.ReadTypes(reader, out var actionType, out var targetType);
            return actionType switch {
                TriggerActionType.TeleportToPosition => ReadTpToPos(targetType, reader),
                TriggerActionType.MoveRelativeToSelf => ReadMoveRelative(targetType, reader),
                TriggerActionType.TeleportToRoom => ReadTpToRoom(targetType, reader),
                TriggerActionType.KillPlayer => ReadKillPlayer(reader),
                TriggerActionType.TeleportToSpawnedObject => ReadTpToSpawnedObject(targetType, reader),
                _ => null
            };
        }

        public static TeleportToPositionData ReadTpToPos(TargetType targetType, BinaryReader reader) {
            var position = reader.ReadVector();
            return new TeleportToPositionData(position) {SelectedTargets = targetType};
        }

        public static MoveRelativeToSelfData ReadMoveRelative(TargetType targetType, BinaryReader reader) {
            var position = reader.ReadVector();
            return new MoveRelativeToSelfData(position) {SelectedTargets = targetType};
        }

        public static TeleportToRoomData ReadTpToRoom(TargetType targetType, BinaryReader reader) {
            var position = reader.ReadVector();
            var name = reader.ReadString();
            return new TeleportToRoomData(position, name) {SelectedTargets = targetType};
        }

        public static KillPlayerData ReadKillPlayer(BinaryReader reader) {
            var cause = reader.ReadString();
            return new KillPlayerData(cause);
        }

        public static SerializableTeleportToSpawnedObjectData ReadTpToSpawnedObject(TargetType targetType, BinaryReader reader) {
            var virtualInstanceId = reader.ReadInt32();
            var offset = reader.ReadVector();
            return new SerializableTeleportToSpawnedObjectData(virtualInstanceId, offset) {SelectedTargets = targetType};
        }

    }

}
