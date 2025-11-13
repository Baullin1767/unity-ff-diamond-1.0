using UnityEngine;
using Zenject;

namespace UI.Minigames.Currency
{
    /// <summary>
    /// ScriptableObject installer that exposes the currency service via Zenject.
    /// </summary>
    [CreateAssetMenu(menuName = "Minigames/Currency Installer", fileName = "MinigameCurrencyInstaller")]
    public sealed class MinigameCurrencyInstaller : ScriptableObjectInstaller<MinigameCurrencyInstaller>
    {
        [SerializeField] private string playerPrefsKey = "MinigameCurrency";
        [Min(0)]
        [SerializeField] private int startingBalance;

        public override void InstallBindings()
        {
            Container.Bind<IMinigameCurrencyService>()
                .To<PlayerPrefsMinigameCurrencyService>()
                .AsSingle()
                .WithArguments(playerPrefsKey, startingBalance);
        }
    }
}
