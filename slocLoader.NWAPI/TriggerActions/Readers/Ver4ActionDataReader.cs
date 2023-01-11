using System.IO;
using slocLoader.TriggerActions.Data;

namespace slocLoader.TriggerActions.Readers {

    public sealed class Ver4ActionDataReader : ITriggerActionDataReader {

        public BaseTriggerActionData Read(BinaryReader reader) {
            ActionManager.ReadTypes(reader, out var targetType, out var actionType);
            return actionType switch {
                TriggerActionType.TeleportToPosition => ReadTpToPos(reader),
                TriggerActionType.MoveRelativeToSelf => ReadMoveRelative(reader),
                TriggerActionType.TeleportToRoom => ReadTpToRoom(reader),
                TriggerActionType.KillPlayer => ReadKillPlayer(targetType, reader),
                TriggerActionType.TeleportToSpawnedObject => ReadTpToSpawnedObject(reader),
                _ => null
            };
        }

        public static TeleportToPositionData ReadTpToPos(BinaryReader reader) {
            var position = reader.ReadVector();
            return new TeleportToPositionData(position);
        }

        public static MoveRelativeToSelfData ReadMoveRelative(BinaryReader reader) {
            var position = reader.ReadVector();
            return new MoveRelativeToSelfData(position);
        }

        public static TeleportToRoomData ReadTpToRoom(BinaryReader reader) {
            var position = reader.ReadVector();
            var name = reader.ReadString();
            return new TeleportToRoomData(position, name);
        }

        public static KillPlayerData ReadKillPlayer(TargetType type, BinaryReader reader) {
            var cause = reader.ReadString();
            return type.Is(TargetType.Player) ? new KillPlayerData(cause) : null;
        }

        public static TeleportToSpawnedObjectData ReadTpToSpawnedObject(BinaryReader reader) {
            var virtualInstanceId = reader.ReadInt32();
            var offset = reader.ReadVector();
            return new TeleportToSpawnedObjectData(virtualInstanceId, offset);
        }

    }

}
