using AdminToys;
using HarmonyLib;
using slocLoader.TriggerActions;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(PrimitiveObjectToy), nameof(PrimitiveObjectToy.SetPrimitive))]
public static class SetPrimitiveTypePatch
{

    // where do we even use this lmfao
    public static bool DestroyCollider(PrimitiveObjectToy toy)
    {
        if (toy._collider)
            Object.Destroy(toy._collider);
        return toy.TryGetComponent(out slocObjectData data) && data.IsTrigger;
    }

    [Obsolete("Use UpdateColliderTriggerState(PrimitiveObjectToy) instead.")]
    public static void SetTrigger(MeshCollider collider, PrimitiveObjectToy toy)
    {
        if (!toy.TryGetComponent(out slocObjectData data))
            return;
        collider.isTrigger = data.IsTrigger;
        if (toy.TryGetComponent(out TriggerListener listener))
            collider.gameObject.AddComponent<TriggerListenerInvoker>().Parent = listener;
    }

    public static void UpdateColliderTriggerState(PrimitiveObjectToy toy)
    {
        if (!toy.TryGetComponent(out slocObjectData data) || !toy._collider)
            return;
        toy._collider.isTrigger = data.IsTrigger;
        if (toy.TryGetComponent(out TriggerListener listener))
            toy._collider.GetOrAddComponent<TriggerListenerInvoker>().Parent = listener;
    }

    public static bool ShouldAddCollider(PrimitiveObjectToy toy)
        => toy.TryGetComponent(out slocObjectData data) ? data.HasColliderOnServer : toy.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Collidable);

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        list.InsertRange(list.Count - 2, [
            This,
            Call(UpdateColliderTriggerState)
        ]);
        return list;
    }

}
