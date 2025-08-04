using HarmonyLib;
using Mirror;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SendSpawnMessage))]
public static class SendSpawnMessagePatch
{

    public static bool ShouldUseGlobalTransform(NetworkIdentity identity) => identity.TryGetComponent(out slocObjectData data) && data.GlobalTransform;

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform", true)]
    public delegate void SpawnDataModifier(NetworkIdentity identity, NetworkConnection connection, ref SpawnMessage message);

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform", true)]
    public static readonly List<SpawnDataModifier> SpawnDataModifiers = [];

    [Obsolete("Deprecated in favor of API::SpawnWithGlobalTransform", true)]
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
            transform.GetPositionAndRotation(out var position, out var rotation);
            message.position = position;
            message.rotation = rotation;
            message.scale = transform.lossyScale;
            return;
        }

        transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
        message.position = localPosition;
        message.rotation = localRotation;
        message.scale = transform.localScale;
    }

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var start = list.FindField(nameof(SpawnMessage.position)) - 2;
        var end = list.FindField(nameof(SpawnMessage.scale));
        list.RemoveRange(start, end - start + 1);
        list.Insert(start, Call(SetTransformValues));
        return list;
    }

}
