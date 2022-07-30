using System.Collections.Generic;
using System.Reflection;
using AdminToys;
using Axwabo.Helpers.Harmony;
using Axwabo.Helpers.Pools;
using HarmonyLib;
using UnityEngine;

namespace slocLoader {

    [HarmonyPatch(typeof(AdminToyBase), "UpdatePositionServer")]
    internal static class AdminToyPatch {

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);
            var scaleIndex = list.FindIndex(i => i.operand is MethodInfo {Name: "get_localScale"});
            list[scaleIndex] = InstructionHelper.Get<Transform>(nameof(Transform.lossyScale));
            foreach (var codeInstruction in list)
                yield return codeInstruction;
            ListPool<CodeInstruction>.Shared.Return(list);
        }

    }

}
