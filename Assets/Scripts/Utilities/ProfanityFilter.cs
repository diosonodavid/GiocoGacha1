using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GachaGame.Utilities
{
    // Denylist-based censor: whole-word, case-insensitive matches get replaced with asterisks of
    // the same length. This is meant to catch plain profanity in chat/usernames, not to defeat
    // deliberate evasion (leetspeak, spacing tricks) - a heavier normalization pass would be
    // needed for that and isn't required by the current chat/naming use cases.
    public class ProfanityFilter
    {
        private static readonly string[] DefaultBannedWords = { "damn", "hell", "crap" };

        private readonly List<Regex> patterns = new();

        public ProfanityFilter(IEnumerable<string> bannedWords = null)
        {
            foreach (var word in bannedWords ?? DefaultBannedWords)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                patterns.Add(new Regex($@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase));
            }
        }

        public bool ContainsProfanity(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            foreach (var pattern in patterns)
                if (pattern.IsMatch(text)) return true;

            return false;
        }

        public string Censor(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            foreach (var pattern in patterns)
                result = pattern.Replace(result, match => new string('*', match.Length));

            return result;
        }
    }
}
