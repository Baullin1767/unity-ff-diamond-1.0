using UnityEngine;

namespace UI
{
    /// <summary>
    /// Base component for all UI views. Handles CanvasGroup state and activation toggles.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool startVisible;

        private bool _initialized;

        protected virtual void Awake()
        {
            if (!_initialized)
            {
                Initialize(startVisible);
            }
        }

        public void Initialize(bool visible)
        {
            _initialized = true;
            if (visible)
            {
                ShowImmediate();
            }
            else
            {
                HideImmediate();
            }
        }

        public void Show()
        {
            EnsureInitialized();
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            UpdateCanvasGroup(1f, true);
            OnShow();
        }

        public void Hide()
        {
            if (!_initialized)
            {
                return;
            }

            OnHide();
            UpdateCanvasGroup(0f, false);
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void ShowImmediate()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            UpdateCanvasGroup(1f, true);
        }

        private void HideImmediate()
        {
            UpdateCanvasGroup(0f, false);
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateCanvasGroup(float alpha, bool interactable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            Initialize(startVisible);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
