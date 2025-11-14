using System;
using Data;
using TMPro;
using UI.ViewSystem;
using UI.ViewSystem.UIViews;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.CustomScrollRect.Items
{
    public class CharactersItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Button button;
        
        private CharacterDetailScreenUIView _characterDetailScreenUIView;
        private Characters _character;
        
        private IUIViewController _uiViewController;

        [Inject]
        public void Construct(IUIViewController uiViewController)
        {
            _uiViewController = uiViewController;
        }
        
        private void Awake()
        {
            _characterDetailScreenUIView = FindObjectOfType<CharacterDetailScreenUIView>();
        }

        public override void Bind<T>(T data)
        {
            if (data is Characters character)
            {
                _character = character;
                
                title.text = character.name;
                desc.text = character.tagline;
                button.onClick.AddListener(OpenDetailScreen);
            } 
        }

        private void OpenDetailScreen()
        {
            _characterDetailScreenUIView.Initialize(_character);
            _uiViewController.ShowScreen(UIScreenId.CharacterDetailScreen);
        }
    }
}
