using System;
using UnityEngine;
using StoryGame.Core;
using StoryGame.Characters;

namespace StoryGame.Dialogue
{
    public class DialogueEngine : MonoBehaviour
    {
        private DialogueData _currentData;
        private DialogueNode _currentNode;
        private CharacterState _characterState;
        private int _currentEpisodeIndex;

        // Events - UI bu eventleri dinleyecek
        public event Action<DialogueNode> OnNarrationNode;
        public event Action<DialogueNode> OnDialogueNode;
        public event Action<DialogueNode> OnChoiceNode;
        public event Action<EndingType> OnEpisodeEnded;

        public void StartEpisode(DialogueData data, CharacterState characterState)
        {
            _currentData = data;
            _characterState = characterState;
            _currentEpisodeIndex = data.episodeIndex;

            Debug.Log($"[DialogueEngine] Bölüm baþladý: {data.episodeId}");

            var firstNode = data.GetFirstNode();
            if (firstNode != null)
                PlayNode(firstNode);
        }

        public void Advance()
        {
            if (_currentNode == null) return;
            if (_currentNode.type == NodeType.Choice) return;
            if (string.IsNullOrEmpty(_currentNode.nextNodeId))
            {
                EndEpisode();
                return;
            }
            PlayNode(_currentData.GetNode(_currentNode.nextNodeId));
        }

        public void SelectChoice(int choiceIndex)
        {
            if (_currentNode == null || _currentNode.type != NodeType.Choice) return;
            if (choiceIndex < 0 || choiceIndex >= _currentNode.choices.Count) return;

            var choice = _currentNode.choices[choiceIndex];

            // Diamond kontrolü
            if (choice.isDiamond)
            {
                var diamonds = ServiceLocator.Get<IDiamondService>();
                if (!diamonds.TrySpend(choice.diamondCost))
                {
                    Debug.Log("[DialogueEngine] Yetersiz elmas.");
                    return;
                }
            }

            // Effect uygula
            ApplyEffect(choice.effect);

            // Sonraki node'a geç
            if (!string.IsNullOrEmpty(choice.nextNodeId))
                PlayNode(_currentData.GetNode(choice.nextNodeId));
            else
                EndEpisode();
        }

        private void PlayNode(DialogueNode node)
        {
            _currentNode = node;

            switch (node.type)
            {
                case NodeType.Narration:
                    OnNarrationNode?.Invoke(node);
                    break;
                case NodeType.Dialogue:
                    OnDialogueNode?.Invoke(node);
                    break;
                case NodeType.Choice:
                    OnChoiceNode?.Invoke(node);
                    break;
            }
        }

        private void ApplyEffect(ChoiceEffect effect)
        {
            if (effect == null) return;

            if (effect.affection != 0)
                _characterState.ModifyAffection(effect.affection);

            if (!string.IsNullOrEmpty(effect.setFlag))
                _characterState.SetFlag(effect.setFlag, _currentEpisodeIndex);
        }

        private void EndEpisode()
        {
            _characterState.ExpireFlags(_currentEpisodeIndex + 1);

            var ending = _characterState.CalculateEnding();
            Debug.Log($"[DialogueEngine] Bölüm bitti. Ending: {ending}, Affection: {_characterState.affectionPoints}");

            OnEpisodeEnded?.Invoke(ending);
        }

        public void StartEpisodeFromNode(DialogueData data, CharacterState characterState, string nodeId)
        {
            _currentData = data;
            _characterState = characterState;
            _currentEpisodeIndex = data.episodeIndex;
            Debug.Log($"[DialogueEngine] Kaldýðý yerden devam: {nodeId}");
            var node = data.GetNode(nodeId);
            if (node != null)
                PlayNode(node);
            else
                StartEpisode(data, characterState);
        }
    }
}