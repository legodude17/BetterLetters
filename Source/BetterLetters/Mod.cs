using HarmonyLib;
using ModBase;
using RimWorld;
using Verse;

namespace BetterLetters
{
    public class BetterLettersMod : BaseMod<BetterLettersSettings>
    {
        public static BetterLettersMod Instance;

        public BetterLettersMod(ModContentPack content) : base("legodude17.bletters", null, content, false)
        {
            Harm.Patch(AccessTools.Method(typeof(LetterStack), "ReceiveLetter",
                new[] {typeof(Letter), typeof(string)}), new HarmonyMethod(typeof(BetterLettersMod), "CheckForBlock"));
            ExtraLetterOptions.DoPatch(Harm);
            ArchiveSearchBar.DoPatches(Harm);
            Instance = this;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            if (Settings.Blocking == null) Settings.Blocking = new LetterBlocking();
            Harm.Unpatch(AccessTools.Method(typeof(LetterStack), "LettersOnGUI"), HarmonyPatchType.Transpiler, Harm.Id);
            if (Settings.SwapOrder) LetterOrderSwap.DoPatch(Harm);
        }

        public static bool CheckForBlock(Letter let, string debugInfo = null)
        {
            if (!Settings.AllowLetters || Settings.Blocking.ShouldBlock(let))
            {
                if (Settings.AlertOnBlock)
                    Messages.Message("BetterLetters.Blocked".Translate(let.def.defName, let.label),
                        MessageTypeDefOf.NeutralEvent);
                Find.Archive.Add(let);
                return false;
            }

            return true;
        }
    }

    public class BetterLettersSettings : BaseModSettings
    {
        [Default(false)] public bool AlertOnBlock = false;
        [Default(true)] public bool AllowLetters = true;
        public LetterBlocking Blocking = new LetterBlocking();
        [Default(true)] public bool SwapOrder = true;
    }
}