using System.Reflection.Emit;
using AdminToys;
using HarmonyLib;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(AdminToyBase), nameof(AdminToyBase.UpdatePositionServer))]
public static class AdminToyPatch
{

    // we're optimizing the method by storing the transform in a local variable, so it doesn't take more Unity calls than needed
    // ReSharper disable once UnusedParameter.Local
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var transform = generator.Local<Transform>();
        var label = generator.DefineLabel();
        return
        [
            This,
            Ldfld<AdminToyBase>(nameof(AdminToyBase.IsStatic)),
            label.False(),
            Return,
            This.WithLabels(label),
            Get<Component>(nameof(Component.transform)),
            transform.Set(),
            This,
            transform.Load(),
            Get<Transform>(nameof(Transform.localPosition)),
            Set<AdminToyBase>(nameof(AdminToyBase.NetworkPosition)),
            This,
            transform.Load(),
            Get<Transform>(nameof(Transform.localRotation)),
            Set<AdminToyBase>(nameof(AdminToyBase.NetworkRotation)),
            This,
            transform.Load(),
            Get<Transform>(nameof(Transform.localScale)),
            Set<AdminToyBase>(nameof(AdminToyBase.NetworkScale)),
            Return
        ];
    }

    [Obsolete("Deprecated in favor of primitive flags, negative scaling is no longer required for the collider to be disabled.")]
    public static Vector3 GetScaleFromTransform(Transform transform)
    {
        var scale = transform.lossyScale;
        return !transform.TryGetComponent(out slocObjectData data)
            ? scale
            : GetScale(scale, data.HasColliderOnClient);
    }

    [Obsolete("Deprecated in favor of primitive flags, negative scaling is no longer required for the collider to be disabled.")]
    public static Vector3 GetScale(Vector3 original, bool positive)
    {
        var absoluteScale = new Vector3(Mathf.Abs(original.x), Mathf.Abs(original.y), Mathf.Abs(original.z));
        return positive ? original : -absoluteScale;
    }

}
