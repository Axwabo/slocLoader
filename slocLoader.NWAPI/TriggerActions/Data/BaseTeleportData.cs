﻿using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Data;

public abstract class BaseTeleportData : BaseTriggerActionData
{

    [field: SerializeField]
    public Vector3 Position { get; set; }

    [field: SerializeField]
    public TeleportOptions Options { get; set; } = TeleportOptions.ResetFallDamage | TeleportOptions.DeltaRotation;

    [field: SerializeField]
    public float RotationY { get; set; }

    protected sealed override void WriteData(BinaryWriter writer)
    {
        writer.WriteVector(Position);
        writer.Write((byte) Options);
        writer.Write(RotationY);
        WriteAdditionalData(writer);
    }

    protected virtual void WriteAdditionalData(BinaryWriter writer)
    {
    }

    public Vector3 ToWorldSpacePosition(Transform reference) =>
        Options.HasFlag(TeleportOptions.WorldSpaceTransform)
            ? reference.position + Position
            : reference.TransformPoint(Position);

    public void ToWorldSpace(Transform reference, out Vector3 position, out float rotation)
    {
        position = ToWorldSpacePosition(reference);
        rotation = Options.HasFlag(TeleportOptions.WorldSpaceTransform)
            ? RotationY
            : reference.rotation.eulerAngles.y + RotationY;
    }

}
