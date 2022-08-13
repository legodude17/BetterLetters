using System.Collections.Generic;
using System.Linq;
using ModBase;
using UnityEngine;
using Verse;

namespace BetterLetters
{
    public class LetterBlocking : ICustomSettingsDraw, IExposable
    {
        public bool BlockingEnabled = true;
        private List<LetterBlock> blocks = new List<LetterBlock>();

        public float Height => blocks.Count * 25f + 50f;

        public void Render(Listing_Standard listing, string label, string tooltip)
        {
            listing.CheckboxLabeled(label, ref BlockingEnabled, tooltip);
            if (listing.ButtonText("BetterLetters.AddNew".Translate()))
                Find.WindowStack.Add(new Dialog_AddNewBlock(this));

            var toRemove = new List<LetterBlock>();
            foreach (var block in blocks)
            {
                var rect = listing.GetRect(25f);
                Widgets.Label(rect.LeftPartPixels(rect.width - 20f),
                    "BetterLetters.Blocking".Translate() + block.Description());
                if (Widgets.ButtonImage(rect.RightPartPixels(20f), Widgets.CheckboxOffTex)) toRemove.Add(block);
            }

            toRemove.ForEach(block => blocks.Remove(block));
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref BlockingEnabled, "enabled", true);
            Scribe_Collections.Look(ref blocks, "blocks", LookMode.Deep);
        }

        public bool ShouldBlock(Letter letter)
        {
            return BlockingEnabled && blocks.Any(block => block.Blocked(letter));
        }

        public void Block(LetterDef def)
        {
            blocks.Add(new LetterBlock {Def = def});
        }

        public void Block(string text)
        {
            blocks.Add(new LetterBlock {SearchText = text});
        }

        public void Block(LetterDef def, string text)
        {
            blocks.Add(new LetterBlock {Def = def, SearchText = text});
        }
    }

    public class LetterBlock : IExposable
    {
        public LetterDef Def;
        public string SearchText;

        public void ExposeData()
        {
            Scribe_Values.Look(ref SearchText, "SearchText");
            Scribe_Defs.Look(ref Def, "Def");
        }

        public bool Blocked(Letter letter)
        {
            return Def != null && letter.def == Def ||
                   !SearchText.NullOrEmpty() && letter.label.ToString().Contains(SearchText);
        }

        public string Description()
        {
            return new[]
            {
                Def == null ? "" : "BetterLetters.OfDef".Translate(Def.defName).ToString(),
                SearchText.NullOrEmpty() ? "" : "BetterLetters.Contain".Translate(SearchText).ToString()
            }.Where(str => !str.NullOrEmpty()).Aggregate("", (str1, str2) => str1 + " " + str2);
        }
    }

    public class Dialog_AddNewBlock : Window
    {
        private readonly LetterBlocking blocking;
        private LetterDef def;
        private string text;

        public Dialog_AddNewBlock(LetterBlocking b)
        {
            blocking = b;
            doCloseButton = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 200f);

        public override void PostClose()
        {
            base.PostClose();
            if (def != null || !text.NullOrEmpty())
                blocking.Block(def, text);
        }

        public override void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            if (listing.ButtonTextLabeled("BetterLetters.Def".Translate(),
                def != null ? def.defName : "BetterLetters.None".Translate().ToString()))
            {
                var opts = new List<FloatMenuOption>
                {
                    new FloatMenuOption("BetterLetters.None".Translate(), () => def = null)
                };
                opts.AddRange(DefDatabase<LetterDef>.AllDefs.Select(letterDef =>
                    new FloatMenuOption(letterDef.defName, () => def = letterDef)));
                Log.Message("Count: " + opts.Count);
                Find.WindowStack.Add(new FloatMenu(opts));
            }

            text = listing.TextEntryLabeled("BetterLetters.Text".Translate(), text);
            listing.End();
        }
    }
}