using slocLoader.Objects;

namespace slocLoader.Extensions;

public static class PrimitiveObjectFlagsExtensions
{

    public static bool HasFlagFast(this PrimitiveObjectFlags flags, PrimitiveObjectFlags check)
        => (flags & check) == check;

    public static bool IsTrigger(this PrimitiveObjectFlags flags)
        => flags.HasFlagFast(PrimitiveObjectFlags.Trigger);

}
