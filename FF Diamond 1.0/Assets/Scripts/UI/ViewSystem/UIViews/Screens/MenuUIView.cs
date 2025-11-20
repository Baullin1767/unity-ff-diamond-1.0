using System;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews
{
    public class MenuUIView : UIView
    {
        [SerializeField] private GameObject titleMenu;
        [SerializeField] private GameObject baseMenu;
        
        [SerializeField] private MenuButtons[] buttons;
        
        [Inject] 
        private IUIViewController _viewController;

        private void Start()
        {
            foreach (var menuButton in buttons)
            {
                var cachedButton = menuButton;
                cachedButton.ComponentButton.onClick.AddListener(
                    () => HandleMenuButtonClick(cachedButton));
            }

        }

        private void HandleMenuButtonClick(MenuButtons clickedButton)
        {
            foreach (var menuButton in buttons)
            {
                var buttonVisual = menuButton.MenuButtonComponent;
                if (buttonVisual == null)
                    continue;

                if (menuButton.ComponentButton == clickedButton.ComponentButton)
                {
                    buttonVisual.Activate();
                }
                else
                {
                    buttonVisual.Deactivate();
                }
            }

            _viewController.ShowScreen(clickedButton.UIScreenId);
        }

        public override void Show()
        {
            baseMenu.SetActive(true);
        }

        public override void Hide()
        {
            titleMenu.SetActive(false);
            baseMenu.SetActive(false);
        }

    }
    [Serializable]
    public struct MenuButtons
    {
        [SerializeField] private Button componentButton;
        [SerializeField] private UIScreenId uIScreenId;
        
        public Button ComponentButton => componentButton;
        public MenuButton MenuButtonComponent => componentButton != null
            ? componentButton.GetComponent<MenuButton>()
            : null;
        public UIScreenId UIScreenId => uIScreenId;
    }
}
