using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ViewSystem.UIViews.Popups
{
    public sealed class PurchaseSuccessPopupUIView : PopupUIView
    {
        [SerializeField] private GameObject popupRoot;

        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton)
                closeButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            if (closeButton)
                closeButton.onClick.RemoveListener(Hide);
        }

        public override void Show(float reward, string context)
        {
            popupRoot.SetActive(true);
        }

        public override void Hide()
        {
            popupRoot.SetActive(false);
        }
    }
}
