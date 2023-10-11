using Mirror;

namespace slocLoader;

[DisallowMultipleComponent]
public sealed class slocObjectData : MonoBehaviour
{

    [NonSerialized]
    public NetworkIdentity networkIdentity;

    public uint netId => networkIdentity.netId;

    public bool HasColliderOnClient { get; set; } = true;

    public bool ShouldBeSpawnedOnClient { get; set; } = true;

    public bool HasColliderOnServer { get; set; } = true;

    public bool IsTrigger { get; set; }

    private void Awake() => networkIdentity = GetComponent<NetworkIdentity>();

}
