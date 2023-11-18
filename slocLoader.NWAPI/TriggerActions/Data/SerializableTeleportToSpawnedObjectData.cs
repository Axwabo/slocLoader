using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Data;

public sealed class SerializableTeleportToSpawnedObjectData : BaseTriggerActionData
{

    public override TargetType PossibleTargets => TargetType.All;

    public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

    public readonly int ID;

    public readonly Vector3 Offset;

    public readonly float RotationY;

    public readonly TeleportOptions Options;

    public SerializableTeleportToSpawnedObjectData(int id, Vector3 offset, TeleportOptions options) : this(id, offset, options, 0)
    {
    }

    public SerializableTeleportToSpawnedObjectData(int id, Vector3 offset, TeleportOptions options, float rotationY)
    {
        ID = id;
        Offset = offset;
        Options = options;
        RotationY = rotationY;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.WriteVector(Offset);
        writer.Write((byte) Options);
        writer.Write(RotationY);
    }

}
