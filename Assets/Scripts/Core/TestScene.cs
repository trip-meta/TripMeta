using UnityEngine;
using TripMeta.Core;

namespace TripMeta.Testing
{
    /// <summary>
    /// 测试场景脚本 - 用于验证基础系统功能
    /// </summary>
    public class TestScene : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private float statusUpdateInterval = 1.0f;
        
        private GameManager gameManager;
        private float lastStatusUpdate = 0f;
        
        void Start()
        {
            // 查找GameManager
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("未找到GameManager，请确保场景中包含GameManager");
                return;
            }
            
            Debug.Log("=== TripMeta 测试场景启动 ===");
            Debug.Log("基础系统测试开始...");
            
            // 等待系统初始化完成
            StartCoroutine(WaitForInitialization());
        }
        
        void Update()
        {
            if (showDebugInfo && Time.time - lastStatusUpdate > statusUpdateInterval)
            {
                ShowSystemStatus();
                lastStatusUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// 等待系统初始化完成
        /// </summary>
        private System.Collections.IEnumerator WaitForInitialization()
        {
            while (gameManager != null && !gameManager.IsInitialized)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (gameManager != null && gameManager.IsInitialized)
            {
                Debug.Log("✅ 系统初始化完成，开始功能测试");
                RunBasicTests();
            }
            else
            {
                Debug.LogError("❌ 系统初始化失败");
            }
        }
        
        /// <summary>
        /// 运行基础测试
        /// </summary>
        private void RunBasicTests()
        {
            Debug.Log("=== 基础功能测试 ===");
            
            // 测试VR系统
            TestVRSystem();
            
            // 测试控制器系统
            TestControllerSystem();
            
            // 测试配置系统
            TestConfigSystem();
            
            Debug.Log("=== 基础功能测试完成 ===");
        }
        
        /// <summary>
        /// 测试VR系统
        /// </summary>
        private void TestVRSystem()
        {
            if (gameManager.VRManager != null)
            {
                Debug.Log("✅ VR系统: 正常");
                
                // 测试VR状态
                bool vrEnabled = gameManager.VRManager.IsVREnabled;
                Debug.Log($"   VR启用状态: {vrEnabled}");
                
                if (vrEnabled)
                {
                    var headPosition = gameManager.VRManager.GetHeadPosition();
                    var headRotation = gameManager.VRManager.GetHeadRotation();
                    Debug.Log($"   头部位置: {headPosition}");
                    Debug.Log($"   头部旋转: {headRotation.eulerAngles}");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ VR系统: 未初始化");
            }
        }
        
        /// <summary>
        /// 测试控制器系统
        /// </summary>
        private void TestControllerSystem()
        {
            if (gameManager.ControllerManager != null)
            {
                Debug.Log("✅ 控制器系统: 正常");
                
                // 测试控制器连接状态
                bool leftConnected = gameManager.ControllerManager.LeftControllerConnected;
                bool rightConnected = gameManager.ControllerManager.RightControllerConnected;
                
                Debug.Log($"   左控制器: {(leftConnected ? "已连接" : "未连接")}");
                Debug.Log($"   右控制器: {(rightConnected ? "已连接" : "未连接")}");
            }
            else
            {
                Debug.LogWarning("⚠️ 控制器系统: 未初始化");
            }
        }
        
        /// <summary>
        /// 测试配置系统
        /// </summary>
        private void TestConfigSystem()
        {
            if (gameManager.Config != null)
            {
                Debug.Log("✅ 配置系统: 正常");
                
                // 测试配置加载
                var vrConfig = gameManager.Config.VRSettings;
                if (vrConfig != null)
                {
                    Debug.Log($"   目标帧率: {vrConfig.TargetFrameRate}");
                    Debug.Log($"   渲染分辨率: {vrConfig.RenderScale}");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ 配置系统: 未初始化");
            }
        }
        
        /// <summary>
        /// 显示系统状态
        /// </summary>
        private void ShowSystemStatus()
        {
            if (gameManager == null || !gameManager.IsInitialized)
                return;
                
            // 显示基础信息
            Debug.Log($"[状态] FPS: {(1f / Time.unscaledDeltaTime):F1} | " +
                     $"内存: {(System.GC.GetTotalMemory(false) / 1024f / 1024f):F1}MB");
            
            // 显示VR信息
            if (gameManager.VRManager != null && gameManager.VRManager.IsVREnabled)
            {
                var headPos = gameManager.VRManager.GetHeadPosition();
                Debug.Log($"[VR] 头部位置: ({headPos.x:F2}, {headPos.y:F2}, {headPos.z:F2})");
            }
            
            // 显示控制器信息
            if (gameManager.ControllerManager != null)
            {
                string controllerStatus = $"[控制器] 左:{(gameManager.ControllerManager.LeftControllerConnected ? "✓" : "✗")} " +
                                        $"右:{(gameManager.ControllerManager.RightControllerConnected ? "✓" : "✗")}";
                Debug.Log(controllerStatus);
            }
        }
        
        /// <summary>
        /// GUI显示（用于编辑器测试）
        /// </summary>
        void OnGUI()
        {
            if (!showDebugInfo || gameManager == null)
                return;
                
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== TripMeta 系统状态 ===");
            
            // 系统状态
            GUILayout.Label($"系统初始化: {(gameManager.IsInitialized ? "✅" : "❌")}");
            GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F1}");
            
            // VR状态
            if (gameManager.VRManager != null)
            {
                GUILayout.Label($"VR系统: {(gameManager.VRManager.IsVREnabled ? "✅" : "❌")}");
            }
            
            // 控制器状态
            if (gameManager.ControllerManager != null)
            {
                GUILayout.Label($"左控制器: {(gameManager.ControllerManager.LeftControllerConnected ? "✅" : "❌")}");
                GUILayout.Label($"右控制器: {(gameManager.ControllerManager.RightControllerConnected ? "✅" : "❌")}");
            }
            
            GUILayout.EndArea();
        }
    }
}