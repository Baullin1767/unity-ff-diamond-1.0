using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ViewSystem.UIViews.Popups
{
    /// <summary>
    /// Popup that displays the quiz completion summary.
    /// </summary>
    public sealed class QuizResultsPopupUIView : PopupUIView
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private TMP_Text ratioLabel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button quizzesMenuButton;
        [SerializeField] private string rewardFormat = "{0:N0}";
        [SerializeField] private string ratioFormat = "{0}";

        private QuizResultContext _context;
        private Action _retryAction;
        private Action _menuAction;

        private void Awake()
        {
            if (retryButton)
                retryButton.onClick.AddListener(HandleRetryClicked);
            if (quizzesMenuButton)
                quizzesMenuButton.onClick.AddListener(HandleMenuClicked);
        }

        private void OnDestroy()
        {
            if (retryButton)
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            if (quizzesMenuButton)
                quizzesMenuButton.onClick.RemoveListener(HandleMenuClicked);
        }

        public void SetContext(in QuizResultContext context)
        {
            _context = context;
        }

        public void ConfigureActions(Action retryAction, Action menuAction)
        {
            _retryAction = retryAction;
            _menuAction = menuAction;

            if (retryButton)
                retryButton.interactable = retryAction != null;
            if (quizzesMenuButton)
                quizzesMenuButton.interactable = menuAction != null;
        }

        public override void Show(float reward, string ratio)
        {
            if (popupRoot)
                popupRoot.SetActive(true);

            if (rewardLabel)
                rewardLabel.text = string.Format(rewardFormat, reward);

            if (ratioLabel)
                ratioLabel.text = string.Format(ratioFormat, ratio);

            if (titleLabel)
                titleLabel.text = _context.Title ?? string.Empty;

            if (descriptionLabel)
                descriptionLabel.text = _context.Description ?? string.Empty;
        }

        public override void Hide()
        {
            if (popupRoot)
                popupRoot.SetActive(false);

            ConfigureActions(null, null);
        }

        private void HandleRetryClicked() => _retryAction?.Invoke();

        private void HandleMenuClicked() => _menuAction?.Invoke();

        public readonly struct QuizResultContext
        {
            public QuizResultContext(string title, string description, Sprite image)
            {
                Title = title;
                Description = description;
                Image = image;
            }

            public string Title { get; }
            public string Description { get; }
            public Sprite Image { get; }
        }
    }
}
