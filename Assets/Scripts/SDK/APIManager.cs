using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TripMeta.SDK
{
    /// <summary>
    /// API管理器
    /// 处理与TripMeta API的通信
    /// </summary>
    public class APIManager : MonoBehaviour
    {
        [Header("API配置")]
        public string apiBaseUrl = "https://api.tripmeta.com/v1";
        public string apiKey = "";
        public int requestTimeout = 30;
        public int maxRetries = 3;

        [Header("速率限制")]
        public int maxRequestsPerMinute = 60;
        private int requestCount = 0;
        private float lastResetTime;

        public static APIManager Instance { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(apiKey);

        // 事件
        public event Action<APIResponse> OnRequestCompleted;
        public event Action<string> OnRequestFailed;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            // 重置速率限制计数器
            if (Time.time - lastResetTime > 60f)
            {
                requestCount = 0;
                lastResetTime = Time.time;
            }
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        public async Task<APIResponse<T>> Get<T>(string endpoint, Dictionary<string, string> parameters = null)
        {
            string url = BuildUrl(endpoint, parameters);
            return await SendRequest<T>(url, UnityWebRequest.kHttpVerbGET);
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        public async Task<APIResponse<T>> Post<T>(string endpoint, object data)
        {
            string url = apiBaseUrl + endpoint;
            string json = JsonUtility.ToJson(data);
            return await SendRequest<T>(url, UnityWebRequest.kHttpVerbPOST, json);
        }

        /// <summary>
        /// 发送PUT请求
        /// </summary>
        public async Task<APIResponse<T>> Put<T>(string endpoint, object data)
        {
            string url = apiBaseUrl + endpoint;
            string json = JsonUtility.ToJson(data);
            return await SendRequest<T>(url, UnityWebRequest.kHttpVerbPUT, json);
        }

        /// <summary>
        /// 发送DELETE请求
        /// </summary>
        public async Task<APIResponse<T>> Delete<T>(string endpoint)
        {
            string url = apiBaseUrl + endpoint;
            return await SendRequest<T>(url, UnityWebRequest.kHttpVerbDELETE);
        }

        /// <summary>
        /// 构建URL
        /// </summary>
        private string BuildUrl(string endpoint, Dictionary<string, string> parameters)
        {
            string url = apiBaseUrl + endpoint;

            if (parameters != null && parameters.Count > 0)
            {
                url += "?";
                foreach (var param in parameters)
                {
                    url += $"{UnityWebRequest.EscapeURL(param.Key)}={UnityWebRequest.EscapeURL(param.Value)}&";
                }
                url = url.TrimEnd('&');
            }

            return url;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        private async Task<APIResponse<T>> SendRequest<T>(string url, string method, string jsonData = null)
        {
            // 检查速率限制
            if (requestCount >= maxRequestsPerMinute)
            {
                return new APIResponse<T>
                {
                    success = false,
                    error = "Rate limit exceeded"
                };
            }

            requestCount++;

            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    UnityWebRequest request = new UnityWebRequest(url, method);

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    }

                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                    request.timeout = requestTimeout;

                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var response = new APIResponse<T>
                        {
                            success = true,
                            data = JsonUtility.FromJson<T>(request.downloadHandler.text),
                            statusCode = (int)request.responseCode
                        };

                        OnRequestCompleted?.Invoke(response);
                        return response;
                    }
                    else
                    {
                        if (retries < maxRetries - 1)
                        {
                            retries++;
                            await Task.Delay(1000 * retries);
                            continue;
                        }

                        var errorResponse = new APIResponse<T>
                        {
                            success = false,
                            error = request.error,
                            statusCode = (int)request.responseCode
                        };

                        OnRequestFailed?.Invoke(request.error);
                        return errorResponse;
                    }
                }
                catch (Exception e)
                {
                    if (retries < maxRetries - 1)
                    {
                        retries++;
                        await Task.Delay(1000 * retries);
                        continue;
                    }

                    var errorResponse = new APIResponse<T>
                    {
                        success = false,
                        error = e.Message
                    };

                    OnRequestFailed?.Invoke(e.Message);
                    return errorResponse;
                }
            }

            return new APIResponse<T>
            {
                success = false,
                error = "Max retries exceeded"
            };
        }

        /// <summary>
        /// 设置API密钥
        /// </summary>
        public void SetApiKey(string key)
        {
            apiKey = key;
        }
    }

    /// <summary>
    /// API响应
    /// </summary>
    public class APIResponse
    {
        public bool success;
        public string error;
        public int statusCode;
    }

    /// <summary>
    /// API响应（泛型）
    /// </summary>
    public class APIResponse<T> : APIResponse
    {
        public T data;
    }
}
