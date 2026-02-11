using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TripMeta.Video
{
    /// <summary>
    /// 命令行录制器 - 用于自动化构建中的视频录制
    /// </summary>
    public class CommandLineRecorder : MonoBehaviour
    {
        [Header("自动录制设置")]
        [SerializeField] private bool enableCommandLineRecording = true;
        [SerializeField] private float recordingDuration = 60f;
        [SerializeField] private int targetFrameRate = 60;

        [Header("场景设置")]
        [SerializeField] private bool suppressUI = true;
        [SerializeField] private int targetWidth = 1920;
        [SerializeField] private int targetHeight = 1080;

        private bool hasStartedRecording = false;

        private void Start()
        {
            // 检查命令行参数
            if (enableCommandLineRecording && HasCommandLineArg("-recordVideo"))
            {
                SetupForRecording();
                StartAutoRecording();
            }
        }

        private void SetupForRecording()
        {
            // 设置目标帧率
            Application.targetFrameRate = targetFrameRate;

            // 设置屏幕分辨率
            if (HasCommandLineArg("-width"))
            {
                string widthArg = GetCommandLineArg("-width");
                if (int.TryParse(widthArg, out int width))
                {
                    targetWidth = width;
                }
            }

            if (HasCommandLineArg("-height"))
            {
                string heightArg = GetCommandLineArg("-height");
                if (int.TryParse(heightArg, out int height))
                {
                    targetHeight = height;
                }
            }

            Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);

            // 隐藏UI（如果需要）
            if (suppressUI && HasCommandLineArg("-noUI"))
            {
                // 可以在这里隐藏不需要的UI元素
            }

            Debug.Log($"[CommandLineRecorder] 设置录制: {targetWidth}x{targetHeight} @ {targetFrameRate}fps");
        }

        private void StartAutoRecording()
        {
            if (hasStartedRecording) return;

            // 查找并启动录制器
            var recorder = FindObjectOfType<VideoRecorder>();
            if (recorder != null)
            {
                recorder.StartRecording();
                hasStartedRecording = true;

                // 设置自动停止
                Invoke(nameof(StopAutoRecording), recordingDuration);
            }
            else
            {
                Debug.LogWarning("[CommandLineRecorder] 未找到 VideoRecorder 组件");
            }
        }

        private void StopAutoRecording()
        {
            if (!hasStartedRecording) return;

            var recorder = FindObjectOfType<VideoRecorder>();
            if (recorder != null)
            {
                recorder.StopRecording();
            }

            hasStartedRecording = false;

            // 如果是命令行模式，录制完成后退出
            if (HasCommandLineArg("-quitAfterRecord"))
            {
                Debug.Log("[CommandLineRecorder] 录制完成，退出应用...");
                Invoke(nameof(QuitApplication), 1f);
            }
        }

        private void QuitApplication()
        {
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private bool HasCommandLineArg(string arg)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string s in args)
            {
                if (s.Equals(arg, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetCommandLineArg(string arg)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(arg, System.StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return "";
        }

        /// <summary>
        /// 获取命令行录制示例
        /// </summary>
        public static string GetUsageExample()
        {
            return @"命令行录制示例:

# Windows
TripMeta.exe -recordVideo -width 1920 -height 1080 -quitAfterRecord

# macOS
./TripMeta.app/Contents/MacOS/TripMeta -recordVideo -width 1920 -height 1080 -quitAfterRecord

# Linux
./TripMeta -recordVideo -width 1920 -height 1080 -quitAfterRecord

参数说明:
  -recordVideo          启用视频录制
  -width <像素>         设置视频宽度 (默认: 1920)
  -height <像素>        设置视频高度 (默认: 1080)
  -noUI                 隐藏UI元素
  -quitAfterRecord      录制完成后自动退出";
    }

        private void OnGUI()
        {
            // 显示录制状态
            if (hasStartedRecording)
            {
                GUI.color = Color.red;
                GUILayout.Label("● 录制中");
                GUI.color = Color.white;
            }
        }
    }
}
