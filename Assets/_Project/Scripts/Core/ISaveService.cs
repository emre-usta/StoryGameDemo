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
    }
}