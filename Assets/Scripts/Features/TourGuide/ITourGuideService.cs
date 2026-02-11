using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Features.TourGuide
{
    /// <summary>
    /// AI智能导游服务接口
    /// </summary>
    public interface ITourGuideService
    {
        Task<string> ProcessVoiceInputAsync(AudioClip audioClip);
        Task<string> ProcessTextInputAsync(string text);
        Task<TourGuideResponse> GenerateResponseAsync(string userInput, Vector3 userPosition);
        Task<LocationInfo> GetLocationInfoAsync(Vector3 position);
    }

    [System.Serializable]
    public class TourGuideResponse
    {
        public string text;
        public AudioClip audioResponse;
        public Vector3[] recommendedPositions;
        public string[] visualCues;
    }

    [System.Serializable]
    public class LocationInfo
    {
        public string name;
        public string description;
        public string[] tags;
        public float interestLevel;
        public Vector3 position;
    }
}