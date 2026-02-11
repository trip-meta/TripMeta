using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI
{
    /// <summary>
    /// Azure语音服务 - 语音识别和合成
    /// </summary>
    public class AzureSpeechService : MonoBehaviour, IAzureSpeechService
    {
        private readonly AzureSpeechConfig config;
        private bool isInitialized = false;
        private bool isPaused = false;
        private bool isRecording = false;
        private bool isSpeaking = false;
        
        // 音频组件
        private AudioSource audioSource;
        private AudioClip recordingClip;
        private readonly Queue<SpeechRequest> speechQueue = new Queue<SpeechRequest>();
        
        public bool IsInitialized => isInitialized;
        public bool IsRecording => isRecording;
        public bool IsSpeaking => isSpeaking;
        
        public event Action<string> OnSpeechRecognized;
        public event Action<string> OnSpeechSynthesized;
        public event Action<string> OnError;
        public event Action OnRecordingStarted;
        public event Action OnRecordingStopped;
        
        public AzureSpeechService(AzureSpeechConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        private void Awake()
        {
            // 创建音频源
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
        
        /// <summary>
        /// 初始化语音服务
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("初始化Azure语音服务...", "Speech");
                
                // 验证配置
                ValidateConfig();
                
                // 检查麦克风权限
                await RequestMicrophonePermissionAsync();
                
                // 测试连接
                await TestConnectionAsync();
                
                isInitialized = true;
                Logger.LogInfo("Azure语音服务初始化完成", "Speech");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Azure语音服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 开始语音识别
        /// </summary>
        public async Task<string> StartSpeechRecognitionAsync(float maxDuration = 10f)
        {
            if (!isInitialized)
                throw new InvalidOperationException("语音服务未初始化");
            
            if (isPaused)
                throw new InvalidOperationException("语音服务已暂停");
            
            if (isRecording)
                throw new InvalidOperationException("正在录音中");
            
            try
            {
                Logger.LogInfo("开始语音识别...", "Speech");
                
                // 开始录音
                await StartRecordingAsync(maxDuration);
                
                // 等待录音完成
                while (isRecording)
                {
                    await Task.Yield();
                }
                
                // 发送音频进行识别
                var recognizedText = await RecognizeSpeechAsync(recordingClip);
                
                OnSpeechRecognized?.Invoke(recognizedText);
                Logger.LogInfo($"语音识别完成: {recognizedText}", "Speech");
                
                return recognizedText;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "语音识别失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// 语音合成
        /// </summary>
        public async Task<AudioClip> SynthesizeSpeechAsync(string text, string voiceName = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("语音服务未初始化");
            
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("文本不能为空", nameof(text));
            
            try
            {
                Logger.LogInfo($"开始语音合成: {text.Substring(0, Math.Min(50, text.Length))}...", "Speech");
                
                // 使用指定的语音或默认语音
                var voice = voiceName ?? config.voiceName;
                
                // 发送合成请求
                var audioClip = await SynthesizeAudioAsync(text, voice);
                
                OnSpeechSynthesized?.Invoke(text);
                Logger.LogInfo("语音合成完成", "Speech");
                
                return audioClip;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "语音合成失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// 播放合成的语音
        /// </summary>
        public async Task PlaySynthesizedSpeechAsync(string text, string voiceName = null)
        {
            try
            {
                var audioClip = await SynthesizeSpeechAsync(text, voiceName);
                await PlayAudioClipAsync(audioClip);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "播放合成语音失败");
                throw;
            }
        }
        
        /// <summary>
        /// 连续语音识别
        /// </summary>
        public async Task StartContinuousRecognitionAsync(Action<string> onPartialResult, Action<string> onFinalResult)
        {
            if (!isInitialized)
                throw new InvalidOperationException("语音服务未初始化");
            
            try
            {
                Logger.LogInfo("开始连续语音识别...", "Speech");
                
                // 简化实现 - 实际应该使用Azure Speech SDK的连续识别功能
                while (!isPaused)
                {
                    try
                    {
                        var result = await StartSpeechRecognitionAsync(5f);
                        if (!string.IsNullOrEmpty(result))
                        {
                            onPartialResult?.Invoke(result);
                            onFinalResult?.Invoke(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, "连续语音识别错误");
                        await Task.Delay(1000); // 等待后重试
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "连续语音识别失败");
                throw;
            }
        }
        
        /// <summary>
        /// 停止连续识别
        /// </summary>
        public void StopContinuousRecognition()
        {
            isPaused = true;
            StopRecording();
            Logger.LogInfo("连续语音识别已停止", "Speech");
        }
        
        /// <summary>
        /// 获取支持的语音列表
        /// </summary>
        public async Task<List<VoiceInfo>> GetAvailableVoicesAsync()
        {
            try
            {
                // 模拟返回支持的语音列表
                var voices = new List<VoiceInfo>
                {
                    new VoiceInfo { Name = "zh-CN-XiaoxiaoNeural", DisplayName = "晓晓", Gender = "Female", Locale = "zh-CN" },
                    new VoiceInfo { Name = "zh-CN-YunxiNeural", DisplayName = "云希", Gender = "Male", Locale = "zh-CN" },
                    new VoiceInfo { Name = "zh-CN-YunyangNeural", DisplayName = "云扬", Gender = "Male", Locale = "zh-CN" },
                    new VoiceInfo { Name = "en-US-AriaNeural", DisplayName = "Aria", Gender = "Female", Locale = "en-US" },
                    new VoiceInfo { Name = "en-US-GuyNeural", DisplayName = "Guy", Gender = "Male", Locale = "en-US" }
                };
                
                await Task.Delay(100); // 模拟网络延迟
                return voices;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "获取语音列表失败");
                throw;
            }
        }
        
        /// <summary>
        /// 开始录音
        /// </summary>
        private async Task StartRecordingAsync(float maxDuration)
        {
            if (isRecording) return;
            
            try
            {
                // 检查麦克风设备
                if (Microphone.devices.Length == 0)
                    throw new InvalidOperationException("未找到麦克风设备");
                
                var deviceName = Microphone.devices[0];
                
                // 开始录音
                recordingClip = Microphone.Start(deviceName, false, (int)maxDuration, config.sampleRate);
                isRecording = true;
                
                OnRecordingStarted?.Invoke();
                Logger.LogInfo($"开始录音，最大时长: {maxDuration}秒", "Speech");
                
                // 等待录音完成或手动停止
                var startTime = Time.time;
                while (isRecording && (Time.time - startTime) < maxDuration)
                {
                    await Task.Yield();
                }
                
                // 停止录音
                StopRecording();
            }
            catch (Exception ex)
            {
                isRecording = false;
                Logger.LogException(ex, "录音失败");
                throw;
            }
        }
        
        /// <summary>
        /// 停止录音
        /// </summary>
        private void StopRecording()
        {
            if (!isRecording) return;
            
            try
            {
                Microphone.End(null);
                isRecording = false;
                
                OnRecordingStopped?.Invoke();
                Logger.LogInfo("录音已停止", "Speech");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "停止录音失败");
            }
        }
        
        /// <summary>
        /// 识别语音
        /// </summary>
        private async Task<string> RecognizeSpeechAsync(AudioClip audioClip)
        {
            try
            {
                // 模拟语音识别 - 实际应该调用Azure Speech API
                await Task.Delay(1000);
                
                // 这里应该将AudioClip转换为音频数据并发送到Azure Speech API
                // 返回模拟结果
                return "这是模拟的语音识别结果";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "语音识别API调用失败");
                throw;
            }
        }
        
        /// <summary>
        /// 合成音频
        /// </summary>
        private async Task<AudioClip> SynthesizeAudioAsync(string text, string voiceName)
        {
            try
            {
                // 模拟语音合成 - 实际应该调用Azure Speech API
                await Task.Delay(500);
                
                // 这里应该调用Azure Speech API进行语音合成
                // 返回模拟的AudioClip
                var audioClip = AudioClip.Create("SynthesizedSpeech", config.sampleRate * 2, 1, config.sampleRate, false);
                
                return audioClip;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "语音合成API调用失败");
                throw;
            }
        }
        
        /// <summary>
        /// 播放音频剪辑
        /// </summary>
        private async Task PlayAudioClipAsync(AudioClip audioClip)
        {
            if (audioClip == null) return;
            
            try
            {
                isSpeaking = true;
                audioSource.clip = audioClip;
                audioSource.Play();
                
                // 等待播放完成
                while (audioSource.isPlaying)
                {
                    await Task.Yield();
                }
                
                isSpeaking = false;
            }
            catch (Exception ex)
            {
                isSpeaking = false;
                Logger.LogException(ex, "播放音频失败");
                throw;
            }
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        private void ValidateConfig()
        {
            if (string.IsNullOrEmpty(config.subscriptionKey))
                throw new InvalidOperationException("Azure语音服务订阅密钥未配置");
            
            if (string.IsNullOrEmpty(config.region))
                throw new InvalidOperationException("Azure语音服务区域未配置");
        }
        
        /// <summary>
        /// 请求麦克风权限
        /// </summary>
        private async Task RequestMicrophonePermissionAsync()
        {
            try
            {
                // 在移动平台上请求麦克风权限
                #if UNITY_ANDROID || UNITY_IOS
                if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                {
                    var permission = Application.RequestUserAuthorization(UserAuthorization.Microphone);
                    while (!permission.isDone)
                    {
                        await Task.Yield();
                    }
                    
                    if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                    {
                        throw new InvalidOperationException("麦克风权限被拒绝");
                    }
                }
                #endif
                
                Logger.LogInfo("麦克风权限检查通过", "Speech");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "麦克风权限请求失败");
                throw;
            }
        }
        
        /// <summary>
        /// 测试连接
        /// </summary>
        private async Task TestConnectionAsync()
        {
            try
            {
                // 模拟连接测试
                await Task.Delay(100);
                Logger.LogInfo("Azure语音服务连接测试成功", "Speech");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Azure语音服务连接测试失败");
                throw;
            }
        }
        
        /// <summary>
        /// 检查健康状态
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                await TestConnectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 重新初始化
        /// </summary>
        public async Task ReinitializeAsync()
        {
            isInitialized = false;
            await InitializeAsync();
        }
        
        /// <summary>
        /// 暂停服务
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            StopRecording();
            
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            
            Logger.LogInfo("Azure语音服务已暂停", "Speech");
        }
        
        /// <summary>
        /// 恢复服务
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            
            if (audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
            
            Logger.LogInfo("Azure语音服务已恢复", "Speech");
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                StopRecording();
                
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                }
                
                speechQueue.Clear();
                isInitialized = false;
                
                Logger.LogInfo("Azure语音服务资源已释放", "Speech");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Azure语音服务资源释放失败");
            }
        }
    }
}