using System;
using UnityEngine;

namespace UI.DailyRewards
{
    /// <summary>
    /// PlayerPrefs-backed implementation that enforces a fixed cooldown between reward claims.
    /// </summary>
    public sealed class PlayerPrefsDailyRewardsService : IDailyRewardsService
    {
        private readonly string _progressKey;
        private readonly string _lastClaimKey;
        private readonly int _totalDays;
        private readonly bool _loopAfterCompletion;
        private readonly TimeSpan _claimCooldown;

        public event Action<int> RewardClaimed;

        public PlayerPrefsDailyRewardsService(string prefsKeyPrefix,
            int totalDays,
            TimeSpan claimCooldown,
            bool loopAfterCompletion)
        {
            if (string.IsNullOrWhiteSpace(prefsKeyPrefix))
                prefsKeyPrefix = "DailyRewards";

            _progressKey = prefsKeyPrefix + ".Progress";
            _lastClaimKey = prefsKeyPrefix + ".LastClaim";
            _totalDays = Mathf.Max(0, totalDays);
            _claimCooldown = claimCooldown <= TimeSpan.Zero ? TimeSpan.FromHours(24) : claimCooldown;
            _loopAfterCompletion = loopAfterCompletion;
        }

        public int CurrentDayIndex
        {
            get
            {
                if (_totalDays == 0)
                    return 0;

                var progress = Mathf.Clamp(ReadProgress(), 0, _totalDays);
                return progress >= _totalDays ? _totalDays - 1 : progress;
            }
        }

        public int ClaimedDaysInCurrentCycle => Mathf.Clamp(ReadProgress(), 0, _totalDays);

        public bool HasCompletedAllRewards => !_loopAfterCompletion && ReadProgress() >= _totalDays && _totalDays > 0;

        public bool CanClaim(DateTime utcNow)
        {
            if (_totalDays == 0)
                return false;

            EnsureCycleResetIfNeeded(utcNow);

            if (HasCompletedAllRewards)
                return false;

            var lastClaim = ReadLastClaimDate();
            if (lastClaim == null)
                return true;

            var elapsed = utcNow - lastClaim.Value;
            return elapsed >= _claimCooldown;
        }

        public bool TryClaim(DateTime utcNow, out int claimedDayIndex)
        {
            claimedDayIndex = CurrentDayIndex;

            if (_totalDays == 0)
                return false;

            EnsureCycleResetIfNeeded(utcNow);

            if (HasCompletedAllRewards)
                return false;

            if (!CanClaim(utcNow))
                return false;

            var progress = Mathf.Clamp(ReadProgress(), 0, _totalDays);
            claimedDayIndex = progress >= _totalDays ? _totalDays - 1 : progress;

            progress = Mathf.Min(progress + 1, _totalDays);
            WriteProgress(progress);
            WriteLastClaimDate(utcNow);
            PlayerPrefs.Save();
            RewardClaimed?.Invoke(claimedDayIndex);
            return true;
        }

        private void EnsureCycleResetIfNeeded(DateTime utcNow)
        {
            if (!_loopAfterCompletion || _totalDays == 0)
                return;

            var progress = ReadProgress();
            if (progress < _totalDays)
                return;

            var lastClaim = ReadLastClaimDate();
            if (lastClaim == null)
                return;

            if (utcNow - lastClaim.Value < _claimCooldown)
                return;

            WriteProgress(0);
            PlayerPrefs.Save();
        }

        private int ReadProgress() => PlayerPrefs.GetInt(_progressKey, 0);

        private void WriteProgress(int value) => PlayerPrefs.SetInt(_progressKey, Mathf.Clamp(value, 0, _totalDays));

        private DateTime? ReadLastClaimDate()
        {
            var raw = PlayerPrefs.GetString(_lastClaimKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
                return null;

            return long.TryParse(raw, out var binary)
                ? DateTime.FromBinary(binary)
                : (DateTime?)null;
        }

        private void WriteLastClaimDate(DateTime dateTimeUtc)
        {
            PlayerPrefs.SetString(_lastClaimKey, dateTimeUtc.ToBinary().ToString());
        }
    }
}
