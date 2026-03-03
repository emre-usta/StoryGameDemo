using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Dialogue
{
    public enum NodeType
    {
        Narration,
        Dialogue,
        Choice
    }

    [System.Serializable]
    public class ChoiceEffect
    {
        public int affection;
        public string setFlag;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
        public string nextNodeId;
        public ChoiceEffect effect;
        public bool isDiamond;
        public int diamondCost;
    }

    [System.Serializable]
    public class DialogueNode
    {
        public string id;
        public NodeType type;
        public string speaker;
        public string text;
        public string backgroundId;
        public string nextNodeId;
        public List<Choice> choices = new List<Choice>();
    }

    [CreateAssetMenu(fileName = "DialogueData", menuName = "StoryGame/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string episodeId;
        public string characterId;
        public int episodeIndex;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        public DialogueNode GetNode(string nodeId)
        {
            var node = nodes.Find(n => n.id == nodeId);
            if (node == null)
                Debug.LogError($"[DialogueData] Node bulunamadý: {nodeId}");
            return node;
        }

        public DialogueNode GetFirstNode()
        {
            if (nodes.Count == 0)
            {
                Debug.LogError($"[DialogueData] {episodeId} içinde hiç node yok!");
                return null;
            }
            return nodes[0];
        }
    }
}