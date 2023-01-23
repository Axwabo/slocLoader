using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers;
using slocLoader.TriggerActions.Readers;

namespace slocLoader.TriggerActions {

    public static class ActionManager {

        public const ushort MinVersion = 4;

        private static readonly ITriggerActionDataReader DefaultReader = new Ver4ActionDataReader();

        private static readonly Dictionary<ushort, ITriggerActionDataReader> Readers = new() {
            {4, new Ver4ActionDataReader()}
        };

        public static readonly ICollection<TargetType> TargetTypeValues = new List<TargetType> {
            TargetType.Player,
            TargetType.Pickup,
            TargetType.Toy,
            TargetType.Ragdoll
        }.AsReadOnly();

        private static readonly ITriggerActionHandler[] ActionHandlers = {
            new TeleportToPositionHandler(),
            new MoveRelativeToSelfHandler(),
            new KillPlayerHandler(),
            new TeleportToRoomHandler()
        };

        public static bool TryGetReader(ushort version, out ITriggerActionDataReader reader) {
            reader = null;
            if (version < MinVersion)
                return Readers.TryGetValue(version, out reader);
            reader = DefaultReader;
            return true;
        }

        public static ITriggerActionDataReader GetReader(ushort version) => TryGetReader(version, out var reader) ? reader : DefaultReader;

        public static void WriteActions(BinaryWriter writer, ICollection<BaseTriggerActionData> actions) {
            writer.Write(actions.Count);
            foreach (var data in actions)
                data.WriteTo(writer);
        }

        public static BaseTriggerActionData[] ReadActions(BinaryReader stream, ITriggerActionDataReader reader) {
            var actionCount = stream.ReadInt32();
            if (actionCount < 1)
                return Array.Empty<BaseTriggerActionData>();
            var actions = new List<BaseTriggerActionData>();
            for (var i = 0; i < actionCount; i++) {
                var action = reader.Read(stream);
                if (action != null)
                    actions.Add(action);
            }

            var array = actions.ToArray();
            return array;
        }

        public static void ReadTypes(BinaryReader reader, out TriggerActionType actionType, out TargetType targetType) {
            actionType = (TriggerActionType) reader.ReadUInt16();
            targetType = (TargetType) reader.ReadByte();
        }

        public static bool TryGetHandler(TriggerActionType actionType, out ITriggerActionHandler handler) {
            foreach (var actionHandler in ActionHandlers) {
                if (actionHandler.ActionType != actionType)
                    continue;
                handler = actionHandler;
                return true;
            }

            handler = null;
            return false;
        }

        public static bool HasFlagFast(this TargetType targetType, TargetType flag) => (targetType & flag) == flag;

        public static bool Is(this TargetType type, TargetType isType) => type is TargetType.All || type.HasFlagFast(isType);

        public static bool ContainsAnyOf(this TargetType type, TargetType multiple) => type is TargetType.All || TargetTypeValues.Any(targetType => type.HasFlagFast(targetType) && multiple.HasFlagFast(targetType));

    }

}
