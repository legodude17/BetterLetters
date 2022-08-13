using System;
using Verse;

namespace BetterLetters
{
    public static class LetterUtilities
    {
        public static void ClearAll(this LetterStack stack)
        {
            var letters = stack.LettersListForReading.ListFullCopy();
            letters.ForEach(stack.RemoveLetter);
        }

        public static void ClearAll(this LetterStack stack, Predicate<Letter> shouldRemove)
        {
            var letters = stack.LettersListForReading.ListFullCopy();
            letters.ForEach(letter =>
            {
                if (shouldRemove(letter)) stack.RemoveLetter(letter);
            });
        }
    }
}