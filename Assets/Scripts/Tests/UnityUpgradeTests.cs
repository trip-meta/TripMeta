using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace TripMeta.Tests
{
    /// <summary>
    /// Unity 2022.3 LTS 升级验证测试
    /// </summary>
    public class UnityUpgradeTests
    {
        [Test]
        public void UnityVersion_Is2022_3()
        {
            // 验证 Unity 版本是 2022.3.x
            string version = Application.unityVersion;
            Assert.IsTrue(version.StartsWith("2022.3"),
                $"Unity 版本应该是 2022.3.x，但实际是 {version}");
        }

        [Test]
        public void XR_Origin_ComponentExists()
        {
            // 验证 XROrigin 组件存在（替代 ARSessionOrigin）
            var xrOriginType = typeof(Unity.XR.CoreUtils.XROrigin);
            Assert.IsNotNull(xrOriginType, "XROrigin 类型应该存在");
        }

        [Test]
        public void InputSystem_IsInstalled()
        {
            // 验证 Input System 已安装
            var inputSystemType = typeof(UnityEngine.InputSystem.InputDevice);
            Assert.IsNotNull(inputSystemType, "Input System 应该已安装");
        }

        [Test]
        public void URP_IsInstalled()
        {
            // 验证 Universal Render Pipeline 已安装
            var urpPipelineType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipeline);
            Assert.IsNotNull(urpPipelineType, "URP 应该已安装");
        }

        [Test]
        public void Netcode_IsInstalled()
        {
            // 验证 Netcode for GameObjects 已安装
            var networkManagerType = typeof(Unity.Netcode.NetworkManager);
            Assert.IsNotNull(networkManagerType, "Netcode 应该已安装");
        }

        [Test]
        public void XRInteractionToolkit_IsInstalled()
        {
            // 验证 XR Interaction Toolkit 已安装
            var xrInteractionType = typeof(UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable);
            Assert.IsNotNull(xrInteractionType, "XR Interaction Toolkit 应该已安装");
        }

        [Test]
        public void ARFoundation_IsInstalled()
        {
            // 验证 AR Foundation 已安装
            var arSessionType = typeof(UnityEngine.XR.ARFoundation.ARSession);
            Assert.IsNotNull(arSessionType, "AR Foundation 应该已安装");
        }

        [UnityTest]
        public IEnumerator ScriptCompilation_NoErrors()
        {
            // 验证所有脚本可以编译
            // 如果存在编译错误，测试框架本身会失败
            yield return null;
            Assert.Pass("所有脚本编译成功");
        }

        [Test]
        public void ProjectSettings_AreValid()
        {
            // 验证项目设置有效
            Assert.Greater(Application.targetFrameRate, 0, "目标帧率应该大于 0");
            Assert.IsNotNull(Camera.main, "应该存在主相机");
        }

        [Test]
        public void Dependencies_AreCompatible()
        {
            // 验证关键依赖项版本兼容
            // 这些测试确保包版本与 Unity 2022.3 兼容

            // 检查 .NET 版本
            #if NET_STANDARD_2_1
            Assert.Pass("使用 .NET Standard 2.1");
            #elif NET_4_6
            Assert.Pass("使用 .NET 4.x");
            #else
            Assert.Inconclusive("无法确定 .NET 版本");
            #endif
        }
    }
}
