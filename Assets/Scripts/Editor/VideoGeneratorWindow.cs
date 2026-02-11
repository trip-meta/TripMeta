using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TripMeta.Video
{
    /// <summary>
    /// 视频生成器编辑器窗口 - 用于批量生成演示视频
    /// </summary>
    public class VideoGeneratorWindow : EditorWindow
    {
        [SerializeField] private VideoRecorderPreset selectedPreset;
        [SerializeField] private int videoWidth = 1920;
        [SerializeField] private int videoHeight = 1080;
        [SerializeField] private int frameRate = 60;
        [SerializeField] private float demoDuration = 45f;
        [SerializeField] private bool openOutputFolder = true;

        private Vector2 scrollPosition;
        private bool isRecording = false;
        private float recordingProgress = 0f;
        private string recordingStatus = "就绪";

        private List<VideoRecorderPreset> presets = new List<VideoRecorderPreset>();

        [MenuItem("TripMeta/Video Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<VideoGeneratorWindow>("视频生成器");
            window.minSize = new Vector2(400, 500);
        }

        private void OnEnable()
        {
            LoadPresets();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawHeader();
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawSettingsSection();
            EditorGUILayout.Space();
            DrawPresetsSection();
            EditorGUILayout.Space();
            DrawRecordingSection();
            EditorGUILayout.Space();
            DrawOutputSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("TripMeta 视频生成器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "使用此工具自动生成演示视频。\n" +
                "支持多种预设和自定义配置。",
                MessageType.Info
            );
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("视频设置", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("分辨率:", GUILayout.Width(80));
                videoWidth = EditorGUILayout.IntPopup(videoWidth,
                    new string[] { "1920x1080", "1280x720", "3840x2160", "2560x1440" },
                    new int[] { 1920, 1280, 3840, 2560 }, GUILayout.Width(150));
                videoHeight = EditorGUILayout.IntPopup(videoHeight,
                    new string[] { "1080p", "720p", "4K", "1440p" },
                    new int[] { 1080, 720, 2160, 1440 });
            }

            frameRate = EditorGUILayout.IntSlider("帧率 (FPS)", frameRate, 30, 120);
            demoDuration = EditorGUILayout.Slider("演示时长 (秒)", demoDuration, 10f, 120f);
            openOutputFolder = EditorGUILayout.Toggle("完成后打开输出文件夹", openOutputFolder);
        }

        private void DrawPresetsSection()
        {
            EditorGUILayout.LabelField("快速预设", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("快速演示", GUILayout.Height(30)))
                {
                    ApplyQuickPreset();
                }
                if (GUILayout.Button("完整演示", GUILayout.Height(30)))
                {
                    ApplyFullPreset();
                }
                if (GUILayout.Button("高质量", GUILayout.Height(30)))
                {
                    ApplyHighQualityPreset();
                }
            }

            EditorGUILayout.HelpBox(
                "• 快速演示: 720p, 30fps, 15秒\n" +
                "• 完整演示: 1080p, 60fps, 45秒\n" +
                "• 高质量: 4K, 60fps, 60秒",
                MessageType.None
            );
        }

        private void DrawRecordingSection()
        {
            EditorGUILayout.LabelField("录制控制", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(isRecording);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = isRecording ? Color.red : Color.green;
                if (GUILayout.Button(isRecording ? "录制中..." : "开始录制", GUILayout.Height(40)))
                {
                    if (isRecording)
                    {
                        StopRecording();
                    }
                    else
                    {
                        StartRecording();
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.EndDisabledGroup();

            // 进度条
            if (isRecording)
            {
                EditorGUILayout.Space();
                recordingProgress = EditorGUILayout.ProgressSlider(recordingProgress, 0f, 1f);
                EditorGUILayout.LabelField(recordingStatus);
            }
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.LabelField("输出", EditorStyles.boldLabel);

            string outputPath = GetOutputPath();
            EditorGUILayout.LabelField("输出文件夹:");
            EditorGUILayout.SelectableLabel(outputPath, EditorStyles.textField);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("打开输出文件夹"))
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
                if (GUILayout.Button("清空输出文件夹"))
                {
                    ClearOutputFolder();
                }
            }
        }

        private void ApplyQuickPreset()
        {
            videoWidth = 1280;
            videoHeight = 720;
            frameRate = 30;
            demoDuration = 15f;
            Debug.Log("[VideoGenerator] 应用快速演示预设");
        }

        private void ApplyFullPreset()
        {
            videoWidth = 1920;
            videoHeight = 1080;
            frameRate = 60;
            demoDuration = 45f;
            Debug.Log("[VideoGenerator] 应用完整演示预设");
        }

        private void ApplyHighQualityPreset()
        {
            videoWidth = 3840;
            videoHeight = 2160;
            frameRate = 60;
            demoDuration = 60f;
            Debug.Log("[VideoGenerator] 应用高质量预设");
        }

        private void StartRecording()
        {
            // 确保场景中有必要的组件
            PrepareSceneForRecording();

            // 启动录制
            isRecording = true;
            recordingStatus = "正在录制...";
            recordingProgress = 0f;

            Debug.Log($"[VideoGenerator] 开始录制: {videoWidth}x{videoHeight} @ {frameRate}fps");

            // 在编辑器中运行场景
            if (EditorApplication.isPlaying)
            {
                InitializeRecordingInPlayMode();
            }
            else
            {
                EditorApplication.isPlaying = true;
                EditorApplication.delayCall += InitializeRecordingInPlayMode;
            }
        }

        private void StopRecording()
        {
            isRecording = false;
            recordingStatus = "录制完成";
            recordingProgress = 1f;

            if (openOutputFolder)
            {
                EditorUtility.RevealInFinder(GetOutputPath());
            }

            Debug.Log("[VideoGenerator] 录制完成");
            Repaint();
        }

        private void InitializeRecordingInPlayMode()
        {
            // 查找VideoRecorder组件并启动
            var recorder = FindObjectOfType<VideoRecorder>();
            if (recorder != null)
            {
                // 通过反射设置私有字段
                var recorderType = typeof(VideoRecorder);
                var widthField = recorderType.GetField("resolutionWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var heightField = recorderType.GetField("resolutionHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var rateField = recorderType.GetField("frameRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (widthField != null) widthField.SetValue(recorder, videoWidth);
                if (heightField != null) heightField.SetValue(recorder, videoHeight);
                if (rateField != null) rateField.SetValue(recorder, frameRate);

                recorder.StartRecording();

                // 设置自动停止计时器
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorApplication.isPlaying)
                        {
                            recorder.StopRecording();
                            StopRecording();
                            EditorApplication.isPlaying = false;
                        }
                    };
                };
            }
        }

        private void PrepareSceneForRecording()
        {
            // 查找或创建必要的组件
            var recorder = FindObjectOfType<VideoRecorder>();
            if (recorder == null)
            {
                GameObject recorderObj = new GameObject("VideoRecorder");
                recorder = recorderObj.AddComponent<VideoRecorder>();
                Debug.Log("[VideoGenerator] 创建VideoRecorder组件");
            }

            var director = FindObjectOfType<AutomatedDemoDirector>();
            if (director == null)
            {
                GameObject directorObj = new GameObject("AutomatedDemoDirector");
                director = directorObj.AddComponent<AutomatedDemoDirector>();
                Debug.Log("[VideoGenerator] 创建AutomatedDemoDirector组件");
            }
        }

        private string GetOutputPath()
        {
            return Path.Combine(Application.dataPath, "..", "Recordings");
        }

        private void ClearOutputFolder()
        {
            if (EditorUtility.DisplayDialog("清空输出文件夹", "确定要清空所有录制的视频吗？", "确定", "取消"))
            {
                string path = GetOutputPath();
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Debug.Log("[VideoGenerator] 输出文件夹已清空");
                }
            }
        }

        private void LoadPresets()
        {
            // 从Resources加载预设
            presets = new List<VideoRecorderPreset>();
            // 可以从文件加载预设配置
        }

        private void Update()
        {
            if (isRecording)
            {
                Repaint();
            }
        }
    }

    /// <summary>
    /// 视频录制预设数据
    /// </summary>
    [System.Serializable]
    public class VideoRecorderPreset
    {
        public string presetName;
        public int width;
        public int height;
        public int frameRate;
        public float duration;
    }
}
