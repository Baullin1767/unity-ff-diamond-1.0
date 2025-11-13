using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace UI.Minigames.WheelFortune
{
    public interface IWheelFortuneConfig
    {
        IReadOnlyList<WheelFortuneSegment> Segments { get; }
        Vector2Int FullSpinsRange { get; }
        Vector2 DurationRange { get; }
        Ease SpinEase { get; }
        WheelFortuneSegment GetRandomSegment(out int segmentIndex);
    }

    /// <summary>
    /// ScriptableObject that keeps wheel setup and binds itself into Zenject.
    /// </summary>
    [CreateAssetMenu(menuName = "Minigames/Wheel Fortune/Config", fileName = "WheelFortuneConfig")]
    public sealed class WheelFortuneConfig : ScriptableObjectInstaller<WheelFortuneConfig>, IWheelFortuneConfig
    {
        [SerializeField] private List<WheelFortuneSegment> segments = new();
        [SerializeField] private Vector2Int fullSpinsRange = new(3, 6);
        [SerializeField] private Vector2 durationRange = new(2.5f, 4.5f);
        [SerializeField] private Ease spinEase = Ease.OutQuart;

        public IReadOnlyList<WheelFortuneSegment> Segments => segments;
        public Vector2Int FullSpinsRange => fullSpinsRange;
        public Vector2 DurationRange => durationRange;
        public Ease SpinEase => spinEase;

        public override void InstallBindings()
        {
            Container.Bind<IWheelFortuneConfig>()
                .FromInstance(this)
                .AsSingle();
        }

        public WheelFortuneSegment GetRandomSegment(out int segmentIndex)
        {
            if (segments == null || segments.Count == 0)
                throw new InvalidOperationException("Wheel fortune config has no segments configured.");

            var pick = UnityEngine.Random.value * TotalWeight();
            var cumulative = 0f;
            for (var i = 0; i < segments.Count; i++)
            {
                var weight = Mathf.Max(0.01f, segments[i].Weight);
                cumulative += weight;
                if (pick <= cumulative)
                {
                    segmentIndex = i;
                    return segments[i];
                }
            }

            segmentIndex = segments.Count - 1;
            return segments[segmentIndex];
        }

        private float TotalWeight()
        {
            var total = 0f;
            foreach (var segment in segments)
                total += Mathf.Max(0.01f, segment.Weight);
            return total;
        }

        private void OnValidate()
        {
            fullSpinsRange.x = Mathf.Max(1, fullSpinsRange.x);
            fullSpinsRange.y = Mathf.Max(fullSpinsRange.x, fullSpinsRange.y);
            durationRange.x = Mathf.Max(0.1f, Mathf.Min(durationRange.x, durationRange.y));
            durationRange.y = Mathf.Max(durationRange.x, durationRange.y);
        }
    }
}
