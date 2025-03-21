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

}
