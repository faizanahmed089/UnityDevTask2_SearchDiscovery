using System.Collections.Generic;
using StoryLibrary.Controllers;
using StoryLibrary.Events;
using StoryLibrary.Models;
using UnityEngine;
using UnityEngine.UI;

namespace StoryLibrary.Views
{
    /// <summary>
    /// View layer (MVC): pure presentation. Reads user input, forwards it to
    /// the Controller, and reacts to StoryEvents to render results/loading/error
    /// states. Contains no search or parsing logic itself.
    /// </summary>
    public class SearchView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private SearchController controller;
        [SerializeField] private InputField queryInput;
        [SerializeField] private Button searchButton;

        [Header("States")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text errorText;
        [SerializeField] private GameObject emptyStateLabel;

        [Header("Results")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private ResultCardView cardPrefab;

        private readonly List<ResultCardView> _pool = new List<ResultCardView>();

        private void OnEnable()
        {
            StoryEvents.OnLoadStarted += HandleLoadStarted;
            StoryEvents.OnLoadCompleted += HandleLoadCompleted;
            StoryEvents.OnLoadFailed += HandleLoadFailed;
            StoryEvents.OnSearchResults += HandleSearchResults;

            queryInput.onValueChanged.AddListener(controller.OnQueryChanged);
            searchButton.onClick.AddListener(() => controller.OnSearchButtonPressed(queryInput.text));
        }

        private void OnDisable()
        {
            StoryEvents.OnLoadStarted -= HandleLoadStarted;
            StoryEvents.OnLoadCompleted -= HandleLoadCompleted;
            StoryEvents.OnLoadFailed -= HandleLoadFailed;
            StoryEvents.OnSearchResults -= HandleSearchResults;
        }

        private void HandleLoadStarted()
        {
            loadingIndicator.SetActive(true);
            errorPanel.SetActive(false);
        }

        private void HandleLoadCompleted(int count)
        {
            loadingIndicator.SetActive(false);
        }

        private void HandleLoadFailed(string message)
        {
            loadingIndicator.SetActive(false);
            errorPanel.SetActive(true);
            errorText.text = $"Couldn't load the library. {message}";
        }

        private void HandleSearchResults(List<StoryItem> results)
        {
            emptyStateLabel.SetActive(results.Count == 0);

            // Simple object pool so scrolling/re-searching doesn't Instantiate/Destroy repeatedly.
            for (int i = 0; i < results.Count; i++)
            {
                ResultCardView card = i < _pool.Count ? _pool[i] : CreateCard();
                card.gameObject.SetActive(true);
                card.Bind(results[i]);
            }
            for (int i = results.Count; i < _pool.Count; i++)
            {
                _pool[i].gameObject.SetActive(false);
            }
        }

        private ResultCardView CreateCard()
        {
            var card = Instantiate(cardPrefab, resultsContainer);
            _pool.Add(card);
            return card;
        }
    }
}