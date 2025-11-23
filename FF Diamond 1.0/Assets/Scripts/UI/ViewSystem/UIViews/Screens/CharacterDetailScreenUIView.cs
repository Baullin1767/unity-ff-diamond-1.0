using System.Collections;
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
        
        [Header("Loading Panel")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private float loadingDurationSeconds = 2f;

        private Coroutine _hideLoadingRoutine;
        
        public override void Show()
        {
            rootGO.SetActive(true);
            ShowLoadingPanel();
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
            if (_hideLoadingRoutine != null)
            {
                StopCoroutine(_hideLoadingRoutine);
                _hideLoadingRoutine = null;
            }

            if (loadingPanel)
                loadingPanel.SetActive(false);

            rootGO.SetActive(false);
        }

        private void ShowLoadingPanel()
        {
            if (!loadingPanel)
                loadingPanel = CreateRuntimeLoadingPanel();

            if (!loadingPanel)
                return;

            loadingPanel.SetActive(true);
            if (_hideLoadingRoutine != null)
                StopCoroutine(_hideLoadingRoutine);

            _hideLoadingRoutine = StartCoroutine(HideLoadingAfterDelay());
        }

        private IEnumerator HideLoadingAfterDelay()
        {
            if (loadingDurationSeconds > 0f)
                yield return new WaitForSeconds(loadingDurationSeconds);

            if (loadingPanel)
                loadingPanel.SetActive(false);

            _hideLoadingRoutine = null;
        }

        private GameObject CreateRuntimeLoadingPanel()
        {
            if (!rootGO)
                return null;

            var parent = rootGO.GetComponent<RectTransform>() ?? rootGO.AddComponent<RectTransform>();
            var panel = new GameObject("LoadingPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.SetAsLastSibling();

            var background = panel.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.6f);

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.SetParent(panelRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.GetComponent<TextMeshProUGUI>();
            text.text = "Loading...";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 48f;
            text.color = Color.white;

            panel.SetActive(false);
            return panel;
        }
    }
}
