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

        // Negatif affection desteklenir; görsel bar 0'da sabitlenir ama hesaplama gerçek değerle yapılır
        public int affectionPoints;

        // Flags - her flag'in kendi ömrü var (_flagLifetime'a bak)
        public bool trustEstablished;
        public bool secretDiscovered;
        public bool recklessPath;
        public bool smoothTalker;
        public bool deepConnection;
        public bool firstCrack;      // EP01 C4 💎 → EP03'te etkili
        public bool sharedSilence;   // EP01 C7 💎 → EP02'de etkili

        // Hangi bölümde set edildi
        private Dictionary<string, int> _flagSetAtEpisode = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> _flagLifetime = new Dictionary<string, int>()
        {
            { "smoothTalker",     1  },
            { "deepConnection",   2  },
            { "trustEstablished", 2  },
            { "secretDiscovered", 99 },
            { "recklessPath",     1  },
            { "firstCrack",       2  },  // EP01 → EP03
            { "sharedSilence",    1  },  // EP01 → EP02
        };

        public void SetFlag(string flagName, int currentEpisode)
        {
            switch (flagName)
            {
                case "trustEstablished": trustEstablished = true; break;
                case "secretDiscovered": secretDiscovered = true; break;
                case "recklessPath": recklessPath = true; break;
                case "smoothTalker": smoothTalker = true; break;
                case "deepConnection": deepConnection = true; break;
                case "firstCrack": firstCrack = true; break;
                case "sharedSilence": sharedSilence = true; break;
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
                int lifetime = _flagLifetime.TryGetValue(kvp.Key, out var l) ? l : 1;
                if (newEpisode - kvp.Value > lifetime)
                    toExpire.Add(kvp.Key);
            }
            foreach (var flag in toExpire)
            {
                switch (flag)
                {
                    case "trustEstablished": trustEstablished = false; break;
                    case "secretDiscovered": secretDiscovered = false; break;
                    case "recklessPath": recklessPath = false; break;
                    case "smoothTalker": smoothTalker = false; break;
                    case "deepConnection": deepConnection = false; break;
                    case "firstCrack": firstCrack = false; break;
                    case "sharedSilence": sharedSilence = false; break;
                }
                _flagSetAtEpisode.Remove(flag);
                Debug.Log($"[CharacterState] Flag süresi doldu: {flag}");
            }
        }

        // DialogueEngine'in flag kontrolü için — switch yerine tek metod
        public bool GetFlag(string flagName)
        {
            return flagName switch
            {
                "trustEstablished" => trustEstablished,
                "secretDiscovered" => secretDiscovered,
                "recklessPath" => recklessPath,
                "smoothTalker" => smoothTalker,
                "deepConnection" => deepConnection,
                "firstCrack" => firstCrack,
                "sharedSilence" => sharedSilence,
                _ => false
            };
        }

        public void ModifyAffection(int amount)
        {
            // Negatif değerler desteklenir; -999 alt sınır, 200 üst sınır
            affectionPoints = Mathf.Clamp(affectionPoints + amount, -999, 200);
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
            if (affectionPoints >= 50 && recklessPath) return EndingType.PassionateChaos;
            if (affectionPoints >= 40) return EndingType.CasualFriend;
            return EndingType.ColdGoodbye;
        }
    }
}