using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews
{
    public class MiniGamesMenuUIView : UIView
    {
        [SerializeField] private GameObject rootGO;
        
        [SerializeField] private MenuButtons[] buttons;

        [Inject] 
        private IUIViewController _viewController;
        
        private void Awake()
        {
            foreach (var menuButton in buttons)
            {
                menuButton.ComponentButton.onClick.AddListener(
                    () => _viewController.ShowScreen(menuButton.UIScreenId));
            }
        }

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