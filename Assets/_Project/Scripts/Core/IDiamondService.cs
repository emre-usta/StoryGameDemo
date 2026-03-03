namespace StoryGame.Core
{
    public interface IDiamondService
    {
        int GetAmount();
        void Add(int amount);
        bool TrySpend(int amount);
        bool HasEnough(int amount);
    }
}