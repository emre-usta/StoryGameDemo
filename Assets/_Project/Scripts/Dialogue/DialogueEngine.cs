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

            Debug.Log($"[DialogueEngine] Bölüm başladı: {data.episodeId}");

            var firstNode = data.GetFirstNode();
            if (firstNode != null)
                PlayNode(firstNode);
        }

        public void StartEpisodeFromNode(DialogueData data, CharacterState characterState, string nodeId)
        {
            _currentData = data;
            _characterState = characterState;
            _currentEpisodeIndex = data.episodeIndex;
            Debug.Log($"[DialogueEngine] Kaldığı yerden devam: {nodeId}");
            var node = data.GetNode(nodeId);
            if (node != null)
                PlayNode(node);
            else
                StartEpisode(data, characterState);
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
            if (node == null)
            {
                Debug.LogError("[DialogueEngine] PlayNode: null node, bölüm sonlandırılıyor.");
                EndEpisode();
                return;
            }

            // ── DURUM 1: requiredFlag dolu → flag kontrolü ──────────────────
            if (!string.IsNullOrEmpty(node.requiredFlag))
            {
                bool flagActive = _characterState.GetFlag(node.requiredFlag);
                Debug.Log($"[DialogueEngine] Flag check — {node.requiredFlag}: {flagActive}");

                if (!flagActive && !string.IsNullOrEmpty(node.altNodeId))
                {
                    PlayNode(_currentData.GetNode(node.altNodeId));
                    return;
                }
                // flag aktifse (veya altNodeId yoksa) normal devam
            }
            // ── DURUM 2: requiredFlag boş + altNodeId dolu = affection kapısı
            // EP03 node_111: CasualFriend (≥40) vs ColdGoodbye (<40)
            else if (!string.IsNullOrEmpty(node.altNodeId))
            {
                var ending = _characterState.CalculateEnding();
                bool casualOrBetter = ending != EndingType.ColdGoodbye;
                Debug.Log($"[DialogueEngine] Affection gate @ {node.id} — " +
                          $"Affection: {_characterState.affectionPoints}, Ending: {ending} → " +
                          $"{(casualOrBetter ? node.nextNodeId : node.altNodeId)}");

                if (!casualOrBetter)
                {
                    PlayNode(_currentData.GetNode(node.altNodeId));
                    return;
                }
                // casualOrBetter → normal devam (nextNodeId ile)
            }

            // Normal node — oynat
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
            // Önce ending hesapla — flagler hâlâ aktifken
            var ending = _characterState.CalculateEnding();
            Debug.Log($"[DialogueEngine] Bölüm bitti. Ending: {ending}, Affection: {_characterState.affectionPoints}");

            // Sonra süresi dolan flagleri temizle
            _characterState.ExpireFlags(_currentEpisodeIndex + 1);

            OnEpisodeEnded?.Invoke(ending);
        }
    }
}