using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class RedeemCodesItemView : BaseItemView
    {
        [SerializeField] private Button button;
        public override void Bind<T>(T data)
        {
            if (data is RedeemCodes redeemCodes)
            {
                title.text = redeemCodes.code;
                button.onClick.AddListener(() => GUIUtility.systemCopyBuffer = redeemCodes.code);
            }
        }
    }
}