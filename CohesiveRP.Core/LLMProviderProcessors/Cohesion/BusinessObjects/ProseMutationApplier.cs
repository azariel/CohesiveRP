using System.Text.RegularExpressions;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.Utils.Parsers.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderProcessors.Cohesion.BusinessObjects
{
    public static class ProseMutationApplier
    {
        private static readonly Regex RecRx = new(
            @"^\s*\[?(?<id>S\d+)\]?\s*:\s*(?<verb>DELETE|TRIM|REPLACE)\b\s*:?\s*(?<text>.*?)(?:\s+--\s+(?<reason>.*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RefRx = new(@"\bS(?<n>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Planning notes the writer leaked into the reply. A matching sentence only relaxes the guards;
        // the model still has to propose the delete.
        private static readonly Regex PlanRx = new(
            @"^\s*(-\s|I\s+(need|must|will|should|want)\b|I'll\b|I’ll\b|The user\b|The scene is\b|The characters?_)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PronounRx = new(@"^(He|She|They|It|His|Her|Their)\b", RegexOptions.Compiled);
        private static readonly Regex WordRx = new(@"[\p{L}']+", RegexOptions.Compiled);
        private static readonly char[] QuoteChars = { '"', '“', '”' };

        /// <summary>Returns the edited text, or null when nothing changed or the result looks gutted.</summary>
        public static string Apply(IdSentence[] sentences, IEnumerable<string> recommendations, List<string> rejected)
        {
            int originalLength = sentences.Sum(s => s.SentenceText.Length);
            int exemptChars = 0, applied = 0, nonPlanning = 0;
            int lastIdx = Array.FindLastIndex(sentences, s => !string.IsNullOrWhiteSpace(s.SentenceText));
            var touched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string rec in recommendations ?? Enumerable.Empty<string>())
            {
                var m = RecRx.Match(rec ?? "");
                if (!m.Success) { rejected.Add($"unparsable: {rec}"); continue; }

                string id = m.Groups["id"].Value.ToUpperInvariant();
                string verb = m.Groups["verb"].Value.ToUpperInvariant();
                string text = m.Groups["text"].Value.Trim().Trim('\'', '"', '`', '“', '”').Trim();
                string reason = m.Groups["reason"].Value;

                int idx = Array.FindIndex(sentences, s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (idx < 0) { rejected.Add($"{id}: unknown id"); continue; }

                var sentence = sentences[idx];
                if (sentence.IsLocked) { rejected.Add($"{id}: locked"); continue; }
                if (idx == lastIdx) { rejected.Add($"{id}: last sentence"); continue; }
                if (touched.Contains(id)) { rejected.Add($"{id}: already edited"); continue; }

                bool planning = PlanRx.IsMatch(sentence.SentenceText);
                if (!planning && nonPlanning >= 8) { rejected.Add($"{id}: too many edits"); continue; }

                switch (verb)
                {
                    case "DELETE":
                        if (!planning)
                        {
                            if (OrphansNext(sentences, idx))
                            { rejected.Add($"{id}: next sentence depends on it"); continue; }

                            var refMatch = RefRx.Match(reason);
                            if (refMatch.Success)
                            {
                                int refIdx = Array.FindIndex(sentences, s => s.Id.Equals("S" + refMatch.Groups["n"].Value, StringComparison.OrdinalIgnoreCase));
                                if (refIdx < 0 || refIdx >= idx || !OverlapsEnough(sentence.SentenceText, sentences[refIdx].SentenceText))
                                { rejected.Add($"{id}: claimed repeat does not overlap"); continue; }
                            }
                        } else exemptChars += sentence.SentenceText.Length;

                        sentence.SentenceText = "";
                        break;

                    case "REPLACE":
                        if (text.Length == 0 || text.Contains('\n') || text.IndexOfAny(QuoteChars) >= 0
                            || text.Length > sentence.SentenceText.Length
                            || IntroducesNewWords(sentence.SentenceText, text))
                        { rejected.Add($"{id}: invalid replacement"); continue; }

                        sentence.SentenceText = text;
                        break;

                    case "TRIM":
                        int pos = text.Length < 3 ? -1 : sentence.SentenceText.IndexOf(text, StringComparison.Ordinal);
                        if (pos < 0) { rejected.Add($"{id}: trim text not found"); continue; }

                        string r = sentence.SentenceText.Remove(pos, text.Length);
                        r = Regex.Replace(r, @"\s+", " ");
                        r = Regex.Replace(r, @"\s+([,.;:!?])", "$1").Trim();
                        r = Regex.Replace(r, @"[,;:]+$", "");
                        if (r.Length > 0 && !".!?…".Contains(r[^1])) r += ".";
                        if (r.Split(' ').Length < 3) { rejected.Add($"{id}: trim leaves too little"); continue; }

                        sentence.SentenceText = r;
                        break;
                }

                touched.Add(id);
                applied++;
                if (!planning) nonPlanning++;
            }

            if (applied == 0) return null;

            string result = SentenceSplitter.Reassemble(sentences);
            // Gutted-result floor, ignoring leaked planning notes (they can legitimately be most of the text).
            return !string.IsNullOrWhiteSpace(result) && result.Length >= (originalLength - exemptChars) * 0.5
                ? result : null;
        }

        private static bool OrphansNext(IdSentence[] all, int idx)
        {
            if (!all[idx].StartsNewParagraph) return false;
            var next = all.Skip(idx + 1).FirstOrDefault(s => s.SentenceText.Length > 0);
            return next != null && PronounRx.IsMatch(next.SentenceText);
        }

        // Shared 4+ letter words must cover at least half of the shorter sentence's words.
        private static bool OverlapsEnough(string a, string b)
        {
            var wa = WordRx.Matches(a.ToLowerInvariant()).Select(m => m.Value).Where(w => w.Length >= 4).ToHashSet();
            var wb = WordRx.Matches(b.ToLowerInvariant()).Select(m => m.Value).Where(w => w.Length >= 4).ToHashSet();
            if (wa.Count == 0 || wb.Count == 0) return false;
            int shared = wa.Count(wb.Contains);
            return shared >= Math.Max(1, (int)Math.Ceiling(Math.Min(wa.Count, wb.Count) * 0.5));
        }

        public static bool IntroducesNewWords(string original, string replacement, int minLength = 4, int allowed = 0)
        {
            string Norm(string s) => s.Replace('’', '\'').Replace('‘', '\'').ToLowerInvariant();
            var known = new HashSet<string>(WordRx.Matches(Norm(original)).Select(m => m.Value));
            return WordRx.Matches(Norm(replacement)).Select(m => m.Value)
                .Count(w => w.Length >= minLength && !known.Contains(w)) > allowed;
        }
    }
}
