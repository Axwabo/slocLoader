using System.IO;

namespace slocLoader.TriggerActions.Data {

    public abstract class BaseTriggerActionData {

        private TargetType _selected;

        public abstract TargetType PossibleTargets { get; }

        public abstract TriggerActionType ActionType { get; }

        public TargetType SelectedTargets {
            get => _selected;
            set {
                if (value is TargetType.None) {
                    _selected = value;
                    return;
                }

                var possible = PossibleTargets;
                foreach (var v in ActionManager.TargetTypeValues) {
                    if (value.HasFlagFast(v) && possible.HasFlagFast(v))
                        _selected |= v;
                    else
                        _selected &= ~v;
                }
            }
        }

        protected BaseTriggerActionData() => SelectedTargets = TargetType.All;

        public void WriteTo(BinaryWriter writer) {
            writer.Write((byte) SelectedTargets);
            writer.Write((ushort) ActionType);
            WriteData(writer);
        }

        protected virtual void WriteData(BinaryWriter writer) {
        }

    }

}
