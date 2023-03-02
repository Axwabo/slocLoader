using System;

namespace slocLoader.TriggerActions.Enums {

    [Flags]
    public enum TargetType : byte {

        None = 0,
        Player = 1,
        Pickup = 2,
        Toy = 4,
        Ragdoll = 8,
        All = Player | Pickup | Toy | Ragdoll

    }

}
