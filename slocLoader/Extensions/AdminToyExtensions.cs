using AdminToys;
using slocLoader.Objects;

namespace slocLoader.Extensions;

public static class AdminToyExtensions
{

    public static void ApplyCommonData(this AdminToyBase toy, slocGameObject sloc, GameObject parent, out GameObject gameObject, out slocObjectData data)
    {
        gameObject = toy.gameObject;
        gameObject.ApplyCommonData(sloc, parent, out data);
        toy.MovementSmoothing = sloc.MovementSmoothing;
        toy.ApplyTransformNetworkProperties(sloc.Transform);
    }

    public static void ApplyTransformNetworkProperties(this AdminToyBase toy, slocTransform localTransform)
        => toy.ApplyTransformNetworkProperties(localTransform.Position, localTransform.Rotation, localTransform.Scale);

    public static void ApplyTransformNetworkProperties(this AdminToyBase toy, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        toy.NetworkPosition = localPosition;
        toy.NetworkRotation = localRotation;
        toy.NetworkScale = localScale;
    }

}
