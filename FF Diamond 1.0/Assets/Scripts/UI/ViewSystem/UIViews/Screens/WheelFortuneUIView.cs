using UI.Minigames.WheelFortune;
using UnityEngine;

namespace UI.ViewSystem.UIViews
{
    /// <summary>
    /// Entry point for the Wheel Fortune minigame screen.
    /// </summary>
    public sealed class WheelFortuneUIView : UIView
    {
        [SerializeField] private GameObject rootGO;
        [SerializeField] private WheelFortuneMinigameView minigameView;

        public override void Show()
        {
            if (rootGO)
                rootGO.SetActive(true);

            if (minigameView && !minigameView.gameObject.activeSelf)
                minigameView.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            if (rootGO)
                rootGO.SetActive(false);
        }
    }
}
