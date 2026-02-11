using UnityEngine;
using TripMeta.Tests;

namespace TripMeta.Demo
{
    /// <summary>
    /// 场景自动配置器 - 在运行时自动添加必要的测试组件
    /// </summary>
    public class SceneAutoConfigurator : MonoBehaviour
    {
        [Header("自动配置选项")]
        [SerializeField] private bool addRuntimeTest = true;
        [SerializeField] private bool addInteractiveDemo = true;

        void Awake()
        {
            // 只在编辑器中运行时添加组件
            #if UNITY_EDITOR
            if (addRuntimeTest && FindObjectOfType<RuntimeSystemTest>() == null)
            {
                GameObject testObj = new GameObject("RuntimeSystemTest");
                testObj.AddComponent<RuntimeSystemTest>();
                Debug.Log("[SceneAutoConfigurator] Added RuntimeSystemTest component");
            }

            if (addInteractiveDemo && FindObjectOfType<InteractiveDemo>() == null)
            {
                GameObject demoObj = new GameObject("InteractiveDemo");
                demoObj.AddComponent<InteractiveDemo>();
                Debug.Log("[SceneAutoConfigurator] Added InteractiveDemo component");
            }
            #endif
        }

        void Start()
        {
            // 自动销毁此组件，因为它的任务已经完成
            Destroy(this);
        }
    }
}
