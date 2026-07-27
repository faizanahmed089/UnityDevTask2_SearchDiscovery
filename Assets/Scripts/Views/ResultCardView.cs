using StoryLibrary.Models;
using UnityEngine;
using UnityEngine.UI;

namespace StoryLibrary.Views
{
    /// <summary>Displays Content Name, Author, and Date for one StoryItem.</summary>
    public class ResultCardView : MonoBehaviour
    {
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text authorLabel;
        [SerializeField] private Text dateLabel;

        public void Bind(StoryItem item)
        {
            nameLabel.text = item.ContentName;
            authorLabel.text = string.IsNullOrEmpty(item.Author) ? "Unknown author" : item.Author;
            dateLabel.text = item.Date.HasValue
                ? item.Date.Value.ToString("dd MMM yyyy")
                : "Date unavailable";
        }
    }
}