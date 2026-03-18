using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TripMeta.Performance
{
    /// <summary>
    /// 性能图表组件
    /// 实时显示性能数据趋势图
    /// </summary>
    public class PerformanceGraph : MonoBehaviour
    {
        [Header("图表配置")]
        public int maxDataPoints = 60;
        public float updateInterval = 1f;
        public Color lineColor = Color.green;
        public Color fillColor = new Color(0, 1, 0, 0.3f);
        public Color gridColor = new Color(1, 1, 1, 0.2f);

        [Header("数值范围")]
        public bool autoScale = true;
        public float minValue = 0f;
        public float maxValue = 100f;

        [Header("显示选项")]
        public bool showGrid = true;
        public bool showFill = true;
        public bool showValue = true;
        public int gridLines = 4;

        [Header("UI 引用")]
        public RectTransform graphArea;
        public Text currentValueText;
        public Text minMaxText;
        public Material lineMaterial;

        private Queue<float> dataPoints = new Queue<float>();
        private float lastUpdateTime;
        private Texture2D graphTexture;
        private RawImage graphImage;

        // 缓存数据
        private float currentMinValue;
        private float currentMaxValue;
        private float currentAverage;

        void Awake()
        {
            InitializeGraph();
        }

        void OnDestroy()
        {
            if (graphTexture != null)
            {
                Destroy(graphTexture);
            }
        }

        /// <summary>
        /// 初始化图表
        /// </summary>
        private void InitializeGraph()
        {
            if (graphArea == null) return;

            // 创建或获取 RawImage
            graphImage = graphArea.GetComponent<RawImage>();
            if (graphImage == null)
            {
                graphImage = graphArea.gameObject.AddComponent<RawImage>();
            }

            // 创建纹理
            int width = Mathf.Max(100, (int)graphArea.rect.width);
            int height = Mathf.Max(50, (int)graphArea.rect.height);
            graphTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            graphTexture.filterMode = FilterMode.Bilinear;

            graphImage.texture = graphTexture;
            graphImage.color = Color.white;

            // 初始化数值范围
            currentMinValue = minValue;
            currentMaxValue = maxValue;
        }

        /// <summary>
        /// 添加数据点
        /// </summary>
        public void AddDataPoint(float value)
        {
            dataPoints.Enqueue(value);

            while (dataPoints.Count > maxDataPoints)
            {
                dataPoints.Dequeue();
            }

            // 更新统计
            UpdateStatistics();

            // 更新显示
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                RedrawGraph();
                UpdateUI();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics()
        {
            if (dataPoints.Count == 0) return;

            var dataArray = dataPoints.ToArray();
            currentMinValue = dataArray.Min();
            currentMaxValue = dataArray.Max();
            currentAverage = dataArray.Average();

            // 自动缩放
            if (autoScale)
            {
                float padding = (currentMaxValue - currentMinValue) * 0.1f;
                minValue = Mathf.Max(0, currentMinValue - padding);
                maxValue = currentMaxValue + padding;

                if (Mathf.Approximately(minValue, maxValue))
                {
                    maxValue = minValue + 1f;
                }
            }
        }
        /// <summary>
        /// 重绘图表
        /// </summary>
        private void RedrawGraph()
        {
            if (graphTexture == null || dataPoints.Count < 2) return;

            int width = graphTexture.width;
            int height = graphTexture.height;
            Color[] pixels = new Color[width * height];

            // 清空背景
            Color backgroundColor = new Color(0, 0, 0, 0.3f);
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }

            // 绘制网格
            if (showGrid)
            {
                DrawGrid(pixels, width, height);
            }

            // 绘制数据线
            DrawDataLine(pixels, width, height);

            graphTexture.SetPixels(pixels);
            graphTexture.Apply();
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawGrid(Color[] pixels, int width, int height)
        {
            // 水平网格线
            for (int i = 1; i < gridLines; i++)
            {
                int y = (height * i) / gridLines;
                for (int x = 0; x < width; x++)
                {
                    SetPixel(pixels, width, x, y, gridColor);
                }
            }

            // 垂直网格线
            int verticalLines = 4;
            for (int i = 1; i < verticalLines; i++)
            {
                int x = (width * i) / verticalLines;
                for (int y = 0; y < height; y++)
                {
                    SetPixel(pixels, width, x, y, gridColor);
                }
            }
        }

        /// <summary>
        /// 绘制数据线
        /// </summary>
        private void DrawDataLine(Color[] pixels, int width, int height)
        {
            var dataArray = dataPoints.ToArray();
            float valueRange = maxValue - minValue;

            // 计算点的位置
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < dataArray.Length; i++)
            {
                float x = (float)i / (maxDataPoints - 1) * (width - 1);
                float normalizedValue = (dataArray[i] - minValue) / valueRange;
                float y = normalizedValue * (height - 1);
                points.Add(new Vector2(x, y));
            }

            // 绘制填充区域
            if (showFill && points.Count > 1)
            {
                DrawFilledArea(pixels, width, height, points);
            }

            // 绘制线条
            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(pixels, width, height,
                    (int)points[i - 1].x, (int)points[i - 1].y,
                    (int)points[i].x, (int)points[i].y,
                    lineColor);
            }
        }

        /// <summary>
        /// 绘制填充区域
        /// </summary>
        private void DrawFilledArea(Color[] pixels, int width, int height, List<Vector2> points)
        {
            for (int x = 0; x < width; x++)
            {
                // 找到该x位置对应的y值
                float t = (float)x / (width - 1);
                int dataIndex = Mathf.RoundToInt(t * (points.Count - 1));
                dataIndex = Mathf.Clamp(dataIndex, 0, points.Count - 1);

                int lineY = (int)points[dataIndex].y;

                // 填充从底部到线条
                for (int y = 0; y <= lineY && y < height; y++)
                {
                    SetPixel(pixels, width, x, y, fillColor);
                }
            }
        }

        /// <summary>
        /// 绘制线条（Bresenham算法）
        /// </summary>
        private void DrawLine(Color[] pixels, int width, int height,
            int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(pixels, width, x0, y0, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// 设置像素颜色
        /// </summary>
        private void SetPixel(Color[] pixels, int width, int x, int y, Color color)
        {
            if (x >= 0 && x < width && y >= 0 && y < pixels.Length / width)
            {
                int index = y * width + x;
                pixels[index] = BlendColors(pixels[index], color);
            }
        }

        /// <summary>
        /// 混合颜色
        /// </summary>
        private Color BlendColors(Color baseColor, Color overlayColor)
        {
            float alpha = overlayColor.a;
            return new Color(
                baseColor.r * (1 - alpha) + overlayColor.r * alpha,
                baseColor.g * (1 - alpha) + overlayColor.g * alpha,
                baseColor.b * (1 - alpha) + overlayColor.b * alpha,
                Mathf.Max(baseColor.a, overlayColor.a)
            );
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (showValue && currentValueText != null && dataPoints.Count > 0)
            {
                float current = dataPoints.Last();
                currentValueText.text = $"{current:F1}";
            }

            if (minMaxText != null && dataPoints.Count > 0)
            {
                minMaxText.text = $"Min: {currentMinValue:F0} | Avg: {currentAverage:F0} | Max: {currentMaxValue:F0}";
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            dataPoints.Clear();
            if (graphTexture != null)
            {
                Color[] pixels = new Color[graphTexture.width * graphTexture.height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color(0, 0, 0, 0.3f);
                }
                graphTexture.SetPixels(pixels);
                graphTexture.Apply();
            }
        }

        /// <summary>
        /// 设置颜色主题
        /// </summary>
        public void SetColorTheme(GraphColorTheme theme)
        {
            switch (theme)
            {
                case GraphColorTheme.Green:
                    lineColor = Color.green;
                    fillColor = new Color(0, 1, 0, 0.3f);
                    break;
                case GraphColorTheme.Blue:
                    lineColor = Color.cyan;
                    fillColor = new Color(0, 1, 1, 0.3f);
                    break;
                case GraphColorTheme.Red:
                    lineColor = new Color(1, 0.3f, 0.3f);
                    fillColor = new Color(1, 0, 0, 0.3f);
                    break;
                case GraphColorTheme.Yellow:
                    lineColor = Color.yellow;
                    fillColor = new Color(1, 1, 0, 0.3f);
                    break;
                case GraphColorTheme.Purple:
                    lineColor = new Color(0.8f, 0.2f, 1f);
                    fillColor = new Color(0.5f, 0, 1f, 0.3f);
                    break;
            }
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public GraphStatistics GetStatistics()
        {
            return new GraphStatistics
            {
                min = currentMinValue,
                max = currentMaxValue,
                average = currentAverage,
                current = dataPoints.Count > 0 ? dataPoints.Last() : 0,
                sampleCount = dataPoints.Count
            };
        }
    }

    /// <summary>
    /// 图表颜色主题
    /// </summary>
    public enum GraphColorTheme
    {
        Green,
        Blue,
        Red,
        Yellow,
        Purple
    }

    /// <summary>
    /// 图表统计数据
    /// </summary>
    public struct GraphStatistics
    {
        public float min;
        public float max;
        public float average;
        public float current;
        public int sampleCount;
    }
}
