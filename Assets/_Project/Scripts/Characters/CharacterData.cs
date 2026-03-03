using UnityEngine;

namespace StoryGame.Characters
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "StoryGame/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Temel Bilgiler")]
        public string characterId;
        public string characterName;
        [TextArea(2, 4)] public string description;

        [Header("Görsel")]
        public Sprite portrait;
        public Sprite[] outfits;

        [Header("Ses")]
        public AudioClip theme;

        [Header("Kilit")]
        public bool isUnlockedByDefault;
        public int unlockDiamondCost;

        [Header("Kiþilik")]
        [Range(0, 100)] public int warmth;
        [Range(0, 100)] public int mystery;
        [Range(0, 100)] public int confidence;
        [Range(0, 100)] public int playfulness;
    }
}