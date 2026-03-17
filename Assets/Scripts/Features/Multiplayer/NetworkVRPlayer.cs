using System;
using Unity.Netcode;
using UnityEngine;

namespace TripMeta.Features.Multiplayer
{
    /// <summary>
    /// VR玩家网络同步组件
    /// </summary>
    public class NetworkVRPlayer : NetworkBehaviour
    {
        [Header("VR组件引用")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        [Header("网络同步设置")]
        [SerializeField] private float positionThreshold = 0.01f;
        [SerializeField] private float rotationThreshold = 1f;
        [SerializeField] private float syncInterval = 0.05f; // 20Hz

        // 网络变量 - 玩家信息
        public NetworkVariable<string> PlayerName = new NetworkVariable<string>("Player");
        public NetworkVariable<bool> IsSpeaking = new NetworkVariable<bool>(false);
        public NetworkVariable<int> CurrentAttraction = new NetworkVariable<int>(-1);

        // 网络变量 - 头部
        private NetworkVariable<Vector3> headPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> headRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // 网络变量 - 左手
        private NetworkVariable<Vector3> leftHandPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> leftHandRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // 网络变量 - 右手
        private NetworkVariable<Vector3> rightHandPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> rightHandRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // 本地缓存
        private Vector3 lastHeadPosition;
        private Quaternion lastHeadRotation;
        private Vector3 lastLeftHandPosition;
        private Quaternion lastLeftHandRotation;
        private Vector3 lastRightHandPosition;
        private Quaternion lastRightHandRotation;

        private float lastSyncTime;

        // 事件
        public event Action<bool> OnSpeakingStateChanged;

        public string PlayerNameValue => PlayerName.Value;

        private void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                // 本地玩家 - 发送同步数据
                UpdateLocalPlayer();
            }
            else
            {
                // 远程玩家 - 应用同步数据
                UpdateRemotePlayer();
            }
        }

        private void UpdateLocalPlayer()
        {
            if (Time.time - lastSyncTime < syncInterval) return;

            // 更新头部位置和旋转
            if (headTransform != null)
            {
                if (Vector3.Distance(headTransform.position, lastHeadPosition) > positionThreshold ||
                    Quaternion.Angle(headTransform.rotation, lastHeadRotation) > rotationThreshold)
                {
                    headPosition.Value = headTransform.position;
                    headRotation.Value = headTransform.rotation;
                    lastHeadPosition = headTransform.position;
                    lastHeadRotation = headTransform.rotation;
                }
            }

            // 更新左手
            if (leftHandTransform != null)
            {
                if (Vector3.Distance(leftHandTransform.position, lastLeftHandPosition) > positionThreshold ||
                    Quaternion.Angle(leftHandTransform.rotation, lastLeftHandRotation) > rotationThreshold)
                {
                    leftHandPosition.Value = leftHandTransform.position;
                    leftHandRotation.Value = leftHandTransform.rotation;
                    lastLeftHandPosition = leftHandTransform.position;
                    lastLeftHandRotation = leftHandTransform.rotation;
                }
            }

            // 更新右手
            if (rightHandTransform != null)
            {
                if (Vector3.Distance(rightHandTransform.position, lastRightHandPosition) > positionThreshold ||
                    Quaternion.Angle(rightHandTransform.rotation, lastRightHandRotation) > rotationThreshold)
                {
                    rightHandPosition.Value = rightHandTransform.position;
                    rightHandRotation.Value = rightHandTransform.rotation;
                    lastRightHandPosition = rightHandTransform.position;
                    lastRightHandRotation = rightHandTransform.rotation;
                }
            }

            lastSyncTime = Time.time;
        }

        private void UpdateRemotePlayer()
        {
            // 平滑插值到目标位置
            if (headTransform != null)
            {
                headTransform.position = Vector3.Lerp(headTransform.position, headPosition.Value, 0.3f);
                headTransform.rotation = Quaternion.Slerp(headTransform.rotation, headRotation.Value, 0.3f);
            }

            if (leftHandTransform != null)
            {
                leftHandTransform.position = Vector3.Lerp(leftHandTransform.position, leftHandPosition.Value, 0.3f);
                leftHandTransform.rotation = Quaternion.Slerp(leftHandTransform.rotation, leftHandRotation.Value, 0.3f);
            }

            if (rightHandTransform != null)
            {
                rightHandTransform.position = Vector3.Lerp(rightHandTransform.position, rightHandPosition.Value, 0.3f);
                rightHandTransform.rotation = Quaternion.Slerp(rightHandTransform.rotation, rightHandRotation.Value, 0.3f);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                // 获取本地VR设备的引用
                FindLocalVRComponents();

                // 设置玩家名称
                PlayerName.Value = $"Player {OwnerClientId}";
            }
            else
            {
                // 远程玩家 - 创建或获取对应的视觉表示
                SetupRemotePlayerVisuals();
            }

            // 订阅网络变量变化
            IsSpeaking.OnValueChanged += OnSpeakingChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            IsSpeaking.OnValueChanged -= OnSpeakingChanged;
        }

        private void FindLocalVRComponents()
        {
            // 查找VR设备
            // 这里简化处理，实际应该通过VRManager获取
            var camera = Camera.main;
            if (camera != null)
            {
                headTransform = camera.transform;
            }

            // 查找手部控制器
            var leftController = GameObject.Find("LeftHand Controller");
            if (leftController != null)
            {
                leftHandTransform = leftController.transform;
            }

            var rightController = GameObject.Find("RightHand Controller");
            if (rightController != null)
            {
                rightHandTransform = rightController.transform;
            }
        }

        private void SetupRemotePlayerVisuals()
        {
            // 为远程玩家创建头部和手部模型
            // 这里应该实例化远程玩家预制体
            Debug.Log($"[NetworkVRPlayer] 设置远程玩家 {OwnerClientId} 的视觉表示");
        }

        private void OnSpeakingChanged(bool previous, bool current)
        {
            OnSpeakingStateChanged?.Invoke(current);
        }

        /// <summary>
        /// 设置说话状态
        /// </summary>
        public void SetSpeakingState(bool isSpeaking)
        {
            if (IsOwner)
            {
                IsSpeaking.Value = isSpeaking;
            }
        }

        /// <summary>
        /// 设置当前景点
        /// </summary>
        public void SetCurrentAttraction(int attractionIndex)
        {
            if (IsOwner)
            {
                CurrentAttraction.Value = attractionIndex;
            }
        }

        /// <summary>
        /// 播放远程玩家的语音
        /// </summary>
        public void PlayRemoteVoice(byte[] audioData)
        {
            if (!IsOwner)
            {
                // 播放接收到的语音数据
                Debug.Log($"[NetworkVRPlayer] 播放来自 {OwnerClientId} 的语音数据");
                // 这里应该使用 AudioSource 播放语音
            }
        }
    }
}
