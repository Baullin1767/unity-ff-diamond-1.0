using System;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class GamePetsItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Image image;
        [SerializeField] private Image imageType;
        [SerializeField] private PetsType type;
        public override async void Bind<T>(T data)
        {
            if (data is Pets petsData)
            {
                title.text = petsData.title;
                desc.text = petsData.desc;
                image.sprite = await DataManager.GetSprite(
                    $"{PathBuilder.GetBasePath(DataType.Pets)}/{petsData.image}");
                Enum.TryParse(petsData.type, out type);
            }
        }
    }

    enum PetsType
    {
        Animal,
        Feline,
        Avian,
        Reptile,
        Mythical
    }
}