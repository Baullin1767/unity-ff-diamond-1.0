using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.ViewSystem.UIViews
{
    public class CharacterDetailScreenUIView : UIView
    {
        [SerializeField] private GameObject rootGO;
        
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private TMP_Text skillDesc;
        [SerializeField] private Image image;
        
        [SerializeField] private TMP_Text gender;
        [SerializeField] private TMP_Text age;
        [SerializeField] private TMP_Text birthday;
        [SerializeField] private TMP_Text story;
        
        public override void Show()
        {
            rootGO.SetActive(true);
        }

        public async void Initialize(Characters data)
        {
            nameText.text = data.name;
            skillName.text = data.skill.skillName;
            skillDesc.text = data.skill.skillDesc;
            image.gameObject.SetActive(true);
            image.sprite = await DataManager.GetSprite(
                $"{PathBuilder.GetBasePath(DataType.Characters)}/{data.image}");
            gender.text = data.biography.gender;
            age.text = data.biography.age.ToString();
            birthday.text = data.biography.birthday;
            story.text = data.biography.story;
        }

        public override void Hide()
        {
            rootGO.SetActive(false);
        }
    }
}