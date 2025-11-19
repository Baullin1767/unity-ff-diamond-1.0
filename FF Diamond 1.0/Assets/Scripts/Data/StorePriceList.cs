using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// ScriptableObject that stores available store prices configured in the editor.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Store Price List", fileName = "StorePriceList")]
    public sealed class StorePriceList : ScriptableObject
    {
        [SerializeField]
        private List<int> prices = new();

        public IReadOnlyList<int> Prices => prices;

    }
}
