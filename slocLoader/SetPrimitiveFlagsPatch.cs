using System.Reflection.Emit;
using AdminToys;
using HarmonyLib;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(PrimitiveObjectToy), nameof(PrimitiveObjectToy.SetFlags))]
internal static class SetPrimitiveFlagsPatch
{

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var start = list.FindCode(OpCodes.Ldarg_2);
        var end = list.FindCall("set_enabled");
        list.RemoveRange(start, end - start);
        list.InsertRange(start, [
            This,
            Call(SetPrimitiveTypePatch.ShouldAddCollider)
        ]);
        return list;
    }

}
