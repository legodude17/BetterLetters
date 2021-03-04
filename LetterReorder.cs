using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LetterReorder
{
    public class LetterReorder : Mod
    {
        public LetterReorder(ModContentPack content) : base(content)
        {
            var harm = new Harmony("legodude17.letterreorder");
            harm.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("Applied patches for " + harm.Id);
        }
    }

    [HarmonyPatch(typeof(LetterStack), "LettersOnGUI")]
    public class LetterStackOnGUI
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
//            Log.Message("<Transpiler>");
            var list = instructions.ToList();
//            PrintCode(list);
            var ind1 = list.FindIndex(ins => ins.opcode == OpCodes.Ldarg_1);
//            Log.Message("  Found ldarg_1 at " + ind1);
            var ind2 = list.FindIndex(ins => ins.opcode == OpCodes.Bge_S);
//            Log.Message("  Found bge_s at " + ind2);
            var list2 = SwapLoop(list.GetRange(ind1, ind2 - ind1 + 1)).ToList();
//            Log.Message("  Finished SwapLoop 1");
            list.RemoveRange(ind1, ind2 - ind1 + 1);
//            Log.Message("  Removed first loop");
            list.InsertRange(ind1, list2);
//            Log.Message("  Added first loop");
            var ind5 = list.FindIndex(ind1 + list2.Count, ins => ins.opcode == OpCodes.Bne_Un_S);
//            Log.Message("  Found bne_un_s at " + ind5);
            var ind3 = list.FindIndex(ind5, ins => ins.opcode == OpCodes.Ldarg_0);
//            Log.Message("  Found ldarg_0 at " + ind3);
            var ind4 = list.FindIndex(ind5, ins => ins.opcode == OpCodes.Bge_S);
//            Log.Message("  Found bge_s at " + ind4);
            var list3 = SwapLoop(list.GetRange(ind3, ind4 - ind3 + 1)).ToList();
//            Log.Message("  Finished SwapLoop 2");
            list.RemoveRange(ind3, ind4 - ind3 + 1);
//            Log.Message("  Removed second loop");
            list.InsertRange(ind3, list3);
//            Log.Message("  Added second loop");
//            PrintCode(list);
            return list;
        }

        public static IEnumerable<CodeInstruction> SwapLoop(IEnumerable<CodeInstruction> instructions)
        {
//            Log.Message("  <SwapLoop>");
            var list = instructions.ToList();
//            Log.Message("    Length of list: " + list.Count);
            var ind1 = list.FindIndex(ins => ins.opcode == OpCodes.Ldarg_0);
//            Log.Message("    Found ldarg_0 at " + ind1);
            var ind2 = list.FindIndex(ins => ins.opcode == OpCodes.Callvirt);
//            Log.Message("    Found callvirt at " + ind2);
            var ind4 = list.FindIndex(ins => ins.opcode == OpCodes.Ldc_I4_0);
//            Log.Message("    Found ldc_i4_0 at " + ind4);
            var len = ind2 - ind1 + 1;
            var list2 = list.GetRange(ind1, len);
//            Log.Message("    Got second list, length: " + list2.Count);
            list.RemoveRange(ind1, len);
//            Log.Message("    Removed first section");
            list.First(ins => ins.opcode == OpCodes.Ldc_I4_1).opcode = OpCodes.Ldc_I4_0;
//            Log.Message("    Rewrote ldc_i4_1 into ldc_i4_0");
            list.Remove(list.First(ins => ins.opcode == OpCodes.Sub));
//            Log.Message("    Removed sub");
            var branch = list.First(ins => ins.opcode == OpCodes.Bge_S);
//            Log.Message("    Found branch");
            branch.opcode = OpCodes.Blt_S;
//            Log.Message("    Rewrote bge_s into blt_s");
            // var target = list.First(ins => ins.opcode == OpCodes.Ldloc_0);
            // Log.Message("    Found branch target");
            // branch.operand = target.labels.First();
            // Log.Message("    Changed branch target");
            ind4 = ind4 - len - 1;
//            Log.Message("    Changed ldc_i4_0 index to " + ind4);
//            Log.Message("    Length of list: " + list.Count);
            list.RemoveAt(ind4);
//            Log.Message("    Removed ldc_i4_0");
            list.InsertRange(ind4, list2);
//            Log.Message("    Added list2");
            list.Where(ins => ins.opcode == OpCodes.Sub).ToList()[2].opcode = OpCodes.Add;
//            Log.Message("    Rewrote index decrement into increment");
//            Log.Message("  </SwapLoop>");
            return list;
        }

//        private static void PrintCode(IEnumerable<CodeInstruction> instructions)
//        {
//            Log.Message("  <Code>");
//            foreach (var ins in instructions)
//                if (ins.operand is Label label)
//                    Log.Message("    " + ins.opcode.Name + " " + label);
//                else
//                    Log.Message("    " + ins.opcode.Name + " " + ins.operand);
//
//            Log.Message("  </Code>");
//        }
    }

    [HarmonyPatch(typeof(DrugPolicy), "get_Item", typeof(ThingDef))]
    [HarmonyPriority(Priority.First)]
    public class DrugPolicy_get_Item_ThingDef
    {
        public static bool Prefix(List<DrugPolicyEntry> ___entriesInt, ThingDef drugDef, ref DrugPolicyEntry __result)
        {
            __result = ___entriesInt.FirstOrFallback(entry => entry.drug == drugDef, new DrugPolicyEntry
            {
                drug = drugDef,
                allowScheduled = false,
                allowedForAddiction = false,
                allowedForJoy = false,
                takeToInventory = 0
            });
            return false;
        }
    }
}