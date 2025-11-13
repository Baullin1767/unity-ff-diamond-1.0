using System;

namespace UI.Minigames.Currency
{
    /// <summary>
    /// Provides read/write access to the player's minigame currency.
    /// </summary>
    public interface IMinigameCurrencyService
    {
        int Balance { get; }
        event Action<int> BalanceChanged;

        void SetBalance(int value);
        void Add(int amount);
        bool TrySpend(int amount);
        int GetBalance();
    }
}
