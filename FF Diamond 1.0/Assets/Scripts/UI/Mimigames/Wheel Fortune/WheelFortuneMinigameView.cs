using TMPro;
using UI.Minigames.Currency;
using UI.ViewSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Minigames.WheelFortune
{
    /// <summary>
    /// Simple UI wrapper that wires button clicks to the spinner and updates the reward label.
    /// </summary>
    public sealed class WheelFortuneMinigameView : MonoBehaviour
    {
        [SerializeField] private WheelFortuneSpinner spinner;
        [SerializeField] private Button spinButton;
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private string rewardFormat = "{0:N0}";

        
        [Inject]
        private IMinigameCurrencyService _currencyService;
        
        [Inject] 
        private IUIViewController _viewController;
        
        private void Awake()
        {
            if (spinButton)
                spinButton.onClick.AddListener(HandleSpinButtonClicked);

            if (spinner)
                spinner.SpinCompleted += HandleSpinFinished;
        }

        private void OnDestroy()
        {
            if (spinButton)
                spinButton.onClick.RemoveListener(HandleSpinButtonClicked);

            if (spinner)
                spinner.SpinCompleted -= HandleSpinFinished;
        }

        private void HandleSpinButtonClicked()
        {
            if (spinner == null)
                return;

            if (spinner.TrySpin())
            {
                if (spinButton)
                    spinButton.interactable = false;
            }
        }

        private void HandleSpinFinished(WheelFortuneSegment segment)
        {
            if (rewardLabel)
                rewardLabel.text = string.Format(rewardFormat, segment.RewardAmount);

            if (spinButton)
                spinButton.interactable = true;
            
            _currencyService.Add(segment.RewardAmount);
            
            _viewController.ShowPopup(UIPopupId.Win, segment.RewardAmount);
        }
    }
}
