using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Data;

[Serializable]
public abstract class BaseTriggerActionData
{

    [SerializeField]
    private TargetType selectedTargets;

    public abstract TargetType PossibleTargets { get; }

    public abstract TriggerActionType ActionType { get; }

    public TargetType SelectedTargets
    {
        get => selectedTargets;
        set
        {
            if (value is TargetType.None)
            {
                selectedTargets = value;
                return;
            }

            var possible = PossibleTargets;
            foreach (var v in ActionManager.TargetTypeValues)
                if (value.HasFlagFast(v) && possible.HasFlagFast(v))
                    selectedTargets |= v;
                else
                    selectedTargets &= ~v;
        }
    }

    [field: SerializeField]
    public TriggerEventType SelectedEvents { get; set; } = TriggerEventType.All;

    protected BaseTriggerActionData() => SelectedTargets = TargetType.All;

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((ushort) ActionType);
        writer.Write(API.CombineSafe((byte) SelectedTargets, (byte) SelectedEvents));
        WriteData(writer);
    }

    protected virtual void WriteData(BinaryWriter writer)
    {
    }

}
