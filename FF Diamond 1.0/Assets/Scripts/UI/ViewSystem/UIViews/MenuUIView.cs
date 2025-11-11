using System;
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
                menuButton.ComponentButton.onClick.AddListener(
                    () => _viewController.ShowScreen(menuButton.UIScreenId));
            }
        }

        public override void Show()
        {
            titleMenu.SetActive(true);
            baseMenu.SetActive(true);
        }

        public override void Hide()
        {
            titleMenu.SetActive(false);
            baseMenu.SetActive(false);
        }
        
        [Serializable]
        public struct MenuButtons
        {
            [SerializeField] private Button componentButton;
            [SerializeField] private UIScreenId uIScreenId;
            
            public Button ComponentButton => componentButton;
            public UIScreenId UIScreenId => uIScreenId;
        }
    }
}
