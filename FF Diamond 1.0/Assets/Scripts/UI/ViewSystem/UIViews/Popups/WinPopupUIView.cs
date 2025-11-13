using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.ViewSystem.UIViews.Popups
{
    public class WinPopupUIView : PopupUIView
    {
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private GameObject popupRoot;
        
        public override void Show(int reward, string result)
        {
            rewardText.text = reward.ToString();
            popupRoot.SetActive(true);
        }

        public override void Hide()
        {
            popupRoot.SetActive(false);
        }
    }
}