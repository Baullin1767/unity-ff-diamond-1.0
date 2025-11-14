using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.ViewSystem.UIViews.Popups
{
    public class WinPopupUIView : PopupUIView
    {
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private UnityEvent onHidden;

        public event Action Hidden;
        
        public override void Show(float reward, string result)
        {
            rewardText.text = reward.ToString();
            popupRoot.SetActive(true);
        }

        public override void Hide()
        {
            popupRoot.SetActive(false);
            onHidden?.Invoke();
            Hidden?.Invoke();
        }
    }
}
