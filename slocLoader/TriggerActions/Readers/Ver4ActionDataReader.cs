using System.IO;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Readers {

    public sealed class Ver4ActionDataReader : ITriggerActionDataReader {

        public BaseTriggerActionData Read(BinaryReader reader) {
            ActionManager.ReadTypes(reader, out var actionType, out var targetType, out var eventType);
            BaseTriggerActionData data = actionType switch {
                TriggerActionType.TeleportToPosition => ReadTpToPos(reader),
                TriggerActionType.MoveRelativeToSelf => ReadMoveRelative(reader),
                TriggerActionType.TeleportToRoom => ReadTpToRoom(reader),
                TriggerActionType.KillPlayer => ReadKillPlayer(reader),
                TriggerActionType.TeleportToSpawnedObject => ReadTpToSpawnedObject(reader),
                TriggerActionType.TeleporterImmunity => ReadTeleporterImmunity(reader),
                _ => null
            };
            if (data is null)
                return null;
            data.SelectedTargets = targetType;
            data.SelectedEvents = eventType;
            return data;
        }

        public static TeleportToPositionData ReadTpToPos(BinaryReader reader) {
            ReadTeleportData(reader, out var position, out var options);
            return new TeleportToPositionData(position) {Options = options};
        }

        public static MoveRelativeToSelfData ReadMoveRelative(BinaryReader reader) {
            ReadTeleportData(reader, out var position, out var options);
            return new MoveRelativeToSelfData(position) {Options = options};
        }

        public static TeleportToRoomData ReadTpToRoom(BinaryReader reader) {
            ReadTeleportData(reader, out var position, out var options);
            var name = reader.ReadString();
            return new TeleportToRoomData(name, position) {Options = options};
        }

        public static KillPlayerData ReadKillPlayer(BinaryReader reader) {
            var cause = reader.ReadString();
            return new KillPlayerData(cause);
        }

        public static SerializableTeleportToSpawnedObjectData ReadTpToSpawnedObject(BinaryReader reader) {
            ReadTeleportData(reader, out var offset, out var options);
            var virtualInstanceId = reader.ReadInt32();
            return new SerializableTeleportToSpawnedObjectData(virtualInstanceId, offset, options);
        }

        public static TeleporterImmunityData ReadTeleporterImmunity(BinaryReader reader) {
            var firstByte = reader.ReadByte();
            API.SplitSafe(firstByte, out var isGlobal, out var options);
            var duration = reader.ReadShortAsFloat();
            return new TeleporterImmunityData(isGlobal != 0, (ImmunityDurationMode) options, duration);
        }

        public static void ReadTeleportData(BinaryReader reader, out Vector3 position, out TeleportOptions options) {
            position = reader.ReadVector();
            options = (TeleportOptions) reader.ReadByte();
        }

    }

}
