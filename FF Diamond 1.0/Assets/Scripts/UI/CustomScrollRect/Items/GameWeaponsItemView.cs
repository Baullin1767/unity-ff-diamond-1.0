using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class GameWeaponsItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text damage;
        [SerializeField] private TMP_Text tag1;
        [SerializeField] private TMP_Text tag2;
        public override void Bind<T>(T data)
        {
            if (data is GameWeapons gameWeapons)
            {
                title.text = gameWeapons.title;
                desc.text = gameWeapons.desc;
                damage.text = gameWeapons.stats.damage.ToString();
                tag1.text = gameWeapons.tags[0];
                tag2.text = gameWeapons.tags[1];
            }
        }
    }
}