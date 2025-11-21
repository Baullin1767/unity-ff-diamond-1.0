using Data;
using UI.CustomScrollRect;
using UI.Minigames.Currency;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews.Items
{
    public class StoreItemView : BaseItemView
    {
        [SerializeField] private Button button;
        [Inject] private IMinigameCurrencyService _currencyService;
        public override void Bind<T>(T data)
        {
            button.onClick.RemoveAllListeners();
            if (data is Store store)
            {
                title.text = store.price.ToString();
                button.onClick.AddListener(() => _currencyService.Add(store.price));
            }
        }
    }
}