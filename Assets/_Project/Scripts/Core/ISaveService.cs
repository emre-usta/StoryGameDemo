using System.Collections.Generic;

namespace StoryGame.Core
{
    public interface ISaveService
    {
        void Save();
        void Load();
        int GetDiamonds();
        void SetDiamonds(int amount);
        void AddDiamonds(int amount);
        int GetLastPlayedEpisode(string characterId);
        void SetLastPlayedEpisode(string characterId, int episodeIndex);
        bool IsCharacterUnlocked(string characterId);
        void UnlockCharacter(string characterId);
        void ResetAll();
        string GetSavedNodeId(string characterId);
        void SaveProgress(string characterId, string nodeId, int affection, List<string> flags, string backgroundId = "");
        (int affection, List<string> flags) LoadProgress(string characterId);
        void DeleteProgress(string characterId);
        string GetSavedBackgroundId(string characterId);
    }
}