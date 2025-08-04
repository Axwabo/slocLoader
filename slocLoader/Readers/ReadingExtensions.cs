using slocLoader.Objects;

namespace slocLoader.Readers
{

    public static class ReadingExtensions
    {

        public static T ApplyProperties<T>(this T o, CommonObjectProperties properties) where T : slocGameObject
        {
            o.ParentId = properties.ParentId;
            o.Transform = properties.Transform;
            o.Name = properties.Name;
            o.Tag = properties.Tag;
            return o;
        }

    }

}
