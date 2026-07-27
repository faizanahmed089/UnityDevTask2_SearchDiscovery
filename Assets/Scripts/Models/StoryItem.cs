using System;

namespace StoryLibrary.Models
{
    /// <summary>
    /// Clean, strongly-typed representation of one story entry,
    /// produced by parsing the raw "CoverInfo" string from Firebase.
    /// This is the ONLY shape the rest of the app (search, UI) should touch —
    /// nobody downstream should ever see the raw pipe/underscore string again.
    /// </summary>
    [Serializable]
    public class StoryItem
    {
        public string Id;              // Firebase key, e.g. "71726852"
        public string ContentName;     // e.g. "الكسور"
        public string Author;          // e.g. "amna"  (may be empty)
        public DateTime? Date;         // parsed from dd/MM/yyyy, null if missing/invalid
        public string RawDate;         // original date string, kept for display/debug
        public string Category;        // "Education" | "UserContent" | "Other" | ...
        public string Subject;         // e.g. "Math" (first pipe segment of subcategory)
        public string Grade;           // e.g. "Three" (second pipe segment)
        public string Term;            // e.g. "Second" (third pipe segment)
        public string Tags;            // free-text trailing segment, may contain underscores

        // Precomputed, lowercased, whitespace-trimmed search fields.
        // Built once at load time so Search() never re-normalizes strings per query.
        public string SearchNameKey;
        public string SearchAuthorKey;

        public bool IsMalformed; // true if parsing hit unexpected/incomplete data
    }
}