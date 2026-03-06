using System;
using UnityEngine;

namespace StoryGame.Core
{
    public class DailyRewardService
    {
        private const string LAST_REWARD_KEY = "LastDailyReward";
        private const int DAILY_REWARD_AMOUNT = 2;

        private readonly ISaveService _saveService;
        private readonly IDiamondService _diamondService;

        public DailyRewardService(ISaveService saveService, IDiamondService diamondService)
        {
            _saveService = saveService;
            _diamondService = diamondService;
        }

        public bool IsDailyRewardAvailable()
        {
            string lastRewardStr = PlayerPrefs.GetString(LAST_REWARD_KEY, "");
            if (string.IsNullOrEmpty(lastRewardStr)) return true;

            if (DateTime.TryParse(lastRewardStr, out DateTime lastReward))
                return (DateTime.Now - lastReward).TotalHours >= 24;

            return true;
        }

        public int ClaimDailyReward()
        {
            if (!IsDailyRewardAvailable()) return 0;

            _diamondService.Add(DAILY_REWARD_AMOUNT);
            PlayerPrefs.SetString(LAST_REWARD_KEY, DateTime.Now.ToString());
            PlayerPrefs.Save();

            Debug.Log($"[DailyReward] Günlük ödül verildi: {DAILY_REWARD_AMOUNT} elmas");
            return DAILY_REWARD_AMOUNT;
        }

        public TimeSpan GetTimeUntilNextReward()
        {
            string lastRewardStr = PlayerPrefs.GetString(LAST_REWARD_KEY, "");
            if (string.IsNullOrEmpty(lastRewardStr)) return TimeSpan.Zero;

            if (DateTime.TryParse(lastRewardStr, out DateTime lastReward))
            {
                var nextReward = lastReward.AddHours(24);
                var remaining = nextReward - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }
    }
}