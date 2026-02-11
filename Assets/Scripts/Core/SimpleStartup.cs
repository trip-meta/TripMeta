using UnityEngine;
using System.Collections.Tasks;
using TripMeta.AI;

namespace TripMeta.Core
{
    /// <summary>
    /// 简化的应用启动器 - 用于快速启动和测试
    /// </summary>
    public class SimpleStartup : MonoBehaviour
    {
        [Header("Startup Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool showDebugInfo = true;

        [Header("Scene References")]
        [SerializeField] private VRManager vrManager;
        [SerializeField] private AIServiceManager aiManager;
        [SerializeField] private AITourGuide tourGuide;

        public static SimpleStartup Instance { get; private set; }
        public bool IsInitialized { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        async void Start()
        {
            if (autoInitialize)
            {
                await InitializeAsync();
            }
        }

        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
                return true;

            Debug.Log("[SimpleStartup] Starting initialization...");

            // Find or create managers
            if (vrManager == null)
                vrManager = FindObjectOfType<VRManager>();
            if (aiManager == null)
                aiManager = FindObjectOfType<AIServiceManager>();
            if (tourGuide == null)
                tourGuide = FindObjectOfType<AITourGuide>();

            // Initialize VR
            if (vrManager != null)
            {
                Debug.Log("[SimpleStartup] Initializing VR...");
                vrManager.InitializeVR();
                await Task.Delay(500);
            }

            // Wait for AI Manager
            if (aiManager != null)
            {
                Debug.Log("[SimpleStartup] Waiting for AI Manager...");
                int timeout = 50; // 5 seconds
                while (!aiManager.isInitialized && timeout > 0)
                {
                    await Task.Delay(100);
                    timeout--;
                }
                Debug.Log($"[SimpleStartup] AI Manager ready: {aiManager.isInitialized}");
            }

            // Initialize Tour Guide
            if (tourGuide != null)
            {
                Debug.Log("[SimpleStartup] Initializing Tour Guide...");
                await Task.Delay(500);
            }

            IsInitialized = true;
            Debug.Log("[SimpleStartup] Initialization complete!");

            return true;
        }

        void Update()
        {
            if (showDebugInfo && IsInitialized)
            {
                // Optional: Show debug info on screen
            }
        }

        public VRManager GetVRManager() => vrManager;
        public AIServiceManager GetAIManager() => aiManager;
        public AITourGuide GetTourGuide() => tourGuide;
    }
}
