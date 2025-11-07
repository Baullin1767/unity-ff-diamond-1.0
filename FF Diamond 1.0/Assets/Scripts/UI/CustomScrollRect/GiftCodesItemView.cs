using System.Collections.Generic;
using Data.GiftCodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect
{
    public class GiftCodesItemView : BaseItemView
    {
        [SerializeField] protected TMP_Text title;
        [SerializeField] protected Button button;

        private int _index;

        public override void Bind<T>(int dataIndex, T data)
        {
            _index = dataIndex;
            if (data is GiftCodesData giftCodesData)
            {
                title.text = giftCodesData.code;
            }
            else
            {
                Debug.LogError($"{nameof(GiftCodesItemView)} can't bind to type {data.GetType()}");
            }
        }
    }
}