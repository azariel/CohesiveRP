using System.Text;
using System.Text.RegularExpressions;
using CohesiveRP.Common.Utils.Parsers.BusinessObjects;

namespace CohesiveRP.Common.Utils
{
    public static class SentenceSplitter
    {
        private static readonly HashSet<string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "mr", "mrs", "ms", "dr", "st", "sr", "jr", "prof", "vs"
    };

        public static string RenderForPrompt(IEnumerable<IdSentence> sentences) => string.Join("\n", sentences.Select(s => $"[{s.Id}]{(s.IsLocked ? " [LOCKED]" : "")} {s.SentenceText}"));

        public static string Reassemble(IEnumerable<IdSentence> sentences)
        {
            var sb = new StringBuilder();
            bool pendingParagraph = false;

            foreach (var s in sentences)
            {
                if (s.StartsNewParagraph) pendingParagraph = true;   // survives even if this sentence was deleted
                if (string.IsNullOrWhiteSpace(s.SentenceText)) continue; // deleted

                if (sb.Length > 0) sb.Append(pendingParagraph ? "\n\n" : " ");
                sb.Append(s.SentenceText.Trim());
                pendingParagraph = false;
            }
            return sb.ToString();
        }

        public static IdSentence[] SplitIntoIdSentences(string rawInputText)
        {
            var result = new List<IdSentence>();
            if (string.IsNullOrWhiteSpace(rawInputText))
                return result.ToArray();

            string text = rawInputText.Replace("\r\n", "\n").Replace('\r', '\n');

            // Every newline run is a paragraph break.
            foreach (string rawParagraph in Regex.Split(text, @"\n+"))
            {
                string paragraph = rawParagraph.Trim();
                if (paragraph.Length == 0) continue;

                bool first = true;
                foreach (string sentence in SplitParagraph(paragraph))
                {
                    result.Add(new IdSentence
                    {
                        Id = $"S{result.Count + 1}",
                        SentenceText = sentence,
                        StartsNewParagraph = first
                    });
                    first = false;
                }
            }

            return result.ToArray();
        }

        private static List<string> SplitParagraph(string p)
        {
            var sentences = new List<string>();
            var sb = new StringBuilder();
            bool inQuote = false;
            int i = 0;

            while (i < p.Length)
            {
                char c = p[i];
                sb.Append(c);

                bool isCandidate = false;

                if (IsDoubleQuote(c))
                {
                    inQuote = c switch { '“' => true, '”' => false, _ => !inQuote };
                    // A closing quote right after a terminator ("No." / "Really?") can end the sentence.
                    isCandidate = !inQuote && PrecededByTerminator(p, i);
                } else if (!inQuote && IsTerminator(c))
                {
                    isCandidate = true;
                }

                if (!isCandidate)
                {
                    i++;
                    continue;
                }

                // Swallow trailing terminators and closing characters: ...  ?!  ."  .*  .)
                int j = i + 1;
                while (j < p.Length && (IsTerminator(p[j]) || IsCloser(p[j])))
                {
                    sb.Append(p[j]);
                    if (IsDoubleQuote(p[j]))
                        inQuote = p[j] switch { '“' => true, '”' => false, _ => !inQuote };
                    j++;
                }

                if (IsBoundary(p, i, j, inQuote))
                {
                    AddSentence(sentences, sb);
                }

                i = j;
            }

            AddSentence(sentences, sb);
            return sentences;
        }

        private static bool IsBoundary(string p, int i, int j, bool inQuote)
        {
            if (inQuote) return false;

            // Must be followed by whitespace or the end of the paragraph ("3.5" stays together).
            if (j < p.Length && !char.IsWhiteSpace(p[j])) return false;

            // Abbreviations such as "Dr." or "Mrs."
            if (p[i] == '.' && (i + 1 >= p.Length || p[i + 1] != '.') && IsAbbreviation(p, i))
                return false;

            // Look at the next visible character.
            int k = j;
            while (k < p.Length && char.IsWhiteSpace(p[k])) k++;
            if (k >= p.Length) return true;

            // Lowercase continuation: "Hmm..." she said / "No!" he said / "...there is"
            return !char.IsLower(p[k]);
        }

        private static bool IsAbbreviation(string p, int dotIndex)
        {
            int start = dotIndex;
            while (start > 0 && char.IsLetter(p[start - 1])) start--;
            string word = p.Substring(start, dotIndex - start);
            return word.Length > 0 && Abbreviations.Contains(word);
        }

        private static bool PrecededByTerminator(string p, int quoteIndex)
        {
            int k = quoteIndex - 1;
            while (k >= 0 && (p[k] == '*' || p[k] == '_' || p[k] == ')' || p[k] == ']')) k--;
            return k >= 0 && IsTerminator(p[k]);
        }

        private static void AddSentence(List<string> sentences, StringBuilder sb)
        {
            string s = sb.ToString().Trim();
            if (s.Length > 0) sentences.Add(s);
            sb.Clear();
        }

        private static bool IsTerminator(char c) => c is '.' or '!' or '?' or '…';
        private static bool IsDoubleQuote(char c) => c is '"' or '“' or '”';
        private static bool IsCloser(char c) => c is '"' or '”' or '’' or '\'' or '*' or '_' or ')' or ']';
    }
}
