using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class WifiIconLoop : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform icon;
        [SerializeField] private CanvasGroup iconCanvasGroup;

        [Header("Timings")]
        [SerializeField] private float showDuration = 0.45f;
        [SerializeField] private float prePulseDelay = 0.12f;
        [SerializeField] private float pulseDuration = 0.3f;
        [SerializeField] private float postPulseDelay = 0.25f;
        [SerializeField] private float hideDuration = 0.45f;

        [Header("Pulse")]
        [SerializeField] private float pulseScale = 1.12f;
        [SerializeField] private Ease showEase = Ease.InOutSine;
        [SerializeField] private Ease hideEase = Ease.InOutSine;
        [SerializeField] private Ease fadeInEase = Ease.InOutSine;
        [SerializeField] private Ease fadeOutEase = Ease.InOutSine;
        [SerializeField] private Ease pulseEase = Ease.InOutSine;

        private Sequence _seq;
        private CanvasGroup CanvasGroup => iconCanvasGroup;

        private void Awake()
        {
            if (!iconCanvasGroup && icon)
                iconCanvasGroup = icon.GetComponent<CanvasGroup>();

            if (!iconCanvasGroup && icon)
            {
                iconCanvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
                iconCanvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            PlayLoop();
        }

        private void OnDisable()
        {
            _seq?.Kill();
        }

        private void PlayLoop()
        {
            if (!icon)
                return;

            _seq?.Kill();

            icon.localRotation = Quaternion.Euler(0f, 0f, 90f);
            icon.localScale = Vector3.one;
            if (CanvasGroup)
                CanvasGroup.alpha = 0f;

            _seq = DOTween.Sequence();

            // 1. Reveal: 90 -> 0 with smooth acceleration + fade in
            var revealRotate = icon
                .DOLocalRotate(Vector3.zero, showDuration)
                .SetEase(showEase);
            _seq.Append(revealRotate);
            if (CanvasGroup)
                _seq.Join(CanvasGroup.DOFade(1f, showDuration).SetEase(fadeInEase));

            // 2. Short pause at 0° before the pulse
            if (prePulseDelay > 0f)
                _seq.AppendInterval(prePulseDelay);

            // 3. Single pulse while visible
            if (pulseDuration > 0f && !Mathf.Approximately(pulseScale, 1f))
            {
                _seq.Append(icon
                    .DOScale(pulseScale, pulseDuration * 0.5f)
                    .SetEase(pulseEase));
                _seq.Append(icon
                    .DOScale(1f, pulseDuration * 0.5f)
                    .SetEase(pulseEase));
            }
            if (postPulseDelay > 0f)
                _seq.AppendInterval(postPulseDelay);

            // 4. Hide: rotate to -90° and fade out
            var hideRotate = icon
                .DOLocalRotate(new Vector3(0f, 0f, -90f), hideDuration)
                .SetEase(hideEase);
            _seq.Append(hideRotate);
            if (CanvasGroup)
                _seq.Join(CanvasGroup.DOFade(0f, hideDuration).SetEase(fadeOutEase));

            // 5. Loop
            _seq.SetLoops(-1, LoopType.Restart);
        }
    }
}
