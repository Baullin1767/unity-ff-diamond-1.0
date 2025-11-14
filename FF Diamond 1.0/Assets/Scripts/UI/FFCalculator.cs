using System;
using TMPro;
using UI.ViewSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class FFCalculator : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button calculateButton;
        [SerializeField] private Button[] buttons;
        [SerializeField] private float[] coefficients = 
            {0.25f, 0.75f, 0.075f, 0.075f};

        private float _currentCoefficient = 0.1f;

        [Inject] private IUIViewController _viewController;

        private void Awake()
        {
            calculateButton.onClick.AddListener(Count);
            for (int i = 0; i < buttons.Length; i++)
            {
                var i1 = i;
                buttons[i].onClick.AddListener(
                    () => { _currentCoefficient = coefficients[i1]; });
            }
        }

        private void Count()
        {
            if (!string.IsNullOrWhiteSpace(inputField.text)
                && int.TryParse(inputField.text, out int currentValue))
            {
                _viewController.ShowPopup(
                    UIPopupId.PurchaseCost, currentValue * _currentCoefficient);
            }
            else
                inputField.text = "Enter an integer";
        }
    }
}