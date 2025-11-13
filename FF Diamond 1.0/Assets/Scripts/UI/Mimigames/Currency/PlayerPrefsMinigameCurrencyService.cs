using System;
using UnityEngine;

namespace UI.Minigames.Currency
{
    /// <summary>
    /// PlayerPrefs-backed implementation of <see cref="IMinigameCurrencyService"/>.
    /// </summary>
    public sealed class PlayerPrefsMinigameCurrencyService : IMinigameCurrencyService
    {
        private readonly string _prefsKey;
        private readonly int _startingBalance;
        private int _balance;
        private bool _loaded;

        public PlayerPrefsMinigameCurrencyService(string prefsKey, int startingBalance)
        {
            _prefsKey = string.IsNullOrWhiteSpace(prefsKey) ? "MinigameCurrency" : prefsKey;
            _startingBalance = Mathf.Max(0, startingBalance);
        }

        public int Balance
        {
            get
            {
                EnsureLoaded();
                return _balance;
            }
        }

        public event Action<int> BalanceChanged;

        public void SetBalance(int value)
        {
            EnsureLoaded();
            var clamped = Mathf.Max(0, value);
            if (_balance == clamped)
                return;

            _balance = clamped;
            Persist();
        }

        public void Add(int amount)
        {
            if (amount == 0)
                return;

            EnsureLoaded();
            var newBalance = Mathf.Max(0, _balance + amount);
            if (newBalance == _balance)
                return;

            _balance = newBalance;
            Persist();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                return true;

            EnsureLoaded();
            if (_balance < amount)
                return false;

            _balance -= amount;
            Persist();
            return true;
        }

        public int GetBalance()
        {
            return PlayerPrefs.GetInt(_prefsKey);
        }

        private void Persist()
        {
            PlayerPrefs.SetInt(_prefsKey, _balance);
            PlayerPrefs.Save();
            BalanceChanged?.Invoke(_balance);
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            _balance = Mathf.Max(0, PlayerPrefs.GetInt(_prefsKey, _startingBalance));
            _loaded = true;
            BalanceChanged?.Invoke(_balance);
        }
    }
}
