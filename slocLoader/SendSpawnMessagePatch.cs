using HarmonyLib;
using Mirror;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SendSpawnMessage))]
public static class SendSpawnMessagePatch
{

    public static bool ShouldUseGlobalTransform(NetworkIdentity identity) => identity.TryGetComponent(out slocObjectData _);

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform")]
    public delegate void SpawnDataModifier(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message);

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform")]
    public static readonly List<SpawnDataModifier> SpawnDataModifiers = new List<SpawnDataModifier>();

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform")]
    public static void ModifySpawnMessage(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message)
    {
        foreach (var modifier in SpawnDataModifiers)
            modifier(identity, connection, ref message);
    }

    public static void SetTransformValues(ref SpawnMessage message, NetworkIdentity identity)
    {
        var transform = identity.transform;
        if (API.ShouldSpawnWithGlobalTransform)
        {
            message.position = transform.position;
            message.rotation = transform.rotation;
            message.scale = transform.lossyScale;
            return;
        }

        message.position = transform.localPosition;
        message.rotation = transform.localRotation;
        message.scale = transform.localScale;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var start = list.FindField(nameof(SpawnMessage.position)) - 2;
        var end = list.FindField(nameof(SpawnMessage.scale));
        list.RemoveRange(start, end - start + 1);
        list.Insert(start, Call(SetTransformValues));
        return list;
    }

}
