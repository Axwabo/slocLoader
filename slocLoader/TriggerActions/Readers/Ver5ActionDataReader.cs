using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Readers;

public sealed class Ver5ActionDataReader : ITriggerActionDataReader
{

    public BaseTriggerActionData Read(BinaryReader reader)
    {
        ActionManager.ReadTypes(reader, out var actionType, out var targetType, out var eventType);
        BaseTriggerActionData data = actionType switch
        {
            TriggerActionType.TeleportToPosition => ReadTpToPos(reader),
            TriggerActionType.MoveRelativeToSelf => ReadMoveRelative(reader),
            TriggerActionType.TeleportToRoom => ReadTpToRoom(reader),
            TriggerActionType.KillPlayer => Ver4ActionDataReader.ReadKillPlayer(reader),
            TriggerActionType.TeleportToSpawnedObject => ReadTpToSpawnedObject(reader),
            TriggerActionType.TeleporterImmunity => Ver4ActionDataReader.ReadTeleporterImmunity(reader),
            _ => null
        };
        if (data is null)
            return null;
        data.SelectedTargets = targetType;
        data.SelectedEvents = eventType;
        return data;
    }

    public static TeleportToPositionData ReadTpToPos(BinaryReader reader)
    {
        ReadTeleportData(reader, out var position, out var options, out var rotation);
        return new TeleportToPositionData(position) {Options = options, RotationY = rotation};
    }

    public static MoveRelativeToSelfData ReadMoveRelative(BinaryReader reader)
    {
        ReadTeleportData(reader, out var position, out var options, out var rotation);
        return new MoveRelativeToSelfData(position) {Options = options, RotationY = rotation};
    }

    public static TeleportToRoomData ReadTpToRoom(BinaryReader reader)
    {
        ReadTeleportData(reader, out var position, out var options, out var rotation);
        var name = reader.ReadString();
        return new TeleportToRoomData(name, position) {Options = options, RotationY = rotation};
    }

    public static SerializableTeleportToSpawnedObjectData ReadTpToSpawnedObject(BinaryReader reader)
    {
        ReadTeleportData(reader, out var offset, out var options, out var rotation);
        var virtualInstanceId = reader.ReadInt32();
        return new SerializableTeleportToSpawnedObjectData(virtualInstanceId, offset, options, rotation);
    }

    public static void ReadTeleportData(BinaryReader reader, out Vector3 position, out TeleportOptions options, out float rotation)
    {
        position = reader.ReadVector();
        options = (TeleportOptions) reader.ReadByte();
        rotation = reader.ReadSingle();
    }

}
