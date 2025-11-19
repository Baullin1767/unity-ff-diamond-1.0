using System;
using TMPro;
using UI.Minigames.Currency;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews.Popups
{
    /// <summary>
    /// Confirmation popup for premium characters/items. Shows the cost and delegates success/failure handling to dedicated popups.
    /// </summary>
    public sealed class PurchaseCostPopupUIView : PopupUIView
    {
        [Header("Root")]
        [SerializeField] private GameObject popupRoot;

        [Header("Content")]
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private string costFormat = "{0:N0}";
        
        [Header("Buttons")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        [Header("Popup Routing")]
        [SerializeField] private UIPopupId successPopupId = UIPopupId.PurchaseSuccess;
        [SerializeField] private UIPopupId insufficientPopupId = UIPopupId.PurchaseInsufficient;

        [Inject] private IUIViewController _viewController;
        [Inject] private IMinigameCurrencyService _currencyService;

        public event Action<string> PurchaseSucceeded;

        private float _currentCost;
        private string _currentContext = string.Empty;

        private void Awake()
        {
            if (buyButton)
                buyButton.onClick.AddListener(HandleBuyClicked);
            if (closeButton)
                closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDestroy()
        {
            if (buyButton)
                buyButton.onClick.RemoveListener(HandleBuyClicked);
            if (closeButton)
                closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public override void Show(float reward, string context)
        {
            _currentCost = Mathf.Max(0f, reward);
            _currentContext = context ?? string.Empty;
            UpdateLabels();
            SetPopupVisible(true);
        }

        public override void Hide() => SetPopupVisible(false);

        private void HandleBuyClicked()
        {
            var cost = Mathf.Max(0, Mathf.RoundToInt(_currentCost));
            var success = cost == 0 || _currencyService == null || _currencyService.TrySpend(cost);

            if (success)
            {
                HideSelf();
                PurchaseSucceeded?.Invoke(_currentContext);
                _currentContext = string.Empty;
                _viewController?.ShowPopup(successPopupId, _currentCost);
            }
            else
            {
                HideSelf();
                _currentContext = string.Empty;
                _viewController?.ShowPopup(insufficientPopupId, _currentCost);
            }
        }

        private void HandleCloseClicked() => HideSelf();

        private void HideSelf()
        {
            if (_viewController != null)
                _viewController.HideAllPopups();
            else
                Hide();
        }

        private void UpdateLabels()
        {
            var formattedCost = FormatCost(_currentCost);

            if (costLabel)
                costLabel.text = formattedCost;
        }

        private void SetPopupVisible(bool visible)
        {
            if (popupRoot)
                popupRoot.SetActive(visible);
        }
        
        private string FormatCost(float value)
        {
            var clamped = Mathf.Max(0f, value);
            try
            {
                return string.Format(costFormat, clamped);
            }
            catch (FormatException)
            {
                return clamped.ToString("N0");
            }
        }
    }
}
