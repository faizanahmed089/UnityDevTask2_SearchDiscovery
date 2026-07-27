using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using StoryLibrary.Events;
using StoryLibrary.Models;
using StoryLibrary.Parsing;

namespace StoryLibrary.Data
{
    /// <summary>
    /// Model layer: owns the connection to Firebase, retrieval, parsing, and
    /// an in-memory cache. This is the only class that talks to Firebase —
    /// everything else (Controller, Views) only ever sees List&lt;StoryItem&gt;.
    /// </summary>
    public class StoryRepository
    {
        private DatabaseReference _dbRoot;
        private List<StoryItem> _cache;
        public bool IsLoaded => _cache != null;

        public StoryRepository()
        {
            _dbRoot = FirebaseDatabase.DefaultInstance.RootReference;
        }

        /// <summary>
        /// Fetches the whole StoryLibrary node once, parses every CoverInfo,
        /// and caches the result in memory for the session so repeat searches
        /// (and re-opening the search page) don't re-hit the network.
        /// </summary>
        public async Task<List<StoryItem>> LoadAllAsync(bool forceRefresh = false)
        {
            if (_cache != null && !forceRefresh)
                return _cache;

            StoryEvents.RaiseLoadStarted();

            try
            {
                DataSnapshot snapshot = await _dbRoot.Child("StoryLibary").GetValueAsync();

                var results = new List<StoryItem>(capacity: (int)(snapshot.ChildrenCount));

                foreach (var storyNode in snapshot.Children)
                {
                    string id = storyNode.Key;
                    var storyChild = storyNode.Child("Story");
                    if (storyChild == null || !storyChild.Exists) continue;

                    // The CoverInfo key is prefixed with the story id, e.g. "71726852CoverInfo"
                    var coverInfoChild = storyChild.Child(id + "CoverInfo");
                    string raw = coverInfoChild.Exists ? coverInfoChild.Value?.ToString() : null;

                    results.Add(CoverInfoParser.Parse(id, raw));
                }

                _cache = results;
                StoryEvents.RaiseLoadCompleted(results.Count);
                return results;
            }
            catch (Exception ex)
            {
                StoryEvents.RaiseLoadFailed(ex.Message);
                throw;
            }
        }

        public void InvalidateCache() => _cache = null;
    }
}