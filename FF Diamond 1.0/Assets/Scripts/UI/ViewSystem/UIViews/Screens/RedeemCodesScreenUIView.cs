using UnityEngine;

namespace UI.ViewSystem.UIViews
{
    public class RedeemCodesScreenUIView : UIView
    {
        [SerializeField] private GameObject rootGO;
        public override void Show()
        {
            rootGO.SetActive(true);
        }

        public override void Hide()
        {
            rootGO.SetActive(false);
        }
    }
}