using UnityEngine;

namespace StoryGame.Core
{
    public class DiamondService : IDiamondService
    {
        private readonly ISaveService _saveService;

        public DiamondService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public int GetAmount()
        {
            return _saveService.GetDiamonds();
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _saveService.AddDiamonds(amount);
            Debug.Log($"[DiamondService] +{amount} elmas eklendi. Toplam: {GetAmount()}");
        }

        public bool TrySpend(int amount)
        {
            if (!HasEnough(amount))
            {
                Debug.Log($"[DiamondService] Yetersiz elmas. Gereken: {amount}, Mevcut: {GetAmount()}");
                return false;
            }
            _saveService.SetDiamonds(GetAmount() - amount);
            Debug.Log($"[DiamondService] -{amount} elmas harcandý. Kalan: {GetAmount()}");
            return true;
        }

        public bool HasEnough(int amount)
        {
            return GetAmount() >= amount;
        }
    }
}