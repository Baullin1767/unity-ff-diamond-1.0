using System;
using DG.Tweening;
using UI.Minigames.Currency;
using UI.ViewSystem;
using UnityEngine;
using Zenject;

namespace UI.Minigames.WheelFortune
{
    /// <summary>
    /// Handles the physical spin of the wheel using DOTween.
    /// </summary>
    public sealed class WheelFortuneSpinner : MonoBehaviour
    {
        [SerializeField] private Transform wheelRoot;
        [Tooltip("Set this if your pointer is not aligned with local up (12 o'clock)."), SerializeField]
        private float pointerOffsetDegrees;
        [SerializeField] private bool rotateClockwise = true;
        [Range(0.05f, 0.95f)]
        [SerializeField] private float minNormalizedLandingPoint = 0.2f;
        [Range(0.05f, 0.95f)]
        [SerializeField] private float maxNormalizedLandingPoint = 0.8f;

        private IUIViewController _viewController;
        private IWheelFortuneConfig _config;
        private Tween _spinTween;
        private bool _isInitialized;

        public event Action<WheelFortuneSegment> SpinCompleted;
        public bool IsSpinning => _spinTween != null && _spinTween.IsActive() && _spinTween.IsPlaying();

        [Inject]
        private void Construct(IWheelFortuneConfig config, IUIViewController viewController)
        {
            _viewController = viewController;
            _config = config;
            _isInitialized = true;
        }

        public bool TrySpin(IMinigameCurrencyService currencyService)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("WheelFortuneSpinner has not been initialized yet.", this);
                return false;
            }

            if (wheelRoot == null)
            {
                Debug.LogWarning("Wheel root is not assigned for WheelFortuneSpinner.", this);
                return false;
            }

            if (_config.Segments.Count == 0)
            {
                Debug.LogWarning("Wheel fortune config does not contain any segments.", this);
                return false;
            }

            if (IsSpinning)
                return false;

            if (!currencyService.TrySpend(_config.SpinCost))
            {
                _viewController.ShowPopup(UIPopupId.PurchaseInsufficient, _config.SpinCost);
                return false;
            }

            var winningSegment = _config.GetRandomSegment(out var segmentIndex);
            var segmentAngle = 360f / _config.Segments.Count;
            var clampedMin = Mathf.Clamp01(minNormalizedLandingPoint);
            var clampedMax = Mathf.Clamp01(Mathf.Max(minNormalizedLandingPoint, maxNormalizedLandingPoint));
            var normalizedPoint = UnityEngine.Random.Range(clampedMin, clampedMax);
            var landingOffset = segmentAngle * normalizedPoint;
            var totalSpins = UnityEngine.Random.Range(_config.FullSpinsRange.x, _config.FullSpinsRange.y + 1);
            var finalDuration = UnityEngine.Random.Range(_config.DurationRange.x, _config.DurationRange.y);

            var desiredAngle = segmentIndex * segmentAngle + landingOffset - pointerOffsetDegrees;
            var currentAngle = Mathf.Repeat(wheelRoot.localEulerAngles.z, 360f);
            var deltaAngle = desiredAngle - currentAngle;

            if (rotateClockwise)
            {
                if (deltaAngle > 0f)
                    deltaAngle -= 360f;
                deltaAngle -= totalSpins * 360f;
            }
            else
            {
                if (deltaAngle < 0f)
                    deltaAngle += 360f;
                deltaAngle += totalSpins * 360f;
            }

            var targetRotation = wheelRoot.localEulerAngles + Vector3.forward * deltaAngle;

            _spinTween?.Kill();
            _spinTween = wheelRoot.DOLocalRotate(targetRotation, finalDuration, RotateMode.FastBeyond360)
                .SetEase(_config.SpinEase)
                .OnComplete(() =>
                {
                    var normalized = wheelRoot.localEulerAngles;
                    normalized.z = Mathf.Repeat(normalized.z, 360f);
                    wheelRoot.localEulerAngles = normalized;
                    SpinCompleted?.Invoke(winningSegment);
                    _spinTween = null;
                });

            return true;
        }

        public void StopImmediately()
        {
            if (_spinTween == null)
                return;

            _spinTween.Kill();
            _spinTween = null;
        }

        private void OnDisable()
        {
            StopImmediately();
        }
    }
}
