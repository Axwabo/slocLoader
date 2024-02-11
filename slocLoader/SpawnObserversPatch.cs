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
        list.InsertRange(loopStart, new[]
        {
            Ldloc(2).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            Call(SendSpawnMessagePatch.ShouldUseGlobalTransform),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform))
        });
        var loopEnd = list.FindCall(nameof(IEnumerator<NetworkIdentity>.MoveNext)) - 1;
        list.InsertRange(loopEnd, new[]
        {
            Int0.WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock)),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform)),
            new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock))
        });
        return list;
    }

}
