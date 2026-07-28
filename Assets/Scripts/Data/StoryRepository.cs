using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using StoryLibrary.Events;
using StoryLibrary.Models;
using StoryLibrary.Parsing;

namespace StoryLibrary.Data
{
    /// <summary>
    /// Model layer: talks to Firebase Realtime Database over its plain REST API
    /// (UnityWebRequest), not the native Firebase Unity SDK. The official SDK
    /// does not support WebGL at all (only iOS/Android/tvOS/Desktop), so REST
    /// is the approach that actually works on every platform, including the
    /// WebGL build this task requires. Caches results in memory for the session.
    /// </summary>
    public class StoryRepository
    {
        // Your Realtime Database URL, from the Firebase console's Realtime
        // Database page (top of the page, looks like:
        // "https://your-project-id-default-rtdb.firebaseio.com" or
        // "...-default-rtdb.<region>.firebasedatabase.app").
        // No trailing slash.
        private const string DatabaseUrl = "https://unity-task2-search-default-rtdb.firebaseio.com/";

        private List<StoryItem> _cache;
        public bool IsLoaded => _cache != null;

        /// <summary>
        /// Fetches the whole StoryLibary node once via a single REST GET,
        /// parses every CoverInfo, and caches the result for the session.
        /// </summary>
        public async Task<List<StoryItem>> LoadAllAsync(bool forceRefresh = false)
        {
            if (_cache != null && !forceRefresh)
                return _cache;

            StoryEvents.RaiseLoadStarted();

            string url = $"{DatabaseUrl}/StoryLibary.json";

            using (var request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    StoryEvents.RaiseLoadFailed(request.error);
                    throw new Exception(request.error);
                }

                var results = new List<StoryItem>();
                JObject root = JObject.Parse(request.downloadHandler.text);

                foreach (var storyEntry in root.Properties())
                {
                    string id = storyEntry.Name;
                    JToken storyNode = storyEntry.Value["Story"];
                    if (storyNode == null) continue;

                    string raw = storyNode[id + "CoverInfo"]?.ToString();
                    results.Add(CoverInfoParser.Parse(id, raw));
                }

                _cache = results;
                StoryEvents.RaiseLoadCompleted(results.Count);
                return results;
            }
        }

        public void InvalidateCache() => _cache = null;
    }
}