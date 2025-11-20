using System;
using Data;
using UI.ViewSystem;
using UI.ViewSystem.UIViews.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.CustomScrollRect.Items
{
    public class RedeemCodesItemView : BaseItemView
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject lockBlock;
        [SerializeField] private Button byeButton;
        [SerializeField, Min(0)] private int redeemCodePurchaseCost = 1000;

        private RedeemCodes _redeemCode;
        private string _purchaseContext = string.Empty;
        private PurchaseCostPopupUIView _purchasePopupView;
        private IUIViewController _viewController;

        [Inject]
        public void Construct(IUIViewController viewController)
        {
            _viewController = viewController;
        }

        private void Awake()
        {
            _purchasePopupView = FindObjectOfType<PurchaseCostPopupUIView>(true);
            if (_purchasePopupView)
                _purchasePopupView.PurchaseSucceeded += HandlePurchaseSucceeded;
        }

        private void OnDestroy()
        {
            if (_purchasePopupView)
                _purchasePopupView.PurchaseSucceeded -= HandlePurchaseSucceeded;

            if (button)
                button.onClick.RemoveAllListeners();
            if (byeButton)
                byeButton.onClick.RemoveListener(OpenPurchasePopup);
        }

        public override void Bind<T>(T data)
        {
            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.image.sprite = button.spriteState.highlightedSprite;
            }
            if (byeButton)
                byeButton.onClick.RemoveListener(OpenPurchasePopup);

            if (data is RedeemCodes redeemCodes)
            {
                _redeemCode = redeemCodes;

                if (title)
                    title.text = redeemCodes.code;

                if (button)
                    button.onClick.AddListener(() =>
                    {
                        GUIUtility.systemCopyBuffer = redeemCodes.code;
                    });

                _purchaseContext = BuildPurchaseContext(redeemCodes);
                bool isPaid = redeemCodes.isPaid;
                if (lockBlock)
                    lockBlock.SetActive(isPaid);

                if (byeButton)
                {
                    byeButton.gameObject.SetActive(isPaid);
                    if (isPaid)
                        byeButton.onClick.AddListener(OpenPurchasePopup);
                }
            }
            else
            {
                _redeemCode = null;
                _purchaseContext = string.Empty;

                if (lockBlock)
                    lockBlock.SetActive(false);
                if (byeButton)
                    byeButton.gameObject.SetActive(false);
            }
        }

        private void OpenPurchasePopup()
        {
            if (_viewController == null || _redeemCode == null)
                return;

            _purchaseContext = BuildPurchaseContext(_redeemCode);
            _viewController.ShowPopup(
                UIPopupId.PurchaseCost,
                Mathf.Max(0, redeemCodePurchaseCost),
                _purchaseContext);
        }

        private void HandlePurchaseSucceeded(string context)
        {
            if (string.IsNullOrEmpty(context) || !string.Equals(context, _purchaseContext, StringComparison.Ordinal))
                return;

            if (_redeemCode != null)
            {
                _redeemCode.isPaid = false;
                PurchaseStateStorage.SetRedeemCodeIsPaid(_redeemCode, false);
            }

            if (lockBlock)
                lockBlock.SetActive(false);
            if (byeButton)
            {
                byeButton.onClick.RemoveListener(OpenPurchasePopup);
                byeButton.gameObject.SetActive(false);
            }
        }

        private static string BuildPurchaseContext(RedeemCodes redeemCode)
        {
            if (redeemCode == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(redeemCode.code))
                return redeemCode.code;

            if (!string.IsNullOrWhiteSpace(redeemCode.desc))
                return redeemCode.desc;

            return redeemCode.GetHashCode().ToString();
        }
    }
}
