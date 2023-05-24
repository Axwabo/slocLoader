using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Handlers;

namespace slocLoader.TriggerActions;

public sealed class TriggerListener : MonoBehaviour
{

    public readonly List<HandlerDataPair> OnEnter = new();
    public readonly List<HandlerDataPair> OnStay = new();
    public readonly List<HandlerDataPair> OnExit = new();

    private void OnTriggerEnter(Collider other) => ExecuteAll(other, OnEnter);

    private void OnTriggerStay(Collider other) => ExecuteAll(other, OnStay);

    private void OnTriggerExit(Collider other) => ExecuteAll(other, OnExit);

    private void ExecuteAll(Collider other, List<HandlerDataPair> list)
    {
        var go = other.gameObject;
        var root = go.transform.root;
        if (root.TryGetComponent(out ItemPickupBase pickup))
            go = pickup.gameObject;
        else if (root.TryGetComponent(out BasicRagdoll ragdoll))
            go = ragdoll.gameObject;
        try
        {
            foreach (var pair in list)
                pair.Handler.HandleObject(go, pair.Data, this);
        }
        finally
        {
            TeleporterImmunityHandler.ApplyAllQueued(go, this);
        }
    }

}
