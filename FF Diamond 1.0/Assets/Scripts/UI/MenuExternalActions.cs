using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Wires up share/external buttons that live on the main menu.
    /// Extracted from <see cref="UI.ViewSystem.UIViews.MenuUIView"/> to keep responsibilities isolated.
    /// </summary>
    public sealed class MenuExternalActions : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button shareButton;
        [SerializeField] private Button buttonTu;
        [SerializeField] private Button buttonPp;

        [Header("Share Payload")]
        [SerializeField] private string shareSubject = "FF Diamond";
        [SerializeField] private string shareUrl;

        [Header("External Links")]
        [SerializeField] private string googleUrl = "https://www.google.com";
        [SerializeField] private string yandexUrl = "https://www.yandex.com";

        private void OnEnable()
        {
            if (shareButton)
                shareButton.onClick.AddListener(ShareApplication);
            if (buttonTu)
                buttonTu.onClick.AddListener(OpenGoogle);
            if (buttonPp)
                buttonPp.onClick.AddListener(OpenYandex);
        }

        private void OnDisable()
        {
            if (shareButton)
                shareButton.onClick.RemoveListener(ShareApplication);
            if (buttonTu)
                buttonTu.onClick.RemoveListener(OpenGoogle);
            if (buttonPp)
                buttonPp.onClick.RemoveListener(OpenYandex);
        }

        private void ShareApplication()
        {
            new NativeShare()
                .SetSubject(shareSubject)
                .SetUrl(shareUrl)
                .Share();
        }

        private void OpenGoogle() => OpenExternalUrl(googleUrl, "Google");
        private void OpenYandex() => OpenExternalUrl(yandexUrl, "Yandex");

        private void OpenExternalUrl(string url, string label)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Debug.LogWarning($"Cannot open {label} link because it is empty.", this);
                return;
            }

            Application.OpenURL(url);
        }
    }
}
