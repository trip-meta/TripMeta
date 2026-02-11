using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Infrastructure.Network;
using TripMeta.Infrastructure.Cache;
using TripMeta.Core.Configuration;

namespace TripMeta.Features.TourGuide
{
    /// <summary>
    /// AI智能导游服务实现
    /// </summary>
    public class TourGuideService : MonoBehaviour, ITourGuideService
    {
        private INetworkService networkService;
        private ICacheService cacheService;
        private AppSettings appSettings;
        
        public void Initialize(INetworkService network, ICacheService cache, AppSettings settings)
        {
            networkService = network;
            cacheService = cache;
            appSettings = settings;
        }
        
        public async Task<string> ProcessVoiceInputAsync(AudioClip audioClip)
        {
            try
            {
                // 将音频转换为文本 (Azure Speech Service)
                var audioData = ConvertAudioClipToBytes(audioClip);
                var speechRequest = new
                {
                    audio = System.Convert.ToBase64String(audioData),
                    language = "zh-CN",
                    format = "wav"
                };
                
                var speechResult = await networkService.PostAsync<SpeechToTextResponse>(
                    "api/speech/recognize", speechRequest);
                
                return speechResult.text;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Voice processing failed: {ex.Message}");
                return "抱歉，我没有听清楚您说的话，请再试一次。";
            }
        }
        
        public async Task<string> ProcessTextInputAsync(string text)
        {
            try
            {
                // 检查缓存
                string cacheKey = $"text_response_{text.GetHashCode()}";
                var cachedResponse = await cacheService.GetAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(cachedResponse))
                {
                    return cachedResponse;
                }
                
                // 调用GPT-4 API
                var gptRequest = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "你是一个专业的VR旅游导游，请用友好、专业的语调回答用户的问题。" },
                        new { role = "user", content = text }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };
                
                var gptResponse = await networkService.PostAsync<GPTResponse>(
                    "api/ai/chat", gptRequest);
                
                string response = gptResponse.choices[0].message.content;
                
                // 缓存响应
                await cacheService.SetAsync(cacheKey, response, System.TimeSpan.FromMinutes(30));
                
                return response;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Text processing failed: {ex.Message}");
                return "抱歉，我现在无法回答您的问题，请稍后再试。";
            }
        }
        
        public async Task<TourGuideResponse> GenerateResponseAsync(string userInput, Vector3 userPosition)
        {
            try
            {
                var locationInfo = await GetLocationInfoAsync(userPosition);
                
                var contextualRequest = new
                {
                    userInput = userInput,
                    location = locationInfo,
                    userPosition = new { x = userPosition.x, y = userPosition.y, z = userPosition.z }
                };
                
                var response = await networkService.PostAsync<TourGuideApiResponse>(
                    "api/tourguide/generate", contextualRequest);
                
                return new TourGuideResponse
                {
                    text = response.text,
                    audioResponse = null, // 将在后续实现音频生成
                    recommendedPositions = response.recommendedPositions,
                    visualCues = response.visualCues
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Response generation failed: {ex.Message}");
                return new TourGuideResponse
                {
                    text = "抱歉，我现在无法为您提供详细信息。",
                    audioResponse = null,
                    recommendedPositions = new Vector3[0],
                    visualCues = new string[0]
                };
            }
        }
        
        public async Task<LocationInfo> GetLocationInfoAsync(Vector3 position)
        {
            try
            {
                string cacheKey = $"location_{position.x:F1}_{position.y:F1}_{position.z:F1}";
                var cachedLocation = await cacheService.GetAsync<LocationInfo>(cacheKey);
                if (cachedLocation != null)
                {
                    return cachedLocation;
                }
                
                var locationRequest = new
                {
                    position = new { x = position.x, y = position.y, z = position.z },
                    radius = 10f
                };
                
                var locationResponse = await networkService.PostAsync<LocationInfo>(
                    "api/location/info", locationRequest);
                
                await cacheService.SetAsync(cacheKey, locationResponse, System.TimeSpan.FromMinutes(60));
                
                return locationResponse;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Location info retrieval failed: {ex.Message}");
                return new LocationInfo
                {
                    name = "未知位置",
                    description = "暂无位置信息",
                    tags = new string[0],
                    interestLevel = 0f,
                    position = position
                };
            }
        }
        
        private byte[] ConvertAudioClipToBytes(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            byte[] bytes = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)(samples[i] * short.MaxValue);
                bytes[i * 2] = (byte)(sample & 0xFF);
                bytes[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
            }
            
            return bytes;
        }
    }
    
    [System.Serializable]
    public class SpeechToTextResponse
    {
        public string text;
        public float confidence;
    }
    
    [System.Serializable]
    public class GPTResponse
    {
        public GPTChoice[] choices;
    }
    
    [System.Serializable]
    public class GPTChoice
    {
        public GPTMessage message;
    }
    
    [System.Serializable]
    public class GPTMessage
    {
        public string content;
    }
    
    [System.Serializable]
    public class TourGuideApiResponse
    {
        public string text;
        public Vector3[] recommendedPositions;
        public string[] visualCues;
    }
}