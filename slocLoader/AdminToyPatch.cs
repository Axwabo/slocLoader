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
    public static class AdminToyPatch {

        public static readonly Dictionary<int, Vector3> DesiredScale = new();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);
            var scaleIndex = list.FindIndex(i => i.operand is MethodInfo {Name: "get_localScale"});
            list[scaleIndex] = Get<Transform>(nameof(Transform.lossyScale));
            var id = generator.Local<int>();
            var value = generator.Local<Vector3>();
            var setScale = generator.DefineLabel();
            /*
            list.InsertRange(list.FindCode(OpCodes.Ret), new[] {
                Ldfld(typeof(AdminToyPatch), nameof(DesiredScale)),
                This,
                Call<Object>(nameof(Object.GetInstanceID)),
                Duplicate,
                id.Set(),
                value.LoadAddress(),
                Call<Dictionary<int, Vector3>>("TryGetValue"),
                setScale.True(),
                Return,
                This.WithLabels(setScale),
                Get<Component>(nameof(Component.transform)),
                value.Load(),
                Set<Transform>(nameof(Transform.localScale)),
                Ldfld(typeof(AdminToyPatch), nameof(DesiredScale)),
                id.Load(),
                Call<Dictionary<int, Vector3>>("Remove", new[] {typeof(int)}),
                Pop
            });
            */
            foreach (var codeInstruction in list)
                yield return codeInstruction;
            ListPool<CodeInstruction>.Shared.Return(list);
        }

    }

}
