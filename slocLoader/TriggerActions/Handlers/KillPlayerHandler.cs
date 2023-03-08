using PlayerStatsSystem;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers {

    public sealed class KillPlayerHandler : PlayerActionHandler<KillPlayerData> {

        public override TriggerActionType ActionType => TriggerActionType.KillPlayer;

        protected override void HandlePlayer(ReferenceHub player, KillPlayerData data, TriggerListener listener) {
            if (!player.characterClassManager.GodMode && player.roleManager.CurrentRole.ActiveTime > 0.1f)
                player.playerStats.DealDamage(new CustomReasonDamageHandler(data.Cause));
        }

    }

}
