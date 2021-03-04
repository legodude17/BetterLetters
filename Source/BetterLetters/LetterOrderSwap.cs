using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace BetterLetters
{
    public class LetterOrderSwap
    {
        public static void DoPatch(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(LetterStack), "LettersOnGUI"),
                transpiler: new HarmonyMethod(typeof(LetterOrderSwap), "Transpiler"));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var ind1 = list.FindIndex(ins => ins.opcode == OpCodes.Ldarg_1);
            var ind2 = list.FindIndex(ins => ins.opcode == OpCodes.Bge_S);
            var list2 = SwapLoop(list.GetRange(ind1, ind2 - ind1 + 1)).ToList();
            list.RemoveRange(ind1, ind2 - ind1 + 1);
            list.InsertRange(ind1, list2);
            var ind5 = list.FindIndex(ind1 + list2.Count, ins => ins.opcode == OpCodes.Bne_Un_S);
            var ind3 = list.FindIndex(ind5, ins => ins.opcode == OpCodes.Ldarg_0);
            var ind4 = list.FindIndex(ind5, ins => ins.opcode == OpCodes.Bge_S);
            var list3 = SwapLoop(list.GetRange(ind3, ind4 - ind3 + 1)).ToList();
            list.RemoveRange(ind3, ind4 - ind3 + 1);
            list.InsertRange(ind3, list3);
            return list;
        }

        public static IEnumerable<CodeInstruction> SwapLoop(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var ind1 = list.FindIndex(ins => ins.opcode == OpCodes.Ldarg_0);
            var ind2 = list.FindIndex(ins => ins.opcode == OpCodes.Callvirt);
            var ind4 = list.FindIndex(ins => ins.opcode == OpCodes.Ldc_I4_0);
            var len = ind2 - ind1 + 1;
            var list2 = list.GetRange(ind1, len);
            list.RemoveRange(ind1, len);
            list.First(ins => ins.opcode == OpCodes.Ldc_I4_1).opcode = OpCodes.Ldc_I4_0;
            list.Remove(list.First(ins => ins.opcode == OpCodes.Sub));
            var branch = list.First(ins => ins.opcode == OpCodes.Bge_S);
            branch.opcode = OpCodes.Blt_S;
            ind4 = ind4 - len - 1;
            list.RemoveAt(ind4);
            list.InsertRange(ind4, list2);
            list.Where(ins => ins.opcode == OpCodes.Sub).ToList()[2].opcode = OpCodes.Add;
            return list;
        }
    }
}