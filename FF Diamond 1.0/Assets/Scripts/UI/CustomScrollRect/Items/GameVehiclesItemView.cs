using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class GameVehiclesItemView : OpenableItemView
    {
        [SerializeField] private Image image;
        public override async void Bind<T>(T data)
        {
            base.Bind(data);
            if (data is GameVehicles gameVehicles)
            {
                title.text = gameVehicles.title;
                content.text = gameVehicles.desc;
                image.sprite = await DataManager.GetSprite(
                    $"{PathBuilder.GetBasePath(DataType.GameVehicles)}/{gameVehicles.image}");
            }
        }
    }
}