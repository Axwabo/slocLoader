using System.Reflection;
using System.Reflection.Emit;
using AdminToys;
using HarmonyLib;
using slocLoader.TriggerActions;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(PrimitiveObjectToy), nameof(PrimitiveObjectToy.SetPrimitive))]
public static class SetPrimitiveTypePatch
{

    public static bool DestroyCollider(PrimitiveObjectToy toy)
    {
        if (toy._spawnedPrimitive.TryGetComponent(out Collider collider))
            Object.Destroy(collider);
        return toy.TryGetComponent(out slocObjectData data) && data.IsTrigger;
    }

    public static void SetTrigger(MeshCollider collider, PrimitiveObjectToy toy)
    {
        if (!toy.TryGetComponent(out slocObjectData data))
            return;
        collider.isTrigger = data.IsTrigger;
        if (toy.TryGetComponent(out TriggerListener listener))
            collider.gameObject.AddComponent<TriggerListenerInvoker>().Parent = listener;
    }

    public static bool ShouldAddCollider(PrimitiveObjectToy toy)
        => toy.TryGetComponent(out slocObjectData data) ? data.HasColliderOnServer : toy.Scale is {x: > 0} or {y: 0} or {z: > 0};

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var label = list[list.FindIndex(i => i.operand is MethodInfo {Name: "get_clear"}) - 1].labels[0];
        var block = list.FindCode(OpCodes.Beq_S) - 2;
        var condition = list.FindLastIndex(block, i => i.operand is MethodInfo {Name: "Destroy"}) + 1;
        list.RemoveRange(condition, block - condition);
        list.InsertRange(condition, new[]
        {
            This,
            Call(ShouldAddCollider),
            label.False()
        });
        var add = list.FindIndex(i => i.operand is MethodInfo {Name: "AddComponent"});
        list.Insert(add + 1, Duplicate);
        var set = list.FindIndex(i => i.operand is MethodInfo {Name: "set_convex"});
        list.InsertRange(set + 1, new[]
        {
            This,
            Call(SetTrigger)
        });
        return list;
    }

}
