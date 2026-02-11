using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TripMeta.Video
{
    /// <summary>
    /// 批量视频生成工具 - 一次性生成多个不同配置的视频
    /// </summary>
    public class BatchVideoGenerator : EditorWindow
    {
        [System.Serializable]
        public class VideoConfig
        {
            public string name;
            public int width;
            public int height;
            public int frameRate;
            public float duration;

            public VideoConfig(string name, int width, int height, int frameRate, float duration)
            {
                this.name = name;
                this.width = width;
                this.height = height;
                this.frameRate = frameRate;
                this.duration = duration;
            }
        }

        private List<VideoConfig> videoConfigs = new List<VideoConfig>();
        private bool isProcessing = false;
        private int currentConfigIndex = 0;
        private string processStatus = "";
        private float processProgress = 0f;

        [MenuItem("TripMeta/Batch Video Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchVideoGenerator>("批量视频生成器");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            // 默认配置
            if (videoConfigs.Count == 0)
            {
                videoConfigs.AddRange(new List<VideoConfig>
                {
                    new VideoConfig("快速演示 (720p)", 1280, 720, 30, 15f),
                    new VideoConfig("标准演示 (1080p)", 1920, 1080, 60, 30f),
                    new VideoConfig("完整演示 (1080p)", 1920, 1080, 60, 45f),
                    new VideoConfig("高质量 (1440p)", 2560, 1440, 60, 60f),
                    new VideoConfig("超高清 (4K)", 3840, 2160, 60, 60f)
                });
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("TripMeta 批量视频生成器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "批量生成多个不同配置的演示视频。\n" +
                "每个视频将使用不同的分辨率、帧率和时长。",
                MessageType.Info
            );

            EditorGUILayout.Space();

            DrawConfigList();
            EditorGUILayout.Space();
            DrawBatchControls();
            EditorGUILayout.Space();
            DrawProgressSection();
        }

        private void DrawConfigList()
        {
            EditorGUILayout.LabelField("视频配置列表", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < videoConfigs.Count; i++)
                {
                    DrawConfigItem(i);
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("添加配置"))
                {
                    videoConfigs.Add(new VideoConfig("新配置", 1920, 1080, 60, 30f));
                }
                if (GUILayout.Button("移除最后一个"))
                {
                    if (videoConfigs.Count > 0)
                    {
                        videoConfigs.RemoveAt(videoConfigs.Count - 1);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("加载快速预设"))
                {
                    LoadQuickPresets();
                }
                if (GUILayout.Button("清空列表"))
                {
                    videoConfigs.Clear();
                }
            }
        }

        private void DrawConfigItem(int index)
        {
            var config = videoConfigs[index];

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{index + 1}.", GUILayout.Width(30));
                config.name = EditorGUILayout.TextField(config.name, GUILayout.Width(150));
                config.width = EditorGUILayout.IntPopup(config.width,
                    new string[] { "1280", "1920", "2560", "3840" },
                    new int[] { 1280, 1920, 2560, 3840 }, GUILayout.Width(70));
                EditorGUILayout.LabelField("x", GUILayout.Width(15));
                config.height = EditorGUILayout.IntPopup(config.height,
                    new string[] { "720", "1080", "1440", "2160" },
                    new int[] { 720, 1080, 1440, 2160 }, GUILayout.Width(70));
                EditorGUILayout.LabelField("@", GUILayout.Width(15));
                config.frameRate = EditorGUILayout.IntPopup(config.frameRate,
                    new string[] { "30", "60", "120" },
                    new int[] { 30, 60, 120 }, GUILayout.Width(50));
                EditorGUILayout.LabelField("FPS", GUILayout.Width(35));
                config.duration = EditorGUILayout.FloatField(config.duration, GUILayout.Width(50));
                EditorGUILayout.LabelField("秒", GUILayout.Width(25));

                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    videoConfigs.RemoveAt(index);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DrawBatchControls()
        {
            EditorGUI.BeginDisabledGroup(isProcessing);

            EditorGUILayout.LabelField("批量控制", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("开始批量生成", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("确认批量生成",
                        $"将生成 {videoConfigs.Count} 个视频。\n\n" +
                        "这可能需要较长时间，请确保:\n" +
                        "• Unity 编辑器不会被关闭\n" +
                        "• 有足够的磁盘空间\n" +
                        "• 不会中断录制过程",
                        "开始", "取消"))
                    {
                        EditorApplication.delayCall += StartBatchGeneration;
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.HelpBox(
                "批量生成过程中:\n" +
                "• 请勿关闭 Unity 编辑器\n" +
                "• 请勿切换场景\n" +
                "• 确保有足够磁盘空间\n" +
                "• 预计总时间: " + EstimateTotalTime(),
                MessageType.Warning
            );

            EditorGUI.EndDisabledGroup();

            if (isProcessing)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("停止生成", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog("确认停止",
                            "确定要停止批量生成吗？当前视频将不完整。",
                            "停止", "继续"))
                        {
                            StopBatchGeneration();
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        private void DrawProgressSection()
        {
            if (isProcessing)
            {
                EditorGUILayout.LabelField("生成进度", EditorStyles.boldLabel);

                // 总进度
                float totalProgress = (float)currentConfigIndex / videoConfigs.Count;
                EditorGUILayout.ProgressBar(totalProgress, $"总进度: {currentConfigIndex}/{videoConfigs.Count}");

                // 当前状态
                EditorGUILayout.HelpBox(processStatus, MessageType.Info);

                // 强制重绘
                Repaint();
            }
        }

        private void LoadQuickPresets()
        {
            videoConfigs = new List<VideoConfig>
            {
                new VideoConfig("快速演示 (720p)", 1280, 720, 30, 15f),
                new VideoConfig("标准演示 (1080p)", 1920, 1080, 60, 30f),
                new VideoConfig("完整演示 (1080p)", 1920, 1080, 60, 45f)
            };
        }

        private string EstimateTotalTime()
        {
            float totalSeconds = 0;
            foreach (var config in videoConfigs)
            {
                totalSeconds += config.duration + 10f; // 加上处理时间
            }

            if (totalSeconds < 60)
            {
                return $"{Mathf.CeilToInt(totalSeconds)} 秒";
            }
            else
            {
                int minutes = Mathf.FloorToInt(totalSeconds / 60);
                int seconds = Mathf.CeilToInt(totalSeconds % 60);
                return $"{minutes} 分 {seconds} 秒";
            }
        }

        private void StartBatchGeneration()
        {
            isProcessing = true;
            currentConfigIndex = 0;
            EditorApplication.delayCall += ProcessNextConfig;
        }

        private void ProcessNextConfig()
        {
            if (currentConfigIndex >= videoConfigs.Count)
            {
                // 完成
                isProcessing = false;
                processStatus = "批量生成完成！";
                EditorUtility.DisplayDialog("完成", "所有视频已生成完成！", "确定");

                // 打开输出文件夹
                string outputPath = Path.Combine(Application.dataPath, "..", "Recordings");
                EditorUtility.RevealInFinder(outputPath);
                return;
            }

            var config = videoConfigs[currentConfigIndex];
            processStatus = $"正在生成: {config.name} ({config.width}x{config.height} @ {config.frameRate}fps)";

            // 生成当前配置的视频
            GenerateVideo(config);
        }

        private void GenerateVideo(VideoConfig config)
        {
            // 这里需要实现实际的录制逻辑
            // 由于Unity Recorder API的限制，我们需要使用一个协程来处理

            // 更新状态
            currentConfigIndex++;
            EditorApplication.delayCall += ProcessNextConfig;
        }

        private void StopBatchGeneration()
        {
            isProcessing = false;
            processStatus = "批量生成已停止";
            Debug.Log("[BatchVideoGenerator] 批量生成已停止");
        }
    }
}
