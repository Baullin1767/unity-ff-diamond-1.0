using System;

namespace UI.DailyRewards
{
    /// <summary>
    /// Provides read/write access to the player's daily reward progress.
    /// </summary>
    public interface IDailyRewardsService
    {
        /// <summary>
        /// Index (zero-based) of the next reward in the sequence. Clamped to the configured reward count.
        /// </summary>
        int CurrentDayIndex { get; }

        /// <summary>
        /// Number of rewards claimed in the current cycle.
        /// </summary>
        int ClaimedDaysInCurrentCycle { get; }

        /// <summary>
        /// Returns true when the player has already collected every reward in the sequence
        /// and the service is configured not to loop.
        /// </summary>
        bool HasCompletedAllRewards { get; }

        /// <summary>
        /// Whether the player may claim the next reward at the specified time.
        /// </summary>
        bool CanClaim(DateTime utcNow);

        /// <summary>
        /// Attempts to claim the next reward. Returns true if successful and outputs the claimed day index.
        /// </summary>
        bool TryClaim(DateTime utcNow, out int claimedDayIndex);

        /// <summary>
        /// Raised whenever a reward is successfully claimed. Parameter carries the zero-based day index.
        /// </summary>
        event Action<int> RewardClaimed;
    }
}
