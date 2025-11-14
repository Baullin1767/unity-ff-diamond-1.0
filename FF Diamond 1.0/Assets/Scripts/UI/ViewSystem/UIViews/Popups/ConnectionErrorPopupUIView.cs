using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ViewSystem.UIViews.Popups
{
    public sealed class ConnectionErrorPopupUIView : PopupUIView
    {
        public enum Mode
        {
            Retry = 0,
            Accept = 1
        }

        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text buttonLabel;
        [SerializeField] private Button actionButton;
        [SerializeField] private string retryText = "retry";
        [SerializeField] private string acceptText = "ACCEPT";

        public event Action<Mode> ActionRequested;

        private Mode _currentMode = Mode.Retry;

        private void Awake()
        {
            if (actionButton)
                actionButton.onClick.AddListener(HandleActionClicked);
        }

        private void OnDestroy()
        {
            if (actionButton)
                actionButton.onClick.RemoveListener(HandleActionClicked);
        }

        public override void Show(float reward, string result)
        {
            _currentMode = ResolveMode(reward);
            UpdateLabel(_currentMode);

            if (popupRoot)
                popupRoot.SetActive(true);
        }

        public override void Hide()
        {
            if (popupRoot)
                popupRoot.SetActive(false);
        }

        private void UpdateLabel(Mode mode)
        {
            if (!buttonLabel)
                return;

            var text = mode == Mode.Retry ? retryText : acceptText;
            buttonLabel.SetText(text);
        }

        private static Mode ResolveMode(float raw)
        {
            var index = Mathf.RoundToInt(raw);
            return Enum.IsDefined(typeof(Mode), index) ? (Mode)index : Mode.Retry;
        }

        private void HandleActionClicked()
        {
            ActionRequested?.Invoke(_currentMode);
        }
    }
}
