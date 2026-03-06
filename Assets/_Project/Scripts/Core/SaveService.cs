using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Core
{
    public class SaveService : ISaveService
    {
        private const string KEY_DIAMONDS = "diamonds";
        private const string KEY_UNLOCKED = "unlocked_";
        private const string KEY_LAST_EPISODE = "last_episode_";
        private const string KEY_SAVED_NODE = "saved_node_";
        private const string KEY_SAVED_AFFECTION = "saved_affection_";
        private const string KEY_SAVED_FLAGS = "saved_flags_";
        private const string KEY_SAVED_BACKGROUND = "saved_background_";

        private int _diamonds;
        private Dictionary<string, bool> _unlockedCharacters = new Dictionary<string, bool>();
        private Dictionary<string, int> _lastPlayedEpisodes = new Dictionary<string, int>();

        public void Save()
        {
            PlayerPrefs.SetInt(KEY_DIAMONDS, _diamonds);
            foreach (var kvp in _unlockedCharacters)
                PlayerPrefs.SetInt(KEY_UNLOCKED + kvp.Key, kvp.Value ? 1 : 0);
            foreach (var kvp in _lastPlayedEpisodes)
                PlayerPrefs.SetInt(KEY_LAST_EPISODE + kvp.Key, kvp.Value);
            PlayerPrefs.Save();
            Debug.Log("[SaveService] Oyun kaydedildi.");
        }

        public void Load()
        {
            _diamonds = PlayerPrefs.GetInt(KEY_DIAMONDS, 15);
            Debug.Log($"[SaveService] Oyun yüklendi. Elmas: {_diamonds}");
        }

        public int GetDiamonds() => _diamonds;

        public void SetDiamonds(int amount)
        {
            _diamonds = Mathf.Max(0, amount);
            Save();
        }

        public void AddDiamonds(int amount)
        {
            _diamonds = Mathf.Max(0, _diamonds + amount);
            Save();
        }

        public int GetLastPlayedEpisode(string characterId)
        {
            return _lastPlayedEpisodes.TryGetValue(characterId, out var episode) ? episode : 0;
        }

        public void SetLastPlayedEpisode(string characterId, int episodeIndex)
        {
            _lastPlayedEpisodes[characterId] = episodeIndex;
            Save();
        }

        public bool IsCharacterUnlocked(string characterId)
        {
            return _unlockedCharacters.TryGetValue(characterId, out var unlocked) && unlocked;
        }

        public void UnlockCharacter(string characterId)
        {
            _unlockedCharacters[characterId] = true;
            Save();
        }

        public string GetSavedNodeId(string characterId)
        {
            return PlayerPrefs.GetString(KEY_SAVED_NODE + characterId, "");
        }

        public void SaveProgress(string characterId, string nodeId, int affection, List<string> flags, string backgroundId = "")
        {
            PlayerPrefs.SetString(KEY_SAVED_NODE + characterId, nodeId);
            PlayerPrefs.SetInt(KEY_SAVED_AFFECTION + characterId, affection);
            PlayerPrefs.SetString(KEY_SAVED_FLAGS + characterId, string.Join(",", flags));
            PlayerPrefs.SetString(KEY_SAVED_BACKGROUND + characterId, backgroundId);
            PlayerPrefs.Save();
            Debug.Log($"[SaveService] Ýlerleme kaydedildi. Karakter: {characterId}, Node: {nodeId}");
        }

        public (int affection, List<string> flags) LoadProgress(string characterId)
        {
            int affection = PlayerPrefs.GetInt(KEY_SAVED_AFFECTION + characterId, 0);
            string flagsStr = PlayerPrefs.GetString(KEY_SAVED_FLAGS + characterId, "");
            var flags = string.IsNullOrEmpty(flagsStr)
                ? new List<string>()
                : new List<string>(flagsStr.Split(','));
            return (affection, flags);
        }

        public string GetSavedBackgroundId(string characterId)
        {
            return PlayerPrefs.GetString(KEY_SAVED_BACKGROUND + characterId, "");
        }

        public void DeleteProgress(string characterId)
        {
            PlayerPrefs.DeleteKey(KEY_SAVED_NODE + characterId);
            PlayerPrefs.DeleteKey(KEY_SAVED_AFFECTION + characterId);
            PlayerPrefs.DeleteKey(KEY_SAVED_FLAGS + characterId);
            PlayerPrefs.DeleteKey(KEY_SAVED_BACKGROUND + characterId);
            PlayerPrefs.Save();
            Debug.Log($"[SaveService] Ýlerleme silindi. Karakter: {characterId}");
        }

        public void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            _diamonds = 15;
            _unlockedCharacters.Clear();
            _lastPlayedEpisodes.Clear();
            Debug.Log("[SaveService] Tüm veriler sýfýrlandý.");
        }
    }
}