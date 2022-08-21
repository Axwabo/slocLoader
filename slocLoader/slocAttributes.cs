using System;

namespace slocLoader {

    [Flags]
    public enum slocAttributes : byte {

        None = 0,
        LossyColors = 1,
        ForcedColliderMode = 2,

    }

}
