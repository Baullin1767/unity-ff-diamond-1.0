using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.DailyRewards
{
    /// <summary>
    /// Simple presenter for a single daily reward tile, toggling visuals for claimed/available/locked states.
    /// </summary>
    public sealed class DailyRewardItemView : MonoBehaviour
    {
        [SerializeField] private Button claimButton;
        [SerializeField] private TMP_Text dayLabel;
        [SerializeField] private TMP_Text amountLabel;
        [SerializeField] private GameObject claimedState;
        [SerializeField] private GameObject availableState;
        [SerializeField] private GameObject lockedState;

        public event Action<DailyRewardItemView> Clicked;

        public Button Button => claimButton;

        private void Awake()
        {
            if (claimButton)
                claimButton.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            if (claimButton)
                claimButton.onClick.RemoveListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            Clicked?.Invoke(this);
        }

        public void SetContent(string dayText, string amountText)
        {
            if (dayLabel)
                dayLabel.text = dayText;

            if (amountLabel)
                amountLabel.text = amountText;
        }

        public void SetState(DailyRewardVisualState state)
        {
            if (claimedState)
                claimedState.SetActive(state == DailyRewardVisualState.Claimed);
            if (availableState)
                availableState.SetActive(state == DailyRewardVisualState.Available);
            if (lockedState)
                lockedState.SetActive(state == DailyRewardVisualState.Locked);

            if (claimButton)
                claimButton.interactable = state == DailyRewardVisualState.Available;
        }
    }

    public enum DailyRewardVisualState
    {
        Locked,
        Available,
        Claimed
    }
}
