using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Core
{
    public class SaveService : ISaveService
    {
        private const string KEY_DIAMONDS = "diamonds";
        private const string KEY_UNLOCKED = "unlocked_";
        private const string KEY_LAST_EPISODE = "last_episode_";

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
            _diamonds = PlayerPrefs.GetInt(KEY_DIAMONDS, 15); // Baþlangýç: 15 elmas
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