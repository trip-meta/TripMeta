using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Features.SceneGeneration
{
    /// <summary>
    /// AI动态场景生成服务接口
    /// </summary>
    public interface ISceneGenerationService
    {
        Task<GameObject> GenerateSceneAsync(SceneGenerationRequest request);
        Task<Texture2D> GenerateTextureAsync(string description);
        Task<Mesh> GenerateMeshAsync(string description);
        Task OptimizeSceneAsync(GameObject scene);
    }

    [System.Serializable]
    public class SceneGenerationRequest
    {
        public string description;
        public string style;
        public Vector3 bounds;
        public int detailLevel;
        public string[] tags;
    }
}