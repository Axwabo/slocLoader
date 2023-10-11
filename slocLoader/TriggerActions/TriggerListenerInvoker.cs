namespace slocLoader.TriggerActions;

[DisallowMultipleComponent]
public sealed class TriggerListenerInvoker : MonoBehaviour
{

    public TriggerListener Parent { get; set; }

    private void Start()
    {
        if (!Parent)
            Destroy(this);
    }

    private void OnTriggerEnter(Collider other) => Parent.InvokeOnEnter(other);

    private void OnTriggerStay(Collider other) => Parent.InvokeOnStay(other);

    private void OnTriggerExit(Collider other) => Parent.InvokeOnExit(other);

}
