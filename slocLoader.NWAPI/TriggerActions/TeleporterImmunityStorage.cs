using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions;

[DisallowMultipleComponent]
public sealed class TeleporterImmunityStorage : MonoBehaviour
{

    public float GloballyImmuneUntil { get; set; }

    private readonly InstanceDictionary<float> _locallyImmuneUntil = new();

    public bool IsGloballyImmune => Time.timeSinceLevelLoad < GloballyImmuneUntil;

    public void MakeGloballyImmune(float duration, ImmunityDurationMode mode = ImmunityDurationMode.Absolute) => GloballyImmuneUntil = mode switch
    {
        ImmunityDurationMode.Absolute => Time.timeSinceLevelLoad + duration,
        ImmunityDurationMode.Add => GloballyImmuneUntil + duration,
        ImmunityDurationMode.Subtract => GloballyImmuneUntil - duration,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public bool IsLocallyImmune(int id) => _locallyImmuneUntil.TryGetValue(id, out var time) && Time.timeSinceLevelLoad < time;

    public void MakeLocallyImmune(int id, float duration, ImmunityDurationMode mode = ImmunityDurationMode.Absolute)
    {
        var currentImmunity = _locallyImmuneUntil.TryGetValue(id, out var data) ? data : 0f;
        var newImmunity = mode switch
        {
            ImmunityDurationMode.Absolute => Time.timeSinceLevelLoad + duration,
            ImmunityDurationMode.Add => currentImmunity + duration,
            ImmunityDurationMode.Subtract => currentImmunity - duration,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
        _locallyImmuneUntil[id] = newImmunity;
    }

    public static bool IsImmune(GameObject obj, TriggerListener listener) =>
        obj.TryGetComponent(out TeleporterImmunityStorage storage)
        && (storage.IsGloballyImmune || storage.IsLocallyImmune(listener.GetInstanceID()));

}
