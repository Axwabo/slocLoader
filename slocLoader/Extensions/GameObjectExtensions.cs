using slocLoader.Objects;

namespace slocLoader.Extensions;

public static class GameObjectExtensions
{

    public static void ApplyNameAndTag(this GameObject o, string name, string tag)
    {
        if (name != null)
            o.name = name;
        if (tag != null)
            o.tag = tag;
    }

    public static void ApplyCommonData(this GameObject o, slocGameObject sloc, GameObject parent, out slocObjectData data)
    {
        o.ApplyNameAndTag(sloc.Name, sloc.Tag);
        var t = o.transform;
        if (parent != null)
            t.SetParent(parent.transform, false);
        t.SetLocalPositionAndRotation(sloc.Transform.Position, sloc.Transform.Rotation);
        t.localScale = sloc.Transform.Scale;
        data = o.AddComponent<slocObjectData>();
    }

}
