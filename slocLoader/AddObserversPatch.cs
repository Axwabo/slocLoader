using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.AddAllReadyServerConnectionsToObservers))]
public static class AddObserversPatch
{

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var list = new List<CodeInstruction>(instructions);
        var local = generator.Local<bool>();
        list.InsertRange(0, [
            Ldarg(0),
            Call(SendSpawnMessagePatch.ShouldUseGlobalTransform),
            local.Set()
        ]);
        var index = list.FindCall(nameof(NetworkIdentity.AddObserver)) - 2;
        list.InsertRange(index, [
            local.Load().WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)).MoveLabelsFrom(list[index]),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform))
        ]);
        var loopEnd = list.FindCall(nameof(NetworkIdentity.AddObserver)) + 1;
        list.InsertRange(loopEnd, [
            Int0.WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock)),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform)),
            new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock))
        ]);
        list.InsertRange(list.Count - 4, [
            local.Load().WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform))
        ]);
        list.InsertRange(list.Count - 1, [
            Int0.WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock)),
            Stfld(typeof(API), nameof(API.ShouldSpawnWithGlobalTransform)),
            new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock))
        ]);
        return list;
    }

}
