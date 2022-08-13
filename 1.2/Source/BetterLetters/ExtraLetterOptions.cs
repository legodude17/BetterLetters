using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace BetterLetters
{
    public class ExtraLetterOptions
    {
        public static void DoPatch(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Letter), "DrawButtonAt"),
                transpiler: new HarmonyMethod(typeof(ExtraLetterOptions), "Transpiler"));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var info = AccessTools.Method(typeof(Letter), "get_CanDismissWithRightClick");
            var idx = list.FindIndex(ins => ins.Calls(info));
            list.InsertRange(idx, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraLetterOptions), "DoExtraOptions")),
                new CodeInstruction(OpCodes.Ldarg_0)
            });
            return list;
        }

        public static void DoExtraOptions(Letter letter, Rect rect)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.control)
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("BetterLetters.ClearAll".Translate(), () => Find.LetterStack.ClearAll()),
                    new FloatMenuOption("BetterLetters.BlockDef".Translate(letter.def.defName),
                        () =>
                        {
                            BetterLettersMod.Settings.Blocking.Block(letter.def);
                            BetterLettersMod.Instance.WriteSettings();
                        }),
                    new FloatMenuOption("BetterLetters.BlockLabel".Translate(letter.label),
                        () =>
                        {
                            BetterLettersMod.Settings.Blocking.Block(letter.label);
                            BetterLettersMod.Instance.WriteSettings();
                        }),
                    new FloatMenuOption("BetterLetters.ClearAllBlocked".Translate(),
                        () => Find.LetterStack.ClearAll(BetterLettersMod.Settings.Blocking.ShouldBlock)),
                    new FloatMenuOption("BetterLetters.ClearAllOfDef".Translate(letter.def.defName),
                        () => Find.LetterStack.ClearAll(let => let.def == letter.def))
                }));
                Event.current.Use();
            }
        }
    }
}