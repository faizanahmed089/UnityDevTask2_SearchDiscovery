using ArabicSupport;
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
            // Fixed every time Bind() runs (i.e. every time this pooled card is
            // reused for a new item) — NOT once in Start(), which only ever
            // catches whatever text happened to be set the very first time.
            nameLabel.text = ArabicFixer.Fix(item.ContentName, showTashkeel: true, useHinduNumbers: false);

            string author = string.IsNullOrEmpty(item.Author) ? "Unknown author" : item.Author;
            authorLabel.text = ArabicFixer.Fix(author, showTashkeel: true, useHinduNumbers: false);

            dateLabel.text = item.Date.HasValue
                ? item.Date.Value.ToString("dd MMM yyyy")
                : "Date unavailable";
        }
    }
}