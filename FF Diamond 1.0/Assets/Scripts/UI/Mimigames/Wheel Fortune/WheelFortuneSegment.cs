using System;
using UnityEngine;

namespace UI.Minigames.WheelFortune
{
    /// <summary>
    /// Single slice of the fortune wheel.
    /// </summary>
    [Serializable]
    public sealed class WheelFortuneSegment
    {
        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private int rewardAmount;
        [Min(0.01f)]
        [SerializeField] private float weight = 1f;
        [SerializeField] private Color color = Color.white;

        public string Id => string.IsNullOrWhiteSpace(id) ? label : id;
        public string Label => label;
        public int RewardAmount => rewardAmount;
        public float Weight => Mathf.Max(0.01f, weight);
        public Color Color => color;
    }
}
