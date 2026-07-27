using System;
using System.Collections.Generic;
using System.Linq;
using StoryLibrary.Models;

namespace StoryLibrary.Data
{
    /// <summary>
    /// Builds a searchable index once during load, allowing fast searches by using
    /// sorted arrays with binary search instead of scanning the entire StoryItems list on every keystroke.
    /// </summary>
    public class SearchIndex
    {
        private StoryItem[] _byName;
        private StoryItem[] _byAuthor;

        public void Build(List<StoryItem> items)
        {
            _byName = items.OrderBy(i => i.SearchNameKey, StringComparer.Ordinal).ToArray();
            _byAuthor = items.OrderBy(i => i.SearchAuthorKey, StringComparer.Ordinal).ToArray();
        }

        public List<StoryItem> Search(string query, int maxResults = 200)
        {
            if (string.IsNullOrWhiteSpace(query) || _byName == null)
                return new List<StoryItem>();

            string key = query.Trim().ToLowerInvariant();
            var results = new List<StoryItem>();
            var seen = new HashSet<string>();

            CollectMatches(_byName, i => i.SearchNameKey, key, results, seen, maxResults);
            CollectMatches(_byAuthor, i => i.SearchAuthorKey, key, results, seen, maxResults);

            return results;
        }

        private void CollectMatches(
            StoryItem[] sorted,
            Func<StoryItem, string> keySelector,
            string query,
            List<StoryItem> results,
            HashSet<string> seen,
            int maxResults)
        {
            if (results.Count >= maxResults) return;

            // Binary search for the first entry whose key could start with `query`.
            int lo = 0, hi = sorted.Length - 1, start = sorted.Length;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (string.CompareOrdinal(keySelector(sorted[mid]), query) >= 0)
                {
                    start = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            for (int i = start; i < sorted.Length && results.Count < maxResults; i++)
            {
                string k = keySelector(sorted[i]);
                if (k.StartsWith(query, StringComparison.Ordinal))
                {
                    AddIfNew(sorted[i], results, seen);
                }
                else if (!k.Contains(query))
                {
                    break; // prefix run ended
                }
            }

            // Contains-anywhere fallback (bounded pass) for substrings that
            // aren't prefixes — keeps behavior correct, not just fast.
            for (int i = 0; i < sorted.Length && results.Count < maxResults; i++)
            {
                if (keySelector(sorted[i]).Contains(query))
                    AddIfNew(sorted[i], results, seen);
            }
        }

        private void AddIfNew(StoryItem item, List<StoryItem> results, HashSet<string> seen)
        {
            if (seen.Add(item.Id))
                results.Add(item);
        }
    }
}