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

            // Ýleride buraya servisler eklenecek:
            // ServiceLocator.Register<ISaveService>(new SaveService());
            // ServiceLocator.Register<IDiamondService>(new DiamondService());
            // ServiceLocator.Register<IAudioService>(new AudioService());

            Debug.Log("[Bootstrap] Servisler hazýr.");
        }

        private void LoadFirstScene()
        {
            Debug.Log($"[Bootstrap] {firstSceneName} sahnesi yükleniyor...");
            SceneManager.LoadScene(firstSceneName);
        }
    }
}