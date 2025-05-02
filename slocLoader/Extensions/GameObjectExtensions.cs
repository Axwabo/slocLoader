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
        t.localScale = sloc.Transform.Scale;
        t.localPosition = sloc.Transform.Position;
        t.localRotation = sloc.Transform.Rotation;
        data = o.AddComponent<slocObjectData>();
    }

}
