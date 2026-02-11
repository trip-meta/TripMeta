using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TripMeta.Video
{
    /// <summary>
    /// 自动演示导演系统 - 控制演示序列用于视频录制
    /// </summary>
    public class AutomatedDemoDirector : MonoBehaviour
    {
        [Header("演示序列")]
        [SerializeField] private float demoDuration = 45f;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float startDelay = 3f;

        [Header("相机控制")]
        [SerializeField] private Camera demoCamera;
        [SerializeField] private Transform[] cameraWaypoints;
        [SerializeField] private float cameraMoveSpeed = 2f;

        [Header("引用")]
        [SerializeField] private DemoController demoController;
        [SerializeField] private InteractiveDemo interactiveDemo;

        private Queue<DemoAction> actionQueue = new Queue<DemoAction>();
        private bool isRunning = false;

        private void Start()
        {
            if (autoStart)
            {
                StartCoroutine(RunDemoSequence());
            }
        }

        public void StartDemo()
        {
            if (!isRunning)
            {
                StartCoroutine(RunDemoSequence());
            }
        }

        private IEnumerator RunDemoSequence()
        {
            isRunning = true;
            Debug.Log("[AutomatedDemoDirector] 开始自动演示序列");

            // 等待初始化
            yield return new WaitForSeconds(startDelay);

            // 1. 欢迎场景
            yield return StartCoroutine(ShowWelcomeScene());

            // 2. 景点概览
            yield return StartCoroutine(ShowLocationOverview());

            // 3. AI对话演示
            yield return StartCoroutine(ShowAIDialogDemo());

            // 4. 景点详情展示
            yield return StartCoroutine(ShowLocationDetails());

            // 5. 交互演示
            yield return StartCoroutine(ShowInteractionDemo());

            // 6. 结束场景
            yield return StartCoroutine(ShowEndingScene());

            isRunning = false;
            Debug.Log("[AutomatedDemoDirector] 演示序列完成");
        }

        private IEnumerator ShowWelcomeScene()
        {
            Debug.Log("[AutomatedDemoDirector] 场景1: 欢迎");

            // 移动相机到初始位置
            if (cameraWaypoints.Length > 0)
            {
                yield return StartCoroutine(MoveCameraTo(cameraWaypoints[0]));
            }

            // 显示欢迎信息
            if (demoController != null)
            {
                demoController.StartTourGuide();
            }

            yield return new WaitForSeconds(5f);
        }

        private IEnumerator ShowLocationOverview()
        {
            Debug.Log("[AutomatedDemoDirector] 场景2: 景点概览");

            // 旋转相机展示所有景点
            float rotationTime = 8f;
            float elapsed = 0f;
            Vector3 originalPos = demoCamera != null ? demoCamera.transform.position : Vector3.zero;

            while (elapsed < rotationTime)
            {
                elapsed += Time.deltaTime;
                if (demoCamera != null)
                {
                    demoCamera.transform.position = originalPos;
                    demoCamera.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * 15f);
                }
                yield return null;
            }

            // 恢复相机位置
            if (demoCamera != null)
            {
                demoCamera.transform.position = originalPos;
                demoCamera.transform.rotation = Quaternion.identity;
            }

            yield return new WaitForSeconds(1f);
        }

        private IEnumerator ShowAIDialogDemo()
        {
            Debug.Log("[AutomatedDemoDirector] 场景3: AI对话演示");

            if (interactiveDemo != null)
            {
                // 模拟用户输入
                interactiveDemo.StartCoroutine(SimulateUserInputs());

                yield return new WaitForSeconds(8f);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }
        }

        private IEnumerator ShowLocationDetails()
        {
            Debug.Log("[AutomatedDemoDirector] 场景4: 景点详情");

            // 遍历每个景点
            for (int i = 0; i < 5; i++)
            {
                if (demoController != null)
                {
                    demoController.NextLocation();
                }

                // 移动相机到对应位置
                if (cameraWaypoints.Length > i + 1)
                {
                    yield return StartCoroutine(MoveCameraTo(cameraWaypoints[i + 1]));
                }

                yield return new WaitForSeconds(4f);
            }
        }

        private IEnumerator ShowInteractionDemo()
        {
            Debug.Log("[AutomatedDemoDirector] 场景5: 交互演示");

            // 演示按钮交互
            yield return new WaitForSeconds(3f);
        }

        private IEnumerator ShowEndingScene()
        {
            Debug.Log("[AutomatedDemoDirector] 场景6: 结束");

            // 移动相机到最终位置
            if (cameraWaypoints.Length > 0)
            {
                yield return StartCoroutine(MoveCameraTo(cameraWaypoints[0]));
            }

            yield return new WaitForSeconds(3f);
        }

        private IEnumerator MoveCameraTo(Transform target)
        {
            if (demoCamera == null || target == null) yield break;

            float duration = Vector3.Distance(demoCamera.transform.position, target.position) / cameraMoveSpeed;
            float elapsed = 0f;
            Vector3 startPos = demoCamera.transform.position;
            Quaternion startRot = demoCamera.transform.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 平滑插值
                t = SmoothStep(t);

                demoCamera.transform.position = Vector3.Lerp(startPos, target.position, t);
                demoCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

                yield return null;
            }

            demoCamera.transform.position = target.position;
            demoCamera.transform.rotation = target.rotation;
        }

        private float SmoothStep(float t)
        {
            return t * t * (3 - 2 * t);
        }

        private IEnumerator SimulateUserInputs()
        {
            // 模拟多个用户输入
            string[] inputs = new string[]
            {
                "你好",
                "纽约有哪些著名景点？",
                "介绍一下时代广场",
                "推荐一些好吃的地方"
            };

            foreach (string input in inputs)
            {
                if (interactiveDemo != null)
                {
                    interactiveDemo.SendMessage("ShowConversation", input);
                }
                yield return new WaitForSeconds(2f);
            }
        }

        private void OnDrawGizmos()
        {
            // 绘制相机路径
            if (cameraWaypoints != null && cameraWaypoints.Length > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < cameraWaypoints.Length - 1; i++)
                {
                    if (cameraWaypoints[i] != null && cameraWaypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(cameraWaypoints[i].position, cameraWaypoints[i + 1].position);
                    }
                }
            }

            // 绘制相机位置
            if (cameraWaypoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var waypoint in cameraWaypoints)
                {
                    if (waypoint != null)
                    {
                        Gizmos.DrawWireSphere(waypoint.position, 0.5f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 演示动作数据结构
    /// </summary>
    public class DemoAction
    {
        public string actionName;
        public float duration;
        public object[] parameters;
    }
}
