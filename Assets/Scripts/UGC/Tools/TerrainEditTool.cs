using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 地形编辑工具
    /// 允许用户编辑场景地形
    /// </summary>
    public class TerrainEditTool : BaseEditorTool
    {
        [Header("地形笔刷")]
        public TerrainBrushType brushType = TerrainBrushType.Raise;
        public float brushSize = 10f;
        public float brushStrength = 0.1f;
        public AnimationCurve brushFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private Terrain terrain;
        private TerrainData terrainData;
        private bool isPainting;

        public TerrainEditTool(SceneEditorManager manager) : base(manager) { }

        public override void Activate()
        {
            base.Activate();
            FindTerrain();
        }

        public override void Update()
        {
            if (!isActive || terrain == null) return;

            HandleInput();

            if (isPainting)
            {
                PaintTerrain();
            }
        }

        /// <summary>
        /// 查找场景中的地形
        /// </summary>
        private void FindTerrain()
        {
            terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                terrainData = terrain.terrainData;
            }
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPainting = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isPainting = false;
            }

            // 调整笔刷大小
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                brushSize = Mathf.Clamp(brushSize + scroll * 5f, 1f, 100f);
            }
        }

        /// <summary>
        /// 绘制地形
        /// </summary>
        private void PaintTerrain()
        {
            Ray ray = GetMouseRay();
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
                return;

            Vector2 terrainCoord = GetTerrainCoord(hit.point);
            int x = (int)(terrainCoord.x * terrainData.heightmapResolution);
            int y = (int)(terrainCoord.y * terrainData.heightmapResolution);

            int brushPixels = (int)(brushSize / terrainData.size.x * terrainData.heightmapResolution);
            int halfBrush = brushPixels / 2;

            float[,] heights = terrainData.GetHeights(
                Mathf.Max(0, x - halfBrush),
                Mathf.Max(0, y - halfBrush),
                Mathf.Min(brushPixels, terrainData.heightmapResolution - x + halfBrush),
                Mathf.Min(brushPixels, terrainData.heightmapResolution - y + halfBrush));

            for (int i = 0; i < heights.GetLength(0); i++)
            {
                for (int j = 0; j < heights.GetLength(1); j++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(i, j),
                        new Vector2(halfBrush, halfBrush)) / halfBrush;

                    float falloff = brushFalloff.Evaluate(Mathf.Clamp01(distance));
                    float delta = brushStrength * falloff * Time.deltaTime;

                    switch (brushType)
                    {
                        case TerrainBrushType.Raise:
                            heights[i, j] += delta;
                            break;
                        case TerrainBrushType.Lower:
                            heights[i, j] -= delta;
                            break;
                        case TerrainBrushType.Smooth:
                            heights[i, j] = SmoothHeight(heights, i, j);
                            break;
                        case TerrainBrushType.Flatten:
                            heights[i, j] = Mathf.Lerp(heights[i, j], 0.5f, delta);
                            break;
                    }

                    heights[i, j] = Mathf.Clamp01(heights[i, j]);
                }
            }

            terrainData.SetHeights(
                Mathf.Max(0, x - halfBrush),
                Mathf.Max(0, y - halfBrush),
                heights);
        }

        /// <summary>
        /// 获取地形坐标
        /// </summary>
        private Vector2 GetTerrainCoord(Vector3 worldPos)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 size = terrainData.size;

            return new Vector2(
                (worldPos.x - terrainPos.x) / size.x,
                (worldPos.z - terrainPos.z) / size.z);
        }

        /// <summary>
        /// 平滑高度
        /// </summary>
        private float SmoothHeight(float[,] heights, int x, int y)
        {
            float sum = 0;
            int count = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int px = x + i;
                    int py = y + j;

                    if (px >= 0 && px < heights.GetLength(0) && py >= 0 && py < heights.GetLength(1))
                    {
                        sum += heights[px, py];
                        count++;
                    }
                }
            }

            return count > 0 ? sum / count : heights[x, y];
        }

        public override void OnDrawGizmos()
        {
            if (!isActive) return;

            Ray ray = GetMouseRay();
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit.point, brushSize * 0.5f);
            }
        }
    }

    /// <summary>
    /// 地形笔刷类型
    /// </summary>
    public enum TerrainBrushType
    {
        Raise,
        Lower,
        Smooth,
        Flatten,
        Paint
    }
}
