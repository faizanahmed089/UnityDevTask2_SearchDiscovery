using System.Threading.Tasks;
using StoryLibrary.Data;
using StoryLibrary.Events;
using UnityEngine;

namespace StoryLibrary.Controllers
{
    /// <summary>
    /// MVC Controller: mediates between the Model (StoryRepository/SearchIndex)
    /// and Views (SearchView, ResultCardView etc). Views never touch the
    /// repository directly; they call the Controller and listen to StoryEvents.
    /// </summary>
    public class SearchController : MonoBehaviour
    {
        private StoryRepository _repository;
        private SearchIndex _index;

        private async void Awake()
        {
            _repository = new StoryRepository();
            _index = new SearchIndex();
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var items = await _repository.LoadAllAsync();
            _index.Build(items);
        }

        /// <summary>Call this from the Search button's OnClick (or Enter key) for an explicit search.</summary>
        public void OnSearchButtonPressed(string query)
        {
            RunSearch(query);
        }

        private void RunSearch(string query)
        {
            var results = _index.Search(query);
            StoryEvents.RaiseSearchResults(results);
        }
    }
}