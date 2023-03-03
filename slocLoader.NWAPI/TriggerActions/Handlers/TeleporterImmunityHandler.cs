using Axwabo.Helpers;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleporterImmunityHandler : PlayerActionHandler<TeleporterImmunityData> {

        public override TriggerActionType ActionType => TriggerActionType.TeleporterImmunity;

        protected override void HandlePlayer(ReferenceHub player, TeleporterImmunityData data, TriggerListener listener) {
            var storage = player.GetOrAddComponent<TeleporterImmunityStorage>();
            if (data.IsGlobal) {
                if (!storage.IsGloballyImmune)
                    storage.MakeGloballyImmune(data.Duration);
            } else {
                var id = listener.GetInstanceID();
                if (!storage.IsLocallyImmune(id))
                    storage.MakeLocallyImmune(id, data.Duration);
            }
        }

    }

}
