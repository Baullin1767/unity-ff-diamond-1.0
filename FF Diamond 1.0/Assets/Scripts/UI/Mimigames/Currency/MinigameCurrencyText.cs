using TMPro;
using UnityEngine;
using Zenject;

namespace UI.Minigames.Currency
{
    /// <summary>
    /// Binds a TMP_Text to the current balance reported by <see cref="IMinigameCurrencyService"/>.
    /// </summary>
    public sealed class MinigameCurrencyText : MonoBehaviour
    {
        [SerializeField] private TMP_Text balanceLabel;
        [SerializeField] private string format = "{0:N0}";

        private IMinigameCurrencyService _currencyService;

        [Inject]
        private void Construct(IMinigameCurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        private void OnEnable()
        {
            if (_currencyService == null)
                return;

            _currencyService.BalanceChanged += HandleBalanceChanged;
            UpdateLabel(_currencyService.GetBalance());
        }

        private void OnDisable()
        {
            if (_currencyService == null)
                return;

            _currencyService.BalanceChanged -= HandleBalanceChanged;
        }

        private void HandleBalanceChanged(int value)
        {
            UpdateLabel(value);
        }

        private void UpdateLabel(int value)
        {
            if (!balanceLabel)
                return;

            balanceLabel.text = string.Format(format, value);
        }
    }
}
