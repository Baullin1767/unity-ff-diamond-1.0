using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using TMPro;
using UI.Minigames.Currency;
using UI.ViewSystem.UIViews.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews
{
    /// <summary>
    /// Handles the quiz gameplay loop: showing questions, capturing answers, and triggering the results popup.
    /// </summary>
    public sealed class QuizScreenUIView : UIView
    {
        [Header("Root")]
        [SerializeField] private GameObject rootGO;

        [Header("Header")]
        [SerializeField] private TMP_Text progressLabel;

        [Header("Question")]
        [SerializeField] private Image questionImage;
        [SerializeField] private TMP_Text questionText;

        [Header("Answers")]
        [SerializeField] private AnswerOption[] answerOptions = Array.Empty<AnswerOption>();
        [SerializeField] private Sprite defaultAnswerSprite;
        [SerializeField] private Sprite correctAnswerSprite;
        [SerializeField] private Sprite incorrectAnswerSprite;

        [Header("Controls")]
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextButtonLabel;
        [SerializeField] private string nextLabel = "NEXT";
        [SerializeField] private string finishLabel = "FINISH";

        [Header("Rewards & Popup")]
        [SerializeField] private int rewardPerCorrectAnswer = 1000;
        [SerializeField] private int completionBonus;
        [SerializeField] private QuizResultsPopupUIView resultsPopup;

        private Quiz _quiz;
        private int _quizDisplayIndex = 1;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private bool _acceptingAnswers;
        private CancellationTokenSource _resultsCts;

        [Inject] private IMinigameCurrencyService _currencyService;
        [Inject] private IUIViewController _viewController;

        private void Awake()
        {
            if (nextButton)
            {
                nextButton.onClick.AddListener(HandleNextButtonClicked);
                nextButton.interactable = false;
            }

            for (var i = 0; i < answerOptions.Length; i++)
            {
                var index = i;
                var option = answerOptions[i];
                if (option.Button)
                    option.Button.onClick.AddListener(() => HandleAnswerClicked(index));
            }
        }

        private void OnDestroy()
        {
            if (nextButton)
                nextButton.onClick.RemoveListener(HandleNextButtonClicked);

            foreach (var option in answerOptions)
            {
                if (option.Button)
                    option.Button.onClick.RemoveAllListeners();
            }

            CancelPendingLoads();
        }

        public override void Show()
        {
            if (rootGO)
                rootGO.SetActive(true);
        }

        public override void Hide()
        {
            if (rootGO)
                rootGO.SetActive(false);
        }

        /// <summary>
        /// Prepares the screen to run through the provided quiz instance.
        /// </summary>
        public void Initialize(Quiz quiz, int displayIndex)
        {
            _quiz = quiz;
            _quizDisplayIndex = Mathf.Max(1, displayIndex);
            ResetQuizState();
        }

        private void ResetQuizState()
        {
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            CancelPendingLoads();
            ShowQuestion();
        }

        private void RestartQuiz()
        {
            if (_quiz == null)
                return;

            ResetQuizState();
        }

        private void ShowQuestion()
        {
            if (_quiz?.Questions == null || _quiz.Questions.Length == 0)
            {
                if (questionText)
                    questionText.text = string.Empty;
                if (progressLabel)
                    progressLabel.text = string.Empty;
                if (nextButton)
                    nextButton.interactable = false;
                return;
            }

            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _quiz.Questions.Length)
                _currentQuestionIndex = 0;

            var question = _quiz.Questions[_currentQuestionIndex];

            if (progressLabel)
                progressLabel.text = $"{_currentQuestionIndex + 1}/{_quiz.Questions.Length} questions";

            if (questionText)
                questionText.text = question.question;

            ResetAnswerButtons();

            var answers = question.answers ?? Array.Empty<Quiz.Question.Answer>();
            for (var i = 0; i < answerOptions.Length; i++)
            {
                var option = answerOptions[i];
                if (!option.Button)
                    continue;

                if (i < answers.Length)
                {
                    option.SetVisible(true);
                    option.SetLabel(answers[i].aVariantOfTheAnswers);
                    option.SetState(AnswerVisualState.Default, defaultAnswerSprite, correctAnswerSprite, incorrectAnswerSprite);
                    option.Button.interactable = true;
                }
                else
                {
                    option.SetVisible(false);
                }
            }

            if (nextButton)
            {
                nextButton.interactable = false;
                if (nextButtonLabel)
                    nextButtonLabel.text = _currentQuestionIndex == _quiz.Questions.Length - 1 ? finishLabel : nextLabel;
            }

            _acceptingAnswers = true;
        }

        private void ResetAnswerButtons()
        {
            foreach (var option in answerOptions)
            {
                option.Button.interactable = true;
                option.SetState(AnswerVisualState.Default, defaultAnswerSprite, correctAnswerSprite, incorrectAnswerSprite);
                option.SetVisible(true);
            }
        }

        private void HandleAnswerClicked(int index)
        {
            if (!_acceptingAnswers || _quiz?.Questions == null)
                return;

            var question = _quiz.Questions[_currentQuestionIndex];
            if (question?.answers == null || index < 0 || index >= question.answers.Length)
                return;

            _acceptingAnswers = false;
            var answer = question.answers[index];
            bool isCorrect = answer.answer;
            if (isCorrect)
                _correctAnswers++;

            for (var i = 0; i < answerOptions.Length; i++)
            {
                var option = answerOptions[i];
                if (option.Button)
                {
                    option.Button.interactable = false;
                    if (i == index)
                        option.SetState(isCorrect ? AnswerVisualState.Correct : AnswerVisualState.Incorrect, defaultAnswerSprite, correctAnswerSprite, incorrectAnswerSprite);
                }
            }

            if (nextButton)
                nextButton.interactable = true;
        }

        private void HandleNextButtonClicked()
        {
            if (_quiz?.Questions == null || _quiz.Questions.Length == 0)
                return;

            if (_currentQuestionIndex < _quiz.Questions.Length - 1)
            {
                _currentQuestionIndex++;
                ShowQuestion();
            }
            else
            {
                CompleteQuizAsync().Forget();
            }
        }

        private async UniTaskVoid CompleteQuizAsync()
        {
            CancelPendingLoads();
            _resultsCts = new CancellationTokenSource();

            var totalQuestions = _quiz?.Questions?.Length ?? 0;
            var totalReward = Mathf.Max(0, _correctAnswers * rewardPerCorrectAnswer + completionBonus);

            if (totalReward > 0 && _currencyService != null)
                _currencyService.Add(totalReward);

            if (resultsPopup)
            {
                QuizResultsPopupUIView.QuizResultContext context = default;
                if (_quiz?.ResultsList != null && _quiz.ResultsList.Length > 0)
                {
                    var definition = ResolveResultDefinition(_correctAnswers);
                    var sprite = await LoadResultSpriteAsync(definition?.image, _resultsCts.Token);
                    context = new QuizResultsPopupUIView.QuizResultContext(
                        definition?.name,
                        definition?.desc,
                        sprite);
                }

                resultsPopup.SetContext(context);
                resultsPopup.ConfigureActions(
                    () =>
                    {
                        _viewController?.HidePopup(UIPopupId.QuizResults);
                        RestartQuiz();
                    },
                    () =>
                    {
                        _viewController?.HidePopup(UIPopupId.QuizResults);
                        _viewController?.ShowScreen(UIScreenId.QuizzesListScreen);
                    });
            }

            var ratio = totalQuestions > 0 ? $"{_correctAnswers}/{totalQuestions}" : _correctAnswers.ToString();
            _viewController?.ShowPopup(UIPopupId.QuizResults, totalReward, ratio);
        }

        private Quiz.Results ResolveResultDefinition(int correctAnswers)
        {
            if (_quiz?.ResultsList == null || _quiz.ResultsList.Length == 0)
                return null;

            Quiz.Results best = null;
            var bestScore = int.MinValue;

            foreach (var result in _quiz.ResultsList)
            {
                if (result == null)
                    continue;

                if (TryParseScore(result.score, out var scoreValue))
                {
                    if (correctAnswers >= scoreValue && scoreValue >= bestScore)
                    {
                        bestScore = scoreValue;
                        best = result;
                    }
                }
            }

            if (best != null)
                return best;

            var clamped = Mathf.Clamp(correctAnswers, 0, _quiz.ResultsList.Length - 1);
            return _quiz.ResultsList[clamped];
        }

        private static bool TryParseScore(string score, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(score))
                return false;

            if (int.TryParse(score, out value))
                return true;

            var digits = new string(score.Where(char.IsDigit).ToArray());
            return digits.Length > 0 && int.TryParse(digits, out value);
        }

        private async UniTask<Sprite> LoadResultSpriteAsync(string relative, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(relative))
                return null;

            var path = $"{PathBuilder.GetBasePath(DataType.Quiz)}/{relative}";
            return await DataManager.GetSprite(path, token);
        }

        private void CancelPendingLoads()
        {
            _resultsCts?.Cancel();
            _resultsCts?.Dispose();
            _resultsCts = null;
        }

        [Serializable]
        private struct AnswerOption
        {
            [SerializeField] private Button button;
            [SerializeField] private TMP_Text label;
            [SerializeField] private GameObject root;
            [SerializeField] private Image stateImage;

            public Button Button => button;

            public void SetLabel(string text)
            {
                if (label)
                    label.text = text;
            }

            public void SetVisible(bool visible)
            {
                if (root)
                    root.SetActive(visible);
                else if (button)
                    button.gameObject.SetActive(visible);
            }

            public void SetState(AnswerVisualState state, Sprite defaultSprite, Sprite correctSprite, Sprite incorrectSprite)
            {
                if (!stateImage)
                    return;

                var sprite = state switch
                {
                    AnswerVisualState.Correct => correctSprite,
                    AnswerVisualState.Incorrect => incorrectSprite,
                    _ => defaultSprite
                };

                stateImage.sprite = sprite;
            }
        }

        private enum AnswerVisualState
        {
            Default,
            Correct,
            Incorrect
        }
    }
}
