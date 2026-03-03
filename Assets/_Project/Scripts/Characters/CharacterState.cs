using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Characters
{
    public enum EndingType
    {
        None,
        DeepBond,
        PassionateChaos,
        CasualFriend,
        ColdGoodbye,
        SecretFaceoff
    }

    public enum AffectionTier
    {
        Cold,       // 0-19
        Neutral,    // 20-39
        Friendly,   // 40-59
        Close,      // 60-74
        Devoted     // 75-100
    }

    [System.Serializable]
    public class CharacterState
    {
        public string characterId;

        [Range(0, 100)] public int affectionPoints;

        // Flags - sadece 1 bölüm taþýnýr
        public bool trustEstablished;
        public bool secretDiscovered;
        public bool recklessPath;

        // Hangi bölümde set edildi
        private Dictionary<string, int> _flagSetAtEpisode = new Dictionary<string, int>();

        public void SetFlag(string flagName, int currentEpisode)
        {
            switch (flagName)
            {
                case "trustEstablished": trustEstablished = true; break;
                case "secretDiscovered": secretDiscovered = true; break;
                case "recklessPath": recklessPath = true; break;
                default: Debug.LogWarning($"[CharacterState] Bilinmeyen flag: {flagName}"); return;
            }
            _flagSetAtEpisode[flagName] = currentEpisode;
            Debug.Log($"[CharacterState] Flag set edildi: {flagName} (Bölüm {currentEpisode})");
        }

        public void ExpireFlags(int newEpisode)
        {
            var toExpire = new List<string>();
            foreach (var kvp in _flagSetAtEpisode)
            {
                if (newEpisode - kvp.Value > 1)
                    toExpire.Add(kvp.Key);
            }

            foreach (var flag in toExpire)
            {
                switch (flag)
                {
                    case "trustEstablished": trustEstablished = false; break;
                    case "secretDiscovered": secretDiscovered = false; break;
                    case "recklessPath": recklessPath = false; break;
                }
                _flagSetAtEpisode.Remove(flag);
                Debug.Log($"[CharacterState] Flag süresi doldu: {flag}");
            }
        }

        public void ModifyAffection(int amount)
        {
            affectionPoints = Mathf.Clamp(affectionPoints + amount, 0, 100);
            Debug.Log($"[CharacterState] Affection: {affectionPoints} ({(amount >= 0 ? "+" : "")}{amount})");
        }

        public AffectionTier GetTier()
        {
            return affectionPoints switch
            {
                >= 75 => AffectionTier.Devoted,
                >= 60 => AffectionTier.Close,
                >= 40 => AffectionTier.Friendly,
                >= 20 => AffectionTier.Neutral,
                _ => AffectionTier.Cold
            };
        }

        public EndingType CalculateEnding()
        {
            if (secretDiscovered) return EndingType.SecretFaceoff;
            if (affectionPoints >= 75 && trustEstablished) return EndingType.DeepBond;
            if (affectionPoints >= 60 && recklessPath) return EndingType.PassionateChaos;
            if (affectionPoints >= 40) return EndingType.CasualFriend;
            return EndingType.ColdGoodbye;
        }
    }
}