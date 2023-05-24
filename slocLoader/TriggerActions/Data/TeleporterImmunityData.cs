using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Data;

public sealed class TeleporterImmunityData : BaseTriggerActionData
{

    public const float MaxValue = 60f;
    public const float FloatToShortMultiplier = 1000f;
    public const float ShortToFloatMultiplier = 1f / FloatToShortMultiplier;

    public override TriggerActionType ActionType => TriggerActionType.TeleporterImmunity;

    public override TargetType PossibleTargets => TargetType.All;

    [field: SerializeField]
    public bool IsGlobal { get; set; }

    [field: SerializeField]
    public ImmunityDurationMode DurationMode { get; set; }

    [field: SerializeField]
    public float Duration { get; set; }

    public TeleporterImmunityData(bool isGlobal, ImmunityDurationMode durationMode, float duration)
    {
        IsGlobal = isGlobal;
        DurationMode = durationMode;
        Duration = duration;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(API.CombineSafe((byte) (IsGlobal ? 1 : 0), (byte) DurationMode));
        writer.WriteFloatAsShort(Duration);
    }

}
