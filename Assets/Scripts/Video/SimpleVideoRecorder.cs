using UnityEngine;
using System.IO;

namespace TripMeta.Video
{
    /// <summary>
    /// 简化的视频录制组件 - 使用Screen Capture API
    /// </summary>
    public class SimpleVideoRecorder : MonoBehaviour
    {
        [Header("录制设置")]
        [SerializeField] private int captureFrameRate = 60;
        [SerializeField] private int superSampleSize = 1;
        [SerializeField] private bool captureUI = false;

        [Header("输出设置")]
        [SerializeField] private string outputFolder = "Recordings";
        [SerializeField] private string fileNamePrefix = "TripMeta_";

        private bool isRecording = false;
        private string outputPath;
        private int frameCount = 0;

        /// <summary>
        /// 开始录制截图序列（可用于后期合成视频）
        /// </summary>
        public void StartCapture()
        {
            if (isRecording)
            {
                Debug.LogWarning("[SimpleVideoRecorder] 已在录制中");
                return;
            }

            // 创建输出目录
            outputPath = Path.Combine(Application.dataPath, "..", outputFolder, "Frames_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(outputPath);

            isRecording = true;
            frameCount = 0;
            Time.captureFramerate = captureFrameRate;

            Debug.Log($"[SimpleVideoRecorder] 开始录制: {outputPath}");
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopCapture()
        {
            if (!isRecording)
            {
                Debug.LogWarning("[SimpleVideoRecorder] 未在录制中");
                return;
            }

            isRecording = false;
            Time.captureFramerate = 0;

            Debug.Log($"[SimpleVideoRecorder] 录制完成，共 {frameCount} 帧");
        }

        private void LateUpdate()
        {
            if (isRecording)
            {
                // 保存当前帧
                string filename = string.Format("{0}/{1}{2:00000}.png", outputPath, fileNamePrefix, frameCount);
                ScreenCapture.CaptureScreenshot(filename, superSampleSize);
                frameCount++;
            }
        }

        private void OnApplicationQuit()
        {
            if (isRecording)
            {
                StopCapture();
            }
        }

        /// <summary>
        /// 获取录制信息
        /// </summary>
        public string GetRecordingInfo()
        {
            return isRecording
                ? $"录制中: {frameCount} 帧"
                : "未录制";
        }

        /// <summary>
        /// 获取输出路径
        /// </summary>
        public string GetOutputPath()
        {
            return outputPath;
        }
    }
}
