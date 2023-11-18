namespace slocLoader.TriggerActions.Enums;

[Flags]
public enum TeleportOptions : byte
{

    None = 0,
    ResetFallDamage = 1,
    ResetVelocity = 2,

    [Obsolete("Use WorldSpacePosition instead.")]
    WorldSpaceTransform = 4,

    WorldSpacePosition = 4,
    WorldSpaceRotation = 8,
    All = ResetFallDamage | ResetVelocity | WorldSpacePosition | WorldSpaceRotation

}
