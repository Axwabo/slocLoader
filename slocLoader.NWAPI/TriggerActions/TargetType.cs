using System;

namespace slocLoader.TriggerActions {

    [Flags]
    public enum TargetType : byte {

        None = 0,
        Player = 1,
        Pickup = 2,
        Toy = 4,
        All = 255

    }

}
