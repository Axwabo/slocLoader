using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Handlers;

public static class TriggerActionHelpers
{

    public static void SetRagdollPosition(BasicRagdoll ragdoll, Vector3 targetPosition) => SetRagdollPositionAndRotation(ragdoll, targetPosition, Quaternion.identity);

    public static void SetRagdollPositionAndRotation(BasicRagdoll ragdoll, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!slocPlugin.Instance.Config.EnableRagdollPositionModification)
            return;
        var t = ragdoll.transform;
        t.position = targetPosition;
        t.rotation = targetRotation;
        var identity = ragdoll.netIdentity;
        foreach (var hub in ReferenceHub.AllHubs)
            if (hub.connectionToClient != null)
                NetworkServer.SendSpawnMessage(identity, hub.connectionToClient);
    }

    public static void ResetVelocityOfPickup(ItemPickupBase pickup, TeleportOptions options)
    {
        if (!options.Is(TeleportOptions.ResetVelocity))
            return;
        switch (pickup.PhysicsModule)
        {
            case PickupStandardPhysics physics:
                physics.Rb.velocity = Vector3.zero;
                physics.Rb.angularVelocity = Vector3.zero;
                break;
            case Scp018Physics scp018:
                scp018._lastVelocity = Vector3.zero;
                break;
        }
    }

    public static bool TryGetMovementModule(this ReferenceHub hub, out FirstPersonMovementModule module)
    {
        if (hub.roleManager.CurrentRole is not IFpcRole {FpcModule: var fpmm})
        {
            module = null;
            return false;
        }

        module = fpmm;
        return true;
    }

    public static void OverridePosition(this ReferenceHub hub, Vector3 position, TeleportOptions options = TeleportOptions.ResetFallDamage)
        => hub.OverridePosition(position, options | TeleportOptions.DeltaRotation, 0);

    public static void OverridePosition(this ReferenceHub hub, Vector3 position, TeleportOptions options, float rotation)
    {
        if (!hub.TryGetMovementModule(out var module))
            return;
        if (options.HasFlagFast(TeleportOptions.ResetFallDamage))
            module.Motor.ResetFallDamageCooldown();
        var delta = options.HasFlagFast(TeleportOptions.DeltaRotation)
            ? rotation
            : rotation - module.MouseLook.CurrentHorizontal;
        module.ServerOverridePosition(position, new Vector3(0, delta, 0));
    }

}
