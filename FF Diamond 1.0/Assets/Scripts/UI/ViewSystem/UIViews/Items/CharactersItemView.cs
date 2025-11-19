using System;
using Data;
using TMPro;
using UI.CustomScrollRect;
using UI.ViewSystem.UIViews.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews.Items
{
    public class CharactersItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Button openButton;
        [SerializeField] private Button payButton;
        [SerializeField] private GameObject blurFrame;
        [SerializeField, Min(0)] private int characterPurchaseCost = 1000;
        
        private CharacterDetailScreenUIView _characterDetailScreenUIView;
        private Characters _character;
        private string _purchaseContext = string.Empty;
        private PurchaseCostPopupUIView _purchasePopupView;
        
        private IUIViewController _uiViewController;

        [Inject]
        public void Construct(IUIViewController uiViewController)
        {
            _uiViewController = uiViewController;
        }
        
        private void Awake()
        {
            _characterDetailScreenUIView = FindObjectOfType<CharacterDetailScreenUIView>();
            _purchasePopupView = FindObjectOfType<PurchaseCostPopupUIView>(true);
            if (_purchasePopupView)
                _purchasePopupView.PurchaseSucceeded += HandlePurchaseSucceeded;
        }

        private void OnDestroy()
        {
            if (_purchasePopupView)
                _purchasePopupView.PurchaseSucceeded -= HandlePurchaseSucceeded;

            if (openButton)
                openButton.onClick.RemoveListener(OpenDetailScreen);
            if (payButton)
                payButton.onClick.RemoveListener(OpenPayScreen);
        }

        public override void Bind<T>(T data)
        {
            if (openButton)
                openButton.onClick.RemoveListener(OpenDetailScreen);
            if (payButton)
                payButton.onClick.RemoveListener(OpenPayScreen);

            if (data is Characters character)
            {
                _character = character;

                if (title)
                    title.text = character.name;
                if (desc)
                    desc.text = character.tagline;

                if (openButton)
                    openButton.onClick.AddListener(OpenDetailScreen);

                _purchaseContext = BuildPurchaseContext(character);
                var isPaid = character.IsPaid;
                if (payButton)
                {
                    payButton.gameObject.SetActive(isPaid);
                    if (isPaid)
                        payButton.onClick.AddListener(OpenPayScreen);
                }

                if (blurFrame)
                    blurFrame.SetActive(isPaid);
            }
            else
            {
                _character = null;
                _purchaseContext = string.Empty;

                if (blurFrame)
                    blurFrame.SetActive(false);
                if (payButton)
                    payButton.gameObject.SetActive(false);
            }
        }

        private void OpenDetailScreen()
        {
            if (_characterDetailScreenUIView == null || _character == null)
                return;

            _characterDetailScreenUIView.Initialize(_character);
            _uiViewController.ShowScreen(UIScreenId.CharacterDetailScreen);
        }
        private void OpenPayScreen()
        {
            if (_uiViewController == null || _character == null)
                return;

            _purchaseContext = BuildPurchaseContext(_character);
            _uiViewController.ShowPopup(
                UIPopupId.PurchaseCost,
                Mathf.Max(0, characterPurchaseCost),
                _purchaseContext);
        }

        private void HandlePurchaseSucceeded(string context)
        {
            if (string.IsNullOrEmpty(context) || !string.Equals(context, _purchaseContext, StringComparison.Ordinal))
                return;

            if (_character != null)
            {
                _character.IsPaid = false;
                PurchaseStateStorage.SetCharacterIsPaid(_character, false);
            }

            if (blurFrame)
                blurFrame.SetActive(false);

            if (payButton)
            {
                payButton.onClick.RemoveListener(OpenPayScreen);
                payButton.gameObject.SetActive(false);
            }
        }

        private static string BuildPurchaseContext(Characters character)
        {
            if (character == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(character.name))
                return character.name;

            if (!string.IsNullOrWhiteSpace(character.tagline))
                return character.tagline;

            return character.GetHashCode().ToString();
        }
    }
}
