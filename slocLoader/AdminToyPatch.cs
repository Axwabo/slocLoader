using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AdminToys;
using Axwabo.Helpers.Harmony;
using Axwabo.Helpers.Pools;
using HarmonyLib;
using UnityEngine;
using static Axwabo.Helpers.Harmony.InstructionHelper;

namespace slocLoader {

    [HarmonyPatch(typeof(AdminToyBase), "UpdatePositionServer")]
    internal static class AdminToyPatch {

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);
            var scaleIndex = list.FindIndex(i => i.operand is MethodInfo {Name: "get_localScale"});
            var originalLabel = generator.DefineLabel();
            var otherLabel = generator.DefineLabel();
            list[scaleIndex].labels.Add(originalLabel);
            list[scaleIndex + 1].labels.Add(otherLabel);
            list.InsertRange(scaleIndex, new[] {
                This,
                Call(typeof(API), nameof(API.ShouldUseLossyScale)),
                originalLabel.False(),
                Get<Transform>(nameof(Transform.lossyScale)),
                otherLabel.Jump()
            });
            foreach (var codeInstruction in list)
                yield return codeInstruction;
            ListPool<CodeInstruction>.Shared.Return(list);
        }

    }

}
