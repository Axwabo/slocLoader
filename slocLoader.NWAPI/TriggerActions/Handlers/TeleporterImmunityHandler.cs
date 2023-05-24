using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleporterImmunityHandler : ITriggerActionHandler
{

    private static readonly List<TeleporterImmunityData> Queued = new();

    public TargetType Targets => TargetType.All;
    public TriggerActionType ActionType => TriggerActionType.TeleporterImmunity;

    public void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener)
    {
        if (data is TeleporterImmunityData immunityData)
            Queued.Add(immunityData);
    }

    public static void ApplyAllQueued(GameObject go, TriggerListener listener)
    {
        if (Queued.Count == 0)
            return;
        var storage = go.GetOrAddComponent<TeleporterImmunityStorage>();
        foreach (var data in Queued)
        {
            var id = listener.GetInstanceID();
            if (data.IsGlobal)
                storage.MakeGloballyImmune(data.Duration, data.DurationMode);
            else
                storage.MakeLocallyImmune(id, data.Duration, data.DurationMode);
        }

        Queued.Clear();
    }

}
