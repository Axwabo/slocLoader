using System.Collections.Generic;
using System.Reflection.Emit;
using AdminToys;
using Axwabo.Helpers.Harmony;
using HarmonyLib;
using UnityEngine;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader {

    [HarmonyPatch(typeof(AdminToyBase), "UpdatePositionServer")]
    public static class AdminToyPatch {

        // we're optimizing the method by storing the transform in a local variable, so it doesn't take more Unity calls than needed
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var transform = generator.Local<Transform>();
            var list = new[] {
                This,
                Get<Component>(nameof(Component.transform)),
                transform.Set(),
                This,
                transform.Load(),
                Get<Transform>(nameof(Transform.position)),
                Set<AdminToyBase>(nameof(AdminToyBase.NetworkPosition)),
                This,
                transform.Load(),
                Get<Transform>(nameof(Transform.rotation)),
                New<LowPrecisionQuaternion>(new[] {typeof(Quaternion)}),
                Set<AdminToyBase>(nameof(AdminToyBase.NetworkRotation)),
                This,
                transform.Load(),
                Call(typeof(AdminToyPatch), nameof(GetScale)),
                Set<AdminToyBase>(nameof(AdminToyBase.NetworkScale)),
                Return
            };
            foreach (var codeInstruction in list)
                yield return codeInstruction;
        }

        public static Vector3 GetScale(Transform transform) {
            var scale = transform.lossyScale;
            var absoluteScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return !transform.TryGetComponent(out slocObjectData data)
                ? scale
                : data.HasColliderOnClient
                    ? absoluteScale
                    : -absoluteScale;
        }

    }

}
