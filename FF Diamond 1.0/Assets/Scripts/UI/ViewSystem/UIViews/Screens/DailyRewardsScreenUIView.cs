using System;
using System.Collections.Generic;
using TMPro;
using UI.DailyRewards;
using UI.Minigames.Currency;
using UnityEngine;
using Zenject;

namespace UI.ViewSystem.UIViews
{
    /// <summary>
    /// Handles the Daily Rewards screen logic: unlock sequencing, claim gating, and reward UI states.
    /// </summary>
    public sealed class DailyRewardsScreenUIView : UIView
    {
        [Header("Root")]
        [SerializeField] private GameObject rootGO;

        [Header("Reward Tiles")]
        [SerializeField] private DailyRewardItemView[] rewardItems = Array.Empty<DailyRewardItemView>();
        [SerializeField] private List<DailyRewardDefinition> rewards = new();
        [SerializeField] private string fallbackDayFormat = "Day {0}";
        [SerializeField] private string rewardAmountFormat = "{0:N0}";

        [Header("Top Reward Preview")]
        [SerializeField] private TMP_Text nextRewardAmountLabel;

        [Header("Progress Storage")]
        [SerializeField] private string playerPrefsKey = "DailyRewards";
        [SerializeField] private bool loopAfterFinalReward = true;
        [SerializeField] private float claimCooldownHours = 24f;

        private IDailyRewardsService _service;
        private int _rewardCount;

        [Inject] private IMinigameCurrencyService _currencyService;
        [Inject] private IUIViewController _viewController;

        private void Awake()
        {
            BuildService();
            SubscribeToTiles();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTiles();
        }

        public override void Show()
        {
            if (rootGO)
                rootGO.SetActive(true);

            RefreshView();
        }

        public override void Hide()
        {
            if (rootGO)
                rootGO.SetActive(false);
        }

        private void BuildService()
        {
            var tileCount = rewardItems?.Length ?? 0;
            _rewardCount = Mathf.Min(tileCount, rewards.Count);

            if (_rewardCount <= 0)
            {
                Debug.LogWarning("DailyRewardsScreenUIView requires configured reward tiles and definitions.", this);
                return;
            }

            if (tileCount != rewards.Count)
            {
                Debug.LogWarning($"Mismatch between reward tiles ({tileCount}) and definitions ({rewards.Count}). Extra entries will be ignored.", this);
            }

            var cooldown = TimeSpan.FromHours(Mathf.Max(1f, claimCooldownHours));
            _service = new PlayerPrefsDailyRewardsService(playerPrefsKey, _rewardCount, cooldown, loopAfterFinalReward);
        }

        private void SubscribeToTiles()
        {
            if (rewardItems == null)
                return;

            foreach (var tile in rewardItems)
            {
                if (!tile)
                    continue;
                tile.Clicked += HandleTileClicked;
            }
        }

        private void UnsubscribeFromTiles()
        {
            if (rewardItems == null)
                return;

            foreach (var tile in rewardItems)
            {
                if (!tile)
                    continue;
                tile.Clicked -= HandleTileClicked;
            }
        }

        private void HandleTileClicked(DailyRewardItemView tile)
        {
            if (_service == null || _rewardCount <= 0)
                return;

            var tileIndex = Array.IndexOf(rewardItems, tile);
            if (tileIndex < 0 || tileIndex >= _rewardCount)
                return;

            var now = DateTime.UtcNow;
            if (!_service.CanClaim(now))
                return;

            var claimedInCycle = _service.ClaimedDaysInCurrentCycle;
            if (tileIndex != claimedInCycle)
                return;

            if (!_service.TryClaim(now, out var claimedDayIndex))
                return;

            var reward = GetDefinition(claimedDayIndex);
            if (reward.Amount > 0 && _currencyService != null)
                _currencyService.Add(reward.Amount);

            _viewController?.ShowPopup(UIPopupId.Win, reward.Amount);

            RefreshView();
        }

        private void RefreshView()
        {
            if (_service == null || _rewardCount <= 0)
                return;

            var now = DateTime.UtcNow;
            var canClaimToday = _service.CanClaim(now);
            var claimedInCycle = Mathf.Clamp(_service.ClaimedDaysInCurrentCycle, 0, _rewardCount);
            var nextIndex = Mathf.Clamp(_service.CurrentDayIndex, 0, Mathf.Max(0, _rewardCount - 1));
            var hasAvailableTile = canClaimToday && claimedInCycle < _rewardCount && !_service.HasCompletedAllRewards;

            for (var i = 0; i < rewardItems.Length; i++)
            {
                var tile = rewardItems[i];
                if (!tile)
                    continue;

                if (i >= _rewardCount)
                {
                    tile.SetState(DailyRewardVisualState.Locked);
                    continue;
                }

                var definition = GetDefinition(i);
                var dayLabel = string.IsNullOrWhiteSpace(definition.DayName)
                    ? string.Format(fallbackDayFormat, i + 1)
                    : definition.DayName;
                var amountLabel = string.Format(rewardAmountFormat, definition.Amount);
                tile.SetContent(dayLabel, amountLabel);

                DailyRewardVisualState state;
                if (i < claimedInCycle)
                {
                    state = DailyRewardVisualState.Claimed;
                }
                else if (i == claimedInCycle && hasAvailableTile)
                {
                    state = DailyRewardVisualState.Available;
                }
                else
                {
                    state = DailyRewardVisualState.Locked;
                }

                tile.SetState(state);
            }

            UpdateHeader(nextIndex);
        }

        private void UpdateHeader(int rewardIndex)
        {
            if (_service != null && _service.HasCompletedAllRewards)
            {
                if (nextRewardAmountLabel)
                    nextRewardAmountLabel.text = string.Empty;
                return;
            }

            if (!nextRewardAmountLabel)
                return;

            if (rewardIndex < 0 || rewardIndex >= _rewardCount)
            {
                nextRewardAmountLabel.text = string.Empty;
                return;
            }

            var reward = GetDefinition(rewardIndex);
            nextRewardAmountLabel.text = string.Format(rewardAmountFormat, reward.Amount);
        }

        private DailyRewardDefinition GetDefinition(int index)
        {
            if (rewards.Count == 0)
                return default;

            index = Mathf.Clamp(index, 0, rewards.Count - 1);
            return rewards[index];
        }

        [Serializable]
        private struct DailyRewardDefinition
        {
            [SerializeField] private string dayName;
            [SerializeField] private int amount;

            public string DayName => dayName;
            public int Amount => amount;
        }
    }
}
