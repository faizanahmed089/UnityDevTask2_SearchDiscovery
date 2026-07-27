using System;
using System.Globalization;
using System.Text.RegularExpressions;
using StoryLibrary.Models;

namespace StoryLibrary.Parsing
{
    /// <summary>
    /// Parses the "{id}CoverInfo" string into a StoryItem. Uses the date (DD/MM/YYYY) as the split anchor
    /// instead of counting underscores, making it resilient to missing fields, embedded underscores, whitespace, and null/empty values.
    /// </summary>
    public static class CoverInfoParser
    {
        private static readonly Regex DatePattern =
            new Regex(@"\d{2}/\d{2}/\d{4}", RegexOptions.Compiled);

        public static StoryItem Parse(string id, string rawCoverInfo)
        {
            var item = new StoryItem { Id = id };

            if (string.IsNullOrWhiteSpace(rawCoverInfo))
            {
                item.IsMalformed = true;
                item.ContentName = "(untitled)";
                item.Author = "";
                FinalizeSearchKeys(item);
                return item;
            }

            var dateMatch = DatePattern.Match(rawCoverInfo);

            string prefix;  // Name_Author
            string suffix;  // Category_Subject|Grade|Term_Tags

            if (dateMatch.Success)
            {
                prefix = rawCoverInfo.Substring(0, dateMatch.Index).TrimEnd('_');
                suffix = rawCoverInfo.Substring(dateMatch.Index + dateMatch.Length).TrimStart('_');

                item.RawDate = dateMatch.Value;
                if (DateTime.TryParseExact(dateMatch.Value, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    item.Date = parsedDate;
                }
                else
                {
                    item.IsMalformed = true; // matched shape but not a real calendar date
                }
            }
            else
            {
                // No recognizable date at all — data is more malformed than usual.
                // Fall back to treating the whole string as Name_Author, best effort.
                item.IsMalformed = true;
                var firstSplit = SplitFirst(rawCoverInfo, '_');
                prefix = rawCoverInfo;
                suffix = "";
            }

            // --- Name / Author ---
            var nameAuthor = SplitFirst(prefix, '_');
            item.ContentName = Clean(nameAuthor.head, "(untitled)");
            item.Author = Clean(nameAuthor.tail, "");

            // --- Category / Subject|Grade|Term / Tags ---
            if (!string.IsNullOrEmpty(suffix))
            {
                var catRest = SplitFirst(suffix, '_');
                item.Category = Clean(catRest.head, "Uncategorized");

                if (!string.IsNullOrEmpty(catRest.tail))
                {
                    var subRest = SplitFirst(catRest.tail, '_');
                    var pipeParts = subRest.head.Split('|');

                    item.Subject = pipeParts.Length > 0 ? Clean(pipeParts[0], "") : "";
                    item.Grade = pipeParts.Length > 1 ? Clean(pipeParts[1], "") : "";
                    item.Term = pipeParts.Length > 2 ? Clean(pipeParts[2], "") : "";

                    // Whatever remains (may itself legitimately contain underscores,
                    // e.g. "#مصادر_الصبيخي") is kept whole as free-text tags.
                    item.Tags = Clean(subRest.tail, "");
                }
            }
            else
            {
                item.Category = "Uncategorized";
            }

            FinalizeSearchKeys(item);
            return item;
        }

        private static void FinalizeSearchKeys(StoryItem item)
        {
            item.SearchNameKey = Normalize(item.ContentName);
            item.SearchAuthorKey = Normalize(item.Author);
        }

        /// <summary>Lowercase + trim, so search matching never redoes this per keystroke.</summary>
        private static string Normalize(string s) =>
            string.IsNullOrEmpty(s) ? "" : s.Trim().ToLowerInvariant();

        private static string Clean(string s, string fallbackIfEmpty) =>
            string.IsNullOrWhiteSpace(s) ? fallbackIfEmpty : s.Trim();

        private static (string head, string tail) SplitFirst(string s, char sep)
        {
            if (string.IsNullOrEmpty(s)) return ("", "");
            int idx = s.IndexOf(sep);
            return idx < 0 ? (s, "") : (s.Substring(0, idx), s.Substring(idx + 1));
        }
    }
}