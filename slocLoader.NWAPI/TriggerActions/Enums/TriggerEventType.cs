using System;

namespace slocLoader.TriggerActions.Enums {

    [Flags]
    public enum TriggerEventType : byte {

        None = 0,
        Enter = 1,
        Stay = 2,
        Exit = 4,
        All = Enter | Stay | Exit

    }

}
