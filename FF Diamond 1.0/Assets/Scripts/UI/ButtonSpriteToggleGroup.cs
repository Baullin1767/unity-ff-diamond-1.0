using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Keeps a set of buttons in sync so the clicked one shows the selected sprite while others revert.
    /// </summary>
    public sealed class ButtonSpriteToggleGroup : MonoBehaviour
    {
        [Serializable]
        private sealed class ButtonEntry
        {
            public Button button;
            public Image targetImage;
            public TMP_Text text;
            [NonSerialized] public UnityAction clickHandler;
        }

        [SerializeField] private List<ButtonEntry> entries = new();
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unselectedColor;
        [SerializeField] private int defaultSelectedIndex = -1;

        private void Awake()
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var index = i;
                var entry = entries[i];
                if (entry == null || entry.button == null)
                    continue;

                if (entry.targetImage == null)
                    entry.targetImage = entry.button.GetComponent<Image>();

                entry.clickHandler = () => HandleButtonClicked(index);
                entry.button.onClick.AddListener(entry.clickHandler);
            }
        }

        private void Start()
        {
            ApplySelection(Mathf.Clamp(defaultSelectedIndex, -1, entries.Count - 1));
        }

        private void OnDestroy()
        {
            foreach (var entry in entries)
            {
                if (entry?.button != null && entry.clickHandler != null)
                    entry.button.onClick.RemoveListener(entry.clickHandler);
            }
        }

        private void HandleButtonClicked(int index)
        {
            ApplySelection(index);
        }

        private void ApplySelection(int selectedIndex)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry?.targetImage == null)
                    continue;

                var sprite = i == selectedIndex ? selectedSprite : unselectedSprite;
                if (sprite != null)
                    entry.targetImage.sprite = sprite;

                entry.text.color = i == selectedIndex ? selectedColor : unselectedColor;
            }
        }
    }
}
