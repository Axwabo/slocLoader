using HarmonyLib;
using Mirror;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SendSpawnMessage))]
public static class SendSpawnMessagePatch
{

    public delegate void SpawnDataModifier(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message);

    public static readonly List<SpawnDataModifier> SpawnDataModifiers = new List<SpawnDataModifier> {SetGlobalTransformForSlocObject};

    public static void ModifySpawnMessage(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message)
    {
        foreach (var modifier in SpawnDataModifiers)
            modifier(identity, connection, ref message);
    }

    private static void SetGlobalTransformForSlocObject(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message)
    {
        if (!identity.TryGetComponent(out slocObjectData _))
            return;
        var transform = identity.transform;
        message.position = transform.position;
        message.rotation = transform.rotation;
        message.scale = transform.lossyScale;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var index = list.FindCall(nameof(NetworkConnection.Send)) - 3;
        list.InsertRange(index, new[]
        {
            Ldarg(0),
            Ldarg(1),
            Ldloca(4),
            Call(ModifySpawnMessage)
        });
        return list;
    }

}
