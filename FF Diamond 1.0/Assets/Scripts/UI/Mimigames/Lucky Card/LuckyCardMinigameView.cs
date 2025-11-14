using System;
using System.Collections.Generic;
using TMPro;
using UI.Minigames.Currency;
using UI.ViewSystem;
using UI.ViewSystem.UIViews.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Minigames.LuckyCard
{
    /// <summary>
    /// Coordinates the Lucky Card minigame: prepares cards, awards rewards, and listens for scratch completion.
    /// </summary>
    public sealed class LuckyCardMinigameView : MonoBehaviour
    {
        [Header("Scratch Setup")]
        [SerializeField] private ScratchCardSurface scratchSurface;

        [Header("UI")]
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private Button newCardButton;
        [SerializeField] private string rewardFormat = "{0:N0}";
        [SerializeField] private string pendingRewardText = "???";

        [Header("Rewards")]
        [SerializeField] private List<LuckyCardReward> rewards = new();

        [Header("Popups")]
        [SerializeField] private WinPopupUIView winPopup;

        private LuckyCardReward _currentReward;
        private bool _rewardPrepared;
        private bool _rewardClaimed;
        private bool _pendingAutoRefresh;

        [Inject] private IMinigameCurrencyService _currencyService;
        [Inject] private IUIViewController _viewController;

        private void Awake()
        {
            if (scratchSurface)
                scratchSurface.Completed += HandleScratchCompleted;

            if (newCardButton)
                newCardButton.onClick.AddListener(HandleNewCardClicked);

            if (winPopup)
                winPopup.Hidden += HandleWinPopupHidden;
        }

        private void OnEnable()
        {
            _rewardClaimed = false;
            _pendingAutoRefresh = false;
            if (!Application.isPlaying)
                return;

            PrepareNextCard();
        }

        private void OnDisable()
        {
            _rewardClaimed = false;
            _pendingAutoRefresh = false;
        }

        private void OnDestroy()
        {
            if (scratchSurface)
                scratchSurface.Completed -= HandleScratchCompleted;

            if (newCardButton)
                newCardButton.onClick.RemoveListener(HandleNewCardClicked);

            if (winPopup)
                winPopup.Hidden -= HandleWinPopupHidden;
        }

        private void HandleNewCardClicked()
        {
            if (!Application.isPlaying)
                return;

            _pendingAutoRefresh = false;
            PrepareNextCard();
        }

        private void PrepareNextCard()
        {
            if (rewards.Count == 0)
            {
                Debug.LogWarning("LuckyCardMinigameView has no rewards configured.", this);
                if (newCardButton)
                    newCardButton.interactable = false;
                return;
            }

            var index = UnityEngine.Random.Range(0, rewards.Count);
            _currentReward = rewards[index];
            _rewardPrepared = true;
            _rewardClaimed = false;
            _pendingAutoRefresh = false;


            UpdateRewardLabel(null);

            if (scratchSurface)
                scratchSurface.ResetScratch();

            if (newCardButton)
                newCardButton.interactable = false;
        }

        private void HandleScratchCompleted()
        {
            if (!_rewardPrepared || _rewardClaimed)
                return;

            _rewardClaimed = true;
            _pendingAutoRefresh = true;
            var rewardAmount = Mathf.Max(0, _currentReward.RewardAmount);
            UpdateRewardLabel(rewardAmount);

            if (rewardAmount > 0 && _currencyService != null)
                _currencyService.Add(rewardAmount);

            _viewController?.ShowPopup(UIPopupId.Win, rewardAmount);

            if (newCardButton)
                newCardButton.interactable = true;
        }

        private void HandleWinPopupHidden()
        {
            if (!_pendingAutoRefresh)
                return;

            _pendingAutoRefresh = false;
            PrepareNextCard();
        }

        private void UpdateRewardLabel(int? amount)
        {
            if (!rewardLabel)
                return;

            rewardLabel.text = amount.HasValue
                ? string.Format(rewardFormat, amount.Value)
                : pendingRewardText;
        }

        [Serializable]
        private struct LuckyCardReward
        {
            [SerializeField] private int rewardAmount;

            public int RewardAmount => rewardAmount;
        }
    }
}
