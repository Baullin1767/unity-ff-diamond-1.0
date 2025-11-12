using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        public void Activate()
        {
            image.sprite = activeSprite;
            text.color = Color.black;
        }

        public void Deactivate()
        {
            image.sprite = inactiveSprite;
            text.color = Color.white;
        }
    }
}