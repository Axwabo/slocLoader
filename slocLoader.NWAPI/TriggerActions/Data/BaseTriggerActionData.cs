using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public abstract class BaseTriggerActionData {

        [SerializeField]
        private TargetType selected;

        public abstract TargetType PossibleTargets { get; }

        public abstract TriggerActionType ActionType { get; }

        public TargetType SelectedTargets {
            get => selected;
            set {
                if (value is TargetType.None) {
                    selected = value;
                    return;
                }

                var possible = PossibleTargets;
                foreach (var v in ActionManager.TargetTypeValues)
                    if (value.HasFlagFast(v) && possible.HasFlagFast(v))
                        selected |= v;
                    else
                        selected &= ~v;
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
