using Data;
using TMPro;
using UI.ViewSystem;
using UI.ViewSystem.UIViews;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.CustomScrollRect.Items
{
    /// <summary>
    /// Displays a quiz entry inside the list and opens the quiz screen when selected.
    /// </summary>
    public sealed class QuizListItemView : BaseItemView, IIndexedScrollItem
    {
        [SerializeField] private Button openButton;
        // [SerializeField] private TMP_Text questionsCountLabel;

        private Quiz _quiz;
        private int _dataIndex;
        private QuizScreenUIView _quizScreen;
        private IUIViewController _viewController;

        [Inject]
        private void Construct(IUIViewController viewController)
        {
            _viewController = viewController;
        }

        private void Awake()
        {
            _quizScreen = FindObjectOfType<QuizScreenUIView>(includeInactive: true);
            if (openButton)
                openButton.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (openButton)
                openButton.onClick.RemoveListener(HandleClicked);
        }

        public override void Bind<T>(T data)
        {
            if (data is Quiz quiz)
            {
                _quiz = quiz;
                UpdateLabels();
            }
        }

        public void SetDataIndex(int index)
        {
            _dataIndex = Mathf.Max(0, index);
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (_quiz == null)
                return;

            var displayIndex = _dataIndex + 1;
            if (title)
                title.text = $"QUIZ #{displayIndex}";

            // if (questionsCountLabel)
            // {
            //     var count = _quiz.Questions?.Length ?? 0;
            //     questionsCountLabel.text = count > 0
            //         ? $"{count} questions"
            //         : string.Empty;
            // }
        }

        private void HandleClicked()
        {
            if (_quiz == null || _viewController == null)
                return;

            if (_quizScreen == null)
                _quizScreen = FindObjectOfType<QuizScreenUIView>(includeInactive: true);

            if (_quizScreen == null)
                return;

            _quizScreen.Initialize(_quiz, _dataIndex + 1);
            _viewController.ShowScreen(UIScreenId.QuizScreen);
        }
    }
}
