using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SpawnObserversForConnection))]
public static class SpawnObserversPatch
{

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var loopStart = list.FindCode(OpCodes.Stloc_2) + 1;
        list.InsertRange(loopStart, [
            Ldloc(2).Try(),
            Call(SendSpawnMessagePatch.ShouldUseGlobalTransform),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform))
        ]);
        var loopEnd = list.FindCall(nameof(IEnumerator<NetworkIdentity>.MoveNext)) - 1;
        list.InsertRange(loopEnd, [
            Int0.Finally(),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform)),
            EndFinally
        ]);
        return list;
    }

}
