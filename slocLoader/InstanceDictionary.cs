namespace slocLoader;

public sealed class InstanceDictionary<T> : Dictionary<int, T>
{

    public T GetOrReturn(int id, T value, bool extraCondition = true) => extraCondition && TryGetValue(id, out var existing) ? existing : value;

    public bool TryGet<TGet>(int id, out TGet value)
    {
        if (TryGetValue(id, out var obj) && obj is TGet t)
        {
            value = t;
            return true;
        }

        value = default;
        return false;
    }

}
