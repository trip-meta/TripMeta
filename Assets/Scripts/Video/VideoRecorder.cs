using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.IO;
using System.Collections;

namespace TripMeta.Video
{
    /// <summary>
    /// 自动视频录制系统 - 使用Unity Recorder API
    /// </summary>
    public class VideoRecorder : MonoBehaviour
    {
        [Header("录制设置")]
        [SerializeField] private string outputFolder = "Recordings";
        [SerializeField] private string fileNamePrefix = "TripMeta_Demo_";
        [SerializeField] private int frameRate = 60;
        [SerializeField] private int resolutionWidth = 1920;
        [SerializeField] private int resolutionHeight = 1080;

        [Header("自动录制")]
        [SerializeField] private bool autoRecordOnStart = false;
        [SerializeField] private float autoRecordDuration = 30f;
        [SerializeField] private float delayBeforeRecord = 2f;

        private RecorderController recorderController;
        private MovieRecorderSettings movieSettings;
        private bool isRecording = false;

        private void Start()
        {
            if (autoRecordOnStart)
            {
                StartCoroutine(AutoRecordCoroutine());
            }
        }

        private IEnumerator AutoRecordCoroutine()
        {
            yield return new WaitForSeconds(delayBeforeRecord);
            StartRecording();
            yield return new WaitForSeconds(autoRecordDuration);
            StopRecording();
        }

        public void StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("[VideoRecorder] 录制已在进行中");
                return;
            }

            SetupRecorder();
            recorderController.StartRecording();
            isRecording = true;
            Debug.Log("[VideoRecorder] 开始录制");
        }

        public void StopRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("[VideoRecorder] 没有正在进行的录制");
                return;
            }

            recorderController.StopRecording();
            isRecording = false;
            Debug.Log("[VideoRecorder] 停止录制");
        }

        private void SetupRecorder()
        {
            // 创建输出目录
            string outputPath = Path.Combine(Application.dataPath, "..", outputFolder);
            Directory.CreateDirectory(outputPath);

            // 配置录制器控制器
            recorderController = new RecorderController(new RecorderControllerSettings
            {
                outputFileRoot = outputPath,
                captureGameCamera = true,
                // 设置帧率
                frameRateOverride = frameRate
            });

            // 创建视频录制设置
            movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieSettings.name = "My Video Recorder";
            movieSettings.Enabled = true;

            // 设置输出文件名
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            movieSettings.OutputFile = fileNamePrefix + timestamp;

            // 设置视频编码
            movieSettings.VideoBitRateMode = VideoBitrateMode.High;
            movieSettings.ImageSize = new Vector2Int(resolutionWidth, resolutionHeight);
            movieSettings.FrameRate = frameRate;

            // 添加主相机作为录制输入
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraInput = ScriptableObject.CreateInstance<CameraInputSettings>();
                cameraInput.Source = mainCamera;
                cameraInput.OutputHeight = resolutionHeight;
                cameraInput.OutputWidth = resolutionWidth;
                cameraInput.FlipFinalOutput = false;

                movieSettings.InputSettings.Add(cameraInput);
            }

            // 将录制设置添加到控制器
            recorderController.AddRecorderSettings(movieSettings);

            Debug.Log($"[VideoRecorder] 录制器已配置，输出路径: {outputPath}");
        }

        private void OnDestroy()
        {
            if (isRecording)
            {
                StopRecording();
            }
        }

        private void OnApplicationQuit()
        {
            if (isRecording)
            {
                StopRecording();
            }
        }
    }
}
