namespace slocLoader;

[Flags]
public enum slocAttributes : byte
{

    None = 0,
    LossyColors = 1,

    [Obsolete("Use DefaultFlags instead.")]
    DefaultColliderMode = DefaultFlags,

    DefaultFlags = 2,
    ExportAllTriggerActions = 4,
    NamesAndTags = 8

}
