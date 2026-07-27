using System;
using System.Collections.Generic;
using StoryLibrary.Models;

namespace StoryLibrary.Events
{
    /// <summary>
    /// Static event bus decoupling the Model (data/search) from Views (UI).
    /// This is the "Event-Driven" half of the required architecture:
    /// Controller raises events, any number of Views can subscribe without
    /// the Controller knowing they exist.
    /// </summary>
    public static class StoryEvents
    {
        public static event Action OnLoadStarted;
        public static event Action<int> OnLoadCompleted;      // total item count
        public static event Action<string> OnLoadFailed;      // error message
        public static event Action<List<StoryItem>> OnSearchResults;

        public static void RaiseLoadStarted() => OnLoadStarted?.Invoke();
        public static void RaiseLoadCompleted(int count) => OnLoadCompleted?.Invoke(count);
        public static void RaiseLoadFailed(string error) => OnLoadFailed?.Invoke(error);
        public static void RaiseSearchResults(List<StoryItem> results) => OnSearchResults?.Invoke(results);
    }
}