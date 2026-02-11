using UnityEngine;
using UnityEditor;
using TripMeta.Video;

namespace TripMeta.Editor
{
    /// <summary>
    /// 视频录制快速设置工具
    /// </summary>
    public class VideoRecorderSetup : EditorWindow
    {
        [MenuItem("TripMeta/Setup Video Recording")]
        public static void ShowWindow()
        {
            var window = GetWindow<VideoRecorderSetup>("视频录制设置");
            window.minSize = new Vector2(350, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("TripMeta 视频录制设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "快速设置视频录制系统。\n" +
                "选择要使用的录制方式并应用到当前场景。",
                MessageType.Info
            );

            EditorGUILayout.Space();

            DrawRecorderOptions();
            EditorGUILayout.Space();
            DrawQuickActions();
            EditorGUILayout.Space();
            DrawDocumentation();
        }

        private void DrawRecorderOptions()
        {
            EditorGUILayout.LabelField("录制器类型", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Unity Recorder API (推荐)\n" +
                "• 官方支持，稳定可靠\n" +
                "• 支持多种视频格式\n" +
                "• 需要安装 Recorder 包",
                MessageType.None
            );

            if (GUILayout.Button("设置 Unity Recorder", GUILayout.Height(35)))
            {
                SetupUnityRecorder();
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Screen Capture (简单)\n" +
                "• 无需额外包\n" +
                "• 输出截图序列\n" +
                "• 需要后期合成视频",
                MessageType.None
            );

            if (GUILayout.Button("设置 Screen Capture", GUILayout.Height(35)))
            {
                SetupScreenCapture();
            }
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("打开视频生成器"))
                {
                    VideoGeneratorWindow.ShowWindow();
                }
                if (GUILayout.Button("查看录制文件夹"))
                {
                    OpenRecordingFolder();
                }
            }

            if (GUILayout.Button("创建演示场景"))
            {
                CreateDemoScene();
            }
        }

        private void DrawDocumentation()
        {
            EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "使用步骤：\n" +
                "1. 选择录制器类型并设置\n" +
                "2. 点击 Play 运行场景\n" +
                "3. 使用快捷键或自动录制开始\n" +
                "4. 停止录制查看输出\n\n" +
                "快捷键：\n" +
                "[R] - 开始/停止录制\n" +
                "[1-4] - 演示控制",
                MessageType.Info
            );
        }

        private void SetupUnityRecorder()
        {
            // 检查是否安装了Recorder包
            bool recorderInstalled = IsRecorderPackageInstalled();

            if (!recorderInstalled)
            {
                if (EditorUtility.DisplayDialog("未安装Recorder",
                    "需要安装 Unity Recorder 包才能使用此功能。\n\n是否打开 Package Manager 安装？",
                    "打开", "取消"))
                {
                    // 打开Package Manager
                    UnityEditor.PackageManager.UI.Window.Open("com.unity.recorder");
                }
                return;
            }

            // 在当前场景中设置录制器
            SetupRecorderComponents();
            EditorUtility.DisplayDialog("设置完成",
                "Unity Recorder 组件已添加到场景。\n\n" +
                "请使用 TripMeta/Video Generator 窗口开始录制。",
                "确定");
        }

        private void SetupScreenCapture()
        {
            SetupScreenCaptureComponents();
            EditorUtility.DisplayDialog("设置完成",
                "Screen Capture 组件已添加到场景。\n\n" +
                "运行时按 [R] 键开始/停止录制。\n" +
                "截图将保存在 Recordings/Frames/ 文件夹。",
                "确定");
        }

        private bool IsRecorderPackageInstalled()
        {
            // 检查是否安装了Recorder包
            var request = UnityEditor.PackageManager.Client.List(true);
            while (!request.IsCompleted) { }

            if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                foreach (var package in request.Result)
                {
                    if (package.name == "com.unity.recorder")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetupRecorderComponents()
        {
            // 查找或创建录制器对象
            GameObject recorderObj = GameObject.Find("VideoRecorder");
            if (recorderObj == null)
            {
                recorderObj = new GameObject("VideoRecorder");
            }

            // 添加组件
            if (recorderObj.GetComponent<VideoRecorder>() == null)
            {
                recorderObj.AddComponent<VideoRecorder>();
            }

            // 添加自动导演系统
            GameObject directorObj = GameObject.Find("AutomatedDemoDirector");
            if (directorObj == null)
            {
                directorObj = new GameObject("AutomatedDemoDirector");
            }

            if (directorObj.GetComponent<AutomatedDemoDirector>() == null)
            {
                directorObj.AddComponent<AutomatedDemoDirector>();
            }

            // 标记场景为已修改
            EditorUtility.SetDirty(recorderObj);
            EditorUtility.SetDirty(directorObj);
            Debug.Log("[VideoRecorderSetup] Recorder components setup complete");
        }

        private void SetupScreenCaptureComponents()
        {
            // 创建简单录制器对象
            GameObject recorderObj = GameObject.Find("SimpleVideoRecorder");
            if (recorderObj == null)
            {
                recorderObj = new GameObject("SimpleVideoRecorder");
            }

            if (recorderObj.GetComponent<SimpleVideoRecorder>() == null)
            {
                recorderObj.AddComponent<SimpleVideoRecorder>();
            }

            EditorUtility.SetDirty(recorderObj);
            Debug.Log("[VideoRecorderSetup] SimpleVideoRecorder component setup complete");
        }

        private void OpenRecordingFolder()
        {
            string path = System.IO.Path.Combine(Application.dataPath, "..", "Recordings");
            System.IO.Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        private void CreateDemoScene()
        {
            if (EditorUtility.DisplayDialog("创建演示场景",
                "这将创建一个新的演示场景用于视频录制。\n\n" +
                "是否继续？",
                "创建", "取消"))
            {
                // 使用现有的快速演示设置
                QuickDemoSetup.CreateDemoScene();
            }
        }
    }
}
