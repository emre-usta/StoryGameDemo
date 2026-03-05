using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoryGame.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string firstSceneName = "MainMenu";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeServices();
            LoadFirstScene();
        }

        private void InitializeServices()
        {
            Debug.Log("[Bootstrap] Servisler baþlatýlýyor...");

            var saveService = new SaveService();
            saveService.Load();
            ServiceLocator.Register<ISaveService>(saveService);

            var diamondService = new DiamondService(saveService);
            ServiceLocator.Register<IDiamondService>(diamondService);

            var audioService = GetComponentInChildren<AudioService>();
            if (audioService != null)
                ServiceLocator.Register<IAudioService>(audioService);
            else
                Debug.LogWarning("[Bootstrap] AudioService bulunamadý!");

            var dailyRewardService = new DailyRewardService(saveService, diamondService);
            ServiceLocator.Register<DailyRewardService>(dailyRewardService);

            Debug.Log($"[Bootstrap] Servisler hazýr. Mevcut elmas: {diamondService.GetAmount()}");
            //saveService.ResetAll();
        }

        private void LoadFirstScene()
        {
            Debug.Log($"[Bootstrap] {firstSceneName} sahnesi yükleniyor...");
            SceneManager.LoadScene(firstSceneName);
        }
    }
}