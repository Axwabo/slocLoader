using System.Collections.Generic;
using Axwabo.Helpers;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class TeleporterImmunityHandler : PlayerActionHandler<TeleporterImmunityData> {

        private static readonly List<TeleporterImmunityData> Queued = new();

        public override TriggerActionType ActionType => TriggerActionType.TeleporterImmunity;

        protected override void HandlePlayer(ReferenceHub player, TeleporterImmunityData data, TriggerListener listener) => Queued.Add(data);

        public static void ApplyAllQueued(GameObject go, TriggerListener triggerListener) {
            if (Queued.Count == 0)
                return;
            var storage = go.GetOrAddComponent<TeleporterImmunityStorage>();
            foreach (var data in Queued) {
                var id = triggerListener.GetInstanceID();
                if (data.IsGlobal)
                    storage.MakeGloballyImmune(data.Duration, data.DurationMode);
                else
                    storage.MakeLocallyImmune(id, data.Duration, data.DurationMode);
            }

            Queued.Clear();
        }

    }

}
