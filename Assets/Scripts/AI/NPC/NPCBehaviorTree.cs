using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI.NPC
{
    /// <summary>
    /// NPC行为树 - 管理NPC的自主行为
    /// 支持巡逻、问候、对话等状态切换
    /// </summary>
    public class NPCBehaviorTree : MonoBehaviour
    {
        [Header("行为配置")]
        [SerializeField] private bool enablePatrol = true;
        [SerializeField] private float idleTimeMin = 3f;
        [SerializeField] private float idleTimeMax = 10f;
        [SerializeField] private float greetingDuration = 5f;
        [SerializeField] private float farewellDuration = 3f;
        
        [Header("动画参数")]
        [SerializeField] private string idleAnimParam = "Idle";
        [SerializeField] private string walkAnimParam = "Walk";
        [SerializeField] private string talkAnimParam = "Talk";
        [SerializeField] private string waveAnimParam = "Wave";
        
        // 组件引用
        private NPCAIController controller;
        private NPCPersonality personality;
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        
        // 巡逻状态
        private int currentWaypointIndex = 0;
        private bool isPatrolling = false;
        private float waypointWaitTimer = 0f;
        
        // 计时器
        private float stateTimer = 0f;
        private float idleTimer = 0f;
        
        // 当前行为节点
        private BehaviorNode currentNode;
        
        // 状态
        private bool isInitialized = false;
        
        public bool IsInitialized => isInitialized;
        
        /// <summary>
        /// 初始化行为树
        /// </summary>
        public void Initialize(NPCAIController npcController, NPCPersonality npcPersonality)
        {
            try
            {
                controller = npcController;
                personality = npcPersonality;
                
                navMeshAgent = controller.GetComponent<NavMeshAgent>();
                animator = controller.GetComponent<Animator>();
                
                // 启用巡逻（如果有路径点）
                if (personality.enablePatrol && personality.patrolWaypoints != null && personality.patrolWaypoints.Length > 0)
                {
                    enablePatrol = true;
                }
                else
                {
                    enablePatrol = false;
                }
                
                // 构建行为树
                BuildBehaviorTree();
                
                isInitialized = true;
                
                Logger.LogInfo($"[NPCBehaviorTree] Behavior tree initialized for {personality.npcName}", "NPC");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to initialize NPC behavior tree");
            }
        }
        
        /// <summary>
        /// 构建行为树
        /// </summary>
        private void BuildBehaviorTree()
        {
            // 简单的优先级行为树
            // 优先级: 对话 > 问候 > 告别 > 巡逻 > 空闲
            currentNode = new SelectorNode(new List<BehaviorNode>
            {
                // 对话中
                new SequenceNode(new List<BehaviorNode>
                {
                    new ConditionNode(() => controller.IsConversing),
                    new ActionNode(() => ExecuteConversing())
                }),
                
                // 问候玩家
                new SequenceNode(new List<BehaviorNode>
                {
                    new ConditionNode(() => controller.CurrentState == NPCState.Greeting),
                    new ActionNode(() => ExecuteGreeting())
                }),
                
                // 告别
                new SequenceNode(new List<BehaviorNode>
                {
                    new ConditionNode(() => controller.CurrentState == NPCState.Farewell),
                    new ActionNode(() => ExecuteFarewell())
                }),
                
                // 思考中
                new SequenceNode(new List<BehaviorNode>
                {
                    new ConditionNode(() => controller.CurrentState == NPCState.Thinking),
                    new ActionNode(() => ExecuteThinking())
                }),
                
                // 巡逻
                new SequenceNode(new List<BehaviorNode>
                {
                    new ConditionNode(() => enablePatrol && controller.CurrentState == NPCState.Idle),
                    new ActionNode(() => ExecutePatrol())
                }),
                
                // 空闲
                new ActionNode(() => ExecuteIdle())
            });
        }
        
        /// <summary>
        /// 更新行为
        /// </summary>
        public void UpdateBehavior()
        {
            if (!isInitialized || currentNode == null) return;
            
            try
            {
                currentNode.Execute();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Behavior tree execution error");
            }
        }
        
        /// <summary>
        /// 执行对话行为
        /// </summary>
        private BehaviorStatus ExecuteConversing()
        {
            // 面向玩家
            if (controller != null && controller.gameObject.activeInHierarchy)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    controller.LookAt(player.transform.position);
                }
            }
            
            // 播放对话动画
            SetAnimatorBool(talkAnimParam, true);
            SetAnimatorBool(idleAnimParam, false);
            SetAnimatorBool(walkAnimParam, false);
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 执行问候行为
        /// </summary>
        private BehaviorStatus ExecuteGreeting()
        {
            stateTimer += Time.deltaTime;
            
            // 面向玩家
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                controller.LookAt(player.transform.position);
            }
            
            // 播放挥手动画
            SetAnimatorTrigger(waveAnimParam);
            
            if (stateTimer >= greetingDuration)
            {
                stateTimer = 0f;
                return BehaviorStatus.Success;
            }
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 执行告别行为
        /// </summary>
        private BehaviorStatus ExecuteFarewell()
        {
            stateTimer += Time.deltaTime;
            
            // 播放挥手动画
            SetAnimatorTrigger(waveAnimParam);
            
            if (stateTimer >= farewellDuration)
            {
                stateTimer = 0f;
                return BehaviorStatus.Success;
            }
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 执行思考行为
        /// </summary>
        private BehaviorStatus ExecuteThinking()
        {
            // 播放思考动画（如果有）
            SetAnimatorBool(idleAnimParam, true);
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 执行巡逻行为
        /// </summary>
        private BehaviorStatus ExecutePatrol()
        {
            if (personality.patrolWaypoints == null || personality.patrolWaypoints.Length == 0)
            {
                return BehaviorStatus.Failure;
            }
            
            // 检查是否到达目标点
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                // 在路径点等待
                waypointWaitTimer += Time.deltaTime;
                
                SetAnimatorBool(walkAnimParam, false);
                SetAnimatorBool(idleAnimParam, true);
                
                if (waypointWaitTimer >= personality.waypointWaitTime)
                {
                    // 移动到下一个路径点
                    MoveToNextWaypoint();
                    waypointWaitTimer = 0f;
                }
            }
            else
            {
                // 移动中
                SetAnimatorBool(walkAnimParam, true);
                SetAnimatorBool(idleAnimParam, false);
            }
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 执行空闲行为
        /// </summary>
        private BehaviorStatus ExecuteIdle()
        {
            idleTimer += Time.deltaTime;
            
            SetAnimatorBool(idleAnimParam, true);
            SetAnimatorBool(walkAnimParam, false);
            SetAnimatorBool(talkAnimParam, false);
            
            // 随机idle动作
            if (idleTimer >= UnityEngine.Random.Range(idleTimeMin, idleTimeMax))
            {
                idleTimer = 0f;
                // 可以播放随机idle动画
            }
            
            return BehaviorStatus.Running;
        }
        
        /// <summary>
        /// 移动到下一个巡逻点
        /// </summary>
        private void MoveToNextWaypoint()
        {
            if (personality.patrolWaypoints.Length == 0) return;
            
            currentWaypointIndex = (currentWaypointIndex + 1) % personality.patrolWaypoints.Length;
            var targetWaypoint = personality.patrolWaypoints[currentWaypointIndex];
            
            controller.MoveTo(targetWaypoint);
            
            Logger.LogInfo($"[NPCBehaviorTree] {personality.npcName} moving to waypoint {currentWaypointIndex}", "NPC");
        }
        
        /// <summary>
        /// 停止巡逻
        /// </summary>
        public void StopPatrol()
        {
            isPatrolling = false;
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = true;
            }
        }
        
        /// <summary>
        /// 恢复巡逻
        /// </summary>
        public void ResumePatrol()
        {
            if (enablePatrol)
            {
                isPatrolling = true;
                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.isStopped = false;
                }
            }
        }
        
        /// <summary>
        /// 设置Animator Bool参数
        /// </summary>
        private void SetAnimatorBool(string paramName, bool value)
        {
            if (animator != null && !string.IsNullOrEmpty(paramName))
            {
                try
                {
                    animator.SetBool(paramName, value);
                }
                catch (Exception ex)
                {
                    // 忽略动画参数不存在错误
                }
            }
        }
        
        /// <summary>
        /// 设置Animator Trigger参数
        /// </summary>
        private void SetAnimatorTrigger(string paramName)
        {
            if (animator != null && !string.IsNullOrEmpty(paramName))
            {
                try
                {
                    animator.SetTrigger(paramName);
                }
                catch (Exception ex)
                {
                    // 忽略动画参数不存在错误
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (personality == null || personality.patrolWaypoints == null) return;
            
            // 绘制巡逻路径
            Gizmos.color = Color.cyan;
            for (int i = 0; i < personality.patrolWaypoints.Length; i++)
            {
                var current = personality.patrolWaypoints[i];
                var next = personality.patrolWaypoints[(i + 1) % personality.patrolWaypoints.Length];
                
                Gizmos.DrawWireSphere(current, 0.3f);
                Gizmos.DrawLine(current, next);
            }
        }
    }
    
    #region 行为树节点
    
    /// <summary>
    /// 行为状态
    /// </summary>
    public enum BehaviorStatus
    {
        Success,
        Failure,
        Running
    }
    
    /// <summary>
    /// 行为节点基类
    /// </summary>
    public abstract class BehaviorNode
    {
        public abstract BehaviorStatus Execute();
    }
    
    /// <summary>
    /// 动作节点
    /// </summary>
    public class ActionNode : BehaviorNode
    {
        private Func<BehaviorStatus> action;
        
        public ActionNode(Func<BehaviorStatus> action)
        {
            this.action = action;
        }
        
        public override BehaviorStatus Execute()
        {
            return action?.Invoke() ?? BehaviorStatus.Failure;
        }
    }
    
    /// <summary>
    /// 条件节点
    /// </summary>
    public class ConditionNode : BehaviorNode
    {
        private Func<bool> condition;
        
        public ConditionNode(Func<bool> condition)
        {
            this.condition = condition;
        }
        
        public override BehaviorStatus Execute()
        {
            return condition?.Invoke() == true ? BehaviorStatus.Success : BehaviorStatus.Failure;
        }
    }
    
    /// <summary>
    /// 序列节点（顺序执行）
    /// </summary>
    public class SequenceNode : BehaviorNode
    {
        private List<BehaviorNode> children;
        private int currentIndex = 0;
        
        public SequenceNode(List<BehaviorNode> children)
        {
            this.children = children;
        }
        
        public override BehaviorStatus Execute()
        {
            while (currentIndex < children.Count)
            {
                var status = children[currentIndex].Execute();
                
                if (status == BehaviorStatus.Running)
                {
                    return BehaviorStatus.Running;
                }
                else if (status == BehaviorStatus.Failure)
                {
                    currentIndex = 0;
                    return BehaviorStatus.Failure;
                }
                
                currentIndex++;
            }
            
            currentIndex = 0;
            return BehaviorStatus.Success;
        }
    }
    
    /// <summary>
    /// 选择节点（优先执行）
    /// </summary>
    public class SelectorNode : BehaviorNode
    {
        private List<BehaviorNode> children;
        
        public SelectorNode(List<BehaviorNode> children)
        {
            this.children = children;
        }
        
        public override BehaviorStatus Execute()
        {
            foreach (var child in children)
            {
                var status = child.Execute();
                
                if (status == BehaviorStatus.Success || status == BehaviorStatus.Running)
                {
                    return status;
                }
            }
            
            return BehaviorStatus.Failure;
        }
    }
    
    #endregion
}
