using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Enterprise.Services;

namespace TripMeta.Enterprise
{
    /// <summary>
    /// 企业级管理器
    /// 处理SSO/SAML、团队管理、高级安全、审计日志
    /// </summary>
    public class EnterpriseManager : MonoBehaviour
    {
        [Header("SSO配置")]
        public bool enableSSO = true;
        public bool enableSAML = true;
        public bool enableOIDC = true;
        public string samlMetadataUrl = "";
        public string oidcAuthority = "";

        [Header("团队管理")]
        public int maxTeamSize = 100;
        public int maxProjectsPerTeam = 10;
        public bool enableRoleManagement = true;
        public bool enableDepartmentHierarchy = true;

        [Header("安全设置")]
        public bool enable2FA = true;
        public bool enforcePasswordPolicy = true;
        public int passwordMinLength = 12;
        public int sessionTimeoutMinutes = 480;
        public bool enableIPWhitelist = true;
        public bool encryptDataAtRest = true;
        public bool encryptDataInTransit = true;

        [Header("审计日志")]
        public bool enableAuditLogging = true;
        public AuditLogLevel logLevel = AuditLogLevel.Detailed;
        public int logRetentionDays = 365;
        public bool logAdminActions = true;
        public bool logDataAccess = true;
        public bool logAuthentication = true;

        // 当前企业
        private EnterpriseProfile currentEnterprise;
        private List<TeamMember> teamMembers = new List<TeamMember>();
        private List<AuditLogEntry> auditLogBuffer = new List<AuditLogEntry>();

        // 服务
        private SSOService ssoService;
        private TeamManagementService teamService;
        private SecurityService securityService;
        private AuditLogService auditService;

        public static EnterpriseManager Instance { get; private set; }

        public EnterpriseProfile CurrentEnterprise => currentEnterprise;
        public IReadOnlyList<TeamMember> TeamMembers => teamMembers;
        public bool IsEnterpriseMode => currentEnterprise != null;

        // 事件
        public event Action<TeamMember> OnTeamMemberAdded;
        public event Action<TeamMember> OnTeamMemberRemoved;
        public event Action<AuditLogEntry> OnAuditLogEntry;
        public event Action<string> OnSecurityAlert;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            InitializeServices();
            Debug.Log("[EnterpriseManager] 企业级管理器初始化完成");
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitializeServices()
        {
            if (enableSSO)
                ssoService = new SSOService();
            if (enableRoleManagement)
                teamService = new TeamManagementService();
            if (enableAuditLogging)
                auditService = new AuditLogService(logRetentionDays);
            securityService = new SecurityService(enableIPWhitelist, encryptDataAtRest);
        }

        #region SSO/SAML

        /// <summary>
        /// 配置SAML
        /// </summary>
        public async Task<bool> ConfigureSAML(string metadataUrl, string entityId)
        {
            if (!enableSAML || ssoService == null) return false;

            var result = await ssoService.ConfigureSAML(metadataUrl, entityId);
            if (result)
            {
                LogAuditEvent("sso_configured", "SAML", $"EntityId: {entityId}");
            }
            return result;
        }

        /// <summary>
        /// 配置OIDC
        /// </summary>
        public async Task<bool> ConfigureOIDC(string authority, string clientId, string clientSecret)
        {
            if (!enableOIDC || ssoService == null) return false;

            var result = await ssoService.ConfigureOIDC(authority, clientId, clientSecret);
            if (result)
            {
                LogAuditEvent("sso_configured", "OIDC", $"Authority: {authority}");
            }
            return result;
        }

        /// <summary>
        /// 使用SSO登录
        /// </summary>
        public async Task<SSOLoginResult> LoginWithSSO(string provider, string token)
        {
            if (!enableSSO || ssoService == null)
            {
                return new SSOLoginResult { success = false, error = "SSO not enabled" };
            }

            var result = await ssoService.Authenticate(provider, token);
            if (result.success)
            {
                LogAuditEvent("sso_login", provider, $"User: {result.userEmail}");
            }
            else
            {
                LogAuditEvent("sso_login_failed", provider, $"Error: {result.error}");
                OnSecurityAlert?.Invoke($"SSO login failed: {result.error}");
            }

            return result;
        }

        #endregion

        #region 团队管理

        /// <summary>
        /// 添加团队成员
        /// </summary>
        public async Task<bool> AddTeamMember(string email, string role, string department = null)
        {
            if (!enableRoleManagement || teamService == null) return false;

            if (teamMembers.Count >= maxTeamSize)
            {
                Debug.LogError("[EnterpriseManager] 团队人数已达上限");
                return false;
            }

            var member = new TeamMember
            {
                memberId = Guid.NewGuid().ToString(),
                email = email,
                role = role,
                department = department,
                joinedAt = DateTime.Now,
                status = MemberStatus.Active,
                permissions = GetPermissionsForRole(role)
            };

            teamMembers.Add(member);
            OnTeamMemberAdded?.Invoke(member);
            LogAuditEvent("team_member_added", email, $"Role: {role}");

            await teamService.SyncMember(member);
            return true;
        }

        /// <summary>
        /// 移除团队成员
        /// </summary>
        public bool RemoveTeamMember(string memberId)
        {
            var member = teamMembers.FirstOrDefault(m => m.memberId == memberId);
            if (member == null) return false;

            teamMembers.Remove(member);
            OnTeamMemberRemoved?.Invoke(member);
            LogAuditEvent("team_member_removed", member.email, "Member removed from team");

            return true;
        }

        /// <summary>
        /// 更新成员角色
        /// </summary>
        public bool UpdateMemberRole(string memberId, string newRole)
        {
            var member = teamMembers.FirstOrDefault(m => m.memberId == memberId);
            if (member == null) return false;

            string oldRole = member.role;
            member.role = newRole;
            member.permissions = GetPermissionsForRole(newRole);

            LogAuditEvent("member_role_changed", member.email, $"From: {oldRole}, To: {newRole}");
            return true;
        }

        /// <summary>
        /// 获取角色权限
        /// </summary>
        private string[] GetPermissionsForRole(string role)
        {
            return role switch
            {
                "Admin" => new[] { "all" },
                "Manager" => new[] { "read", "write", "manage_users", "view_analytics" },
                "Editor" => new[] { "read", "write", "create_content" },
                "Viewer" => new[] { "read" },
                _ => new[] { "read" }
            };
        }

        /// <summary>
        /// 检查成员权限
        /// </summary>
        public bool HasPermission(string memberId, string permission)
        {
            var member = teamMembers.FirstOrDefault(m => m.memberId == memberId);
            if (member == null || member.status != MemberStatus.Active) return false;

            return member.permissions.Contains("all") || member.permissions.Contains(permission);
        }

        /// <summary>
        /// 暂停成员
        /// </summary>
        public bool SuspendMember(string memberId, string reason)
        {
            var member = teamMembers.FirstOrDefault(m => m.memberId == memberId);
            if (member == null) return false;

            member.status = MemberStatus.Suspended;
            LogAuditEvent("member_suspended", member.email, $"Reason: {reason}");
            return true;
        }

        /// <summary>
        /// 获取部门成员
        /// </summary>
        public IEnumerable<TeamMember> GetDepartmentMembers(string department)
        {
            return teamMembers.Where(m => m.department == department && m.status == MemberStatus.Active);
        }

        #endregion

        #region 高级安全

        /// <summary>
        /// 验证2FA
        /// </summary>
        public async Task<bool> Verify2FA(string userId, string code)
        {
            if (!enable2FA) return true;

            var result = await securityService.Verify2FA(userId, code);
            if (!result)
            {
                LogAuditEvent("2fa_failed", userId, "Invalid 2FA code");
                OnSecurityAlert?.Invoke($"2FA verification failed for user: {userId}");
            }
            return result;
        }

        /// <summary>
        /// 检查密码策略
        /// </summary>
        public PasswordPolicyResult CheckPasswordPolicy(string password)
        {
            if (!enforcePasswordPolicy)
            {
                return new PasswordPolicyResult { valid = true };
            }

            var result = new PasswordPolicyResult();
            var errors = new List<string>();

            if (password.Length < passwordMinLength)
                errors.Add($"Password must be at least {passwordMinLength} characters");
            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain uppercase letter");
            if (!password.Any(char.IsLower))
                errors.Add("Password must contain lowercase letter");
            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain digit");
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("Password must contain special character");

            result.valid = errors.Count == 0;
            result.errors = errors.ToArray();
            return result;
        }

        /// <summary>
        /// 检查IP白名单
        /// </summary>
        public bool CheckIPWhitelist(string ipAddress)
        {
            if (!enableIPWhitelist) return true;
            return securityService.IsIPAllowed(ipAddress);
        }

        /// <summary>
        /// 添加IP到白名单
        /// </summary>
        public void AddToIPWhitelist(string ipAddress)
        {
            securityService.AddAllowedIP(ipAddress);
            LogAuditEvent("ip_whitelist_add", ipAddress, "IP added to whitelist");
        }

        /// <summary>
        /// 加密敏感数据
        /// </summary>
        public string EncryptData(string data)
        {
            if (!encryptDataAtRest) return data;
            return securityService.Encrypt(data);
        }

        /// <summary>
        /// 解密敏感数据
        /// </summary>
        public string DecryptData(string encryptedData)
        {
            if (!encryptDataAtRest) return encryptedData;
            return securityService.Decrypt(encryptedData);
        }

        /// <summary>
        /// 检查会话是否超时
        /// </summary>
        public bool IsSessionExpired(DateTime lastActivity)
        {
            return (DateTime.Now - lastActivity).TotalMinutes > sessionTimeoutMinutes;
        }

        #endregion

        #region 审计日志

        /// <summary>
        /// 记录审计事件
        /// </summary>
        public void LogAuditEvent(string action, string target, string details)
        {
            if (!enableAuditLogging) return;

            var entry = new AuditLogEntry
            {
                entryId = Guid.NewGuid().ToString(),
                timestamp = DateTime.Now,
                action = action,
                target = target,
                details = details,
                userId = GetCurrentUserId(),
                ipAddress = GetClientIPAddress(),
                severity = GetSeverityForAction(action)
            };

            auditLogBuffer.Add(entry);
            OnAuditLogEntry?.Invoke(entry);

            if (auditLogBuffer.Count >= 100)
            {
                FlushAuditLog();
            }
        }

        /// <summary>
        /// 刷新审计日志
        /// </summary>
        private async void FlushAuditLog()
        {
            if (auditLogBuffer.Count == 0 || auditService == null) return;

            var logsToSave = auditLogBuffer.ToList();
            auditLogBuffer.Clear();

            await auditService.SaveAuditLogs(logsToSave);
        }

        /// <summary>
        /// 获取审计日志
        /// </summary>
        public async Task<AuditLogEntry[]> GetAuditLogs(DateTime startDate, DateTime endDate, string action = null)
        {
            if (auditService == null) return new AuditLogEntry[0];
            return await auditService.GetLogs(startDate, endDate, action);
        }

        /// <summary>
        /// 获取操作严重级别
        /// </summary>
        private LogSeverity GetSeverityForAction(string action)
        {
            if (action.Contains("delete") || action.Contains("remove"))
                return LogSeverity.High;
            if (action.Contains("failed") || action.Contains("error"))
                return LogSeverity.Critical;
            if (action.Contains("login") || action.Contains("auth"))
                return LogSeverity.Medium;
            return LogSeverity.Low;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private string GetCurrentUserId()
        {
            return "current_user";
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        private string GetClientIPAddress()
        {
            return "0.0.0.0";
        }

        #endregion

        /// <summary>
        /// 初始化企业配置
        /// </summary>
        public void InitializeEnterprise(EnterpriseProfile profile)
        {
            currentEnterprise = profile;
            LogAuditEvent("enterprise_initialized", profile.enterpriseId, $"Name: {profile.name}");
        }

        void OnApplicationQuit()
        {
            FlushAuditLog();
        }

        void OnDestroy()
        {
            FlushAuditLog();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 企业档案
    /// </summary>
    public class EnterpriseProfile
    {
        public string enterpriseId;
        public string name;
        public string domain;
        public string billingEmail;
        public string plan;
        public DateTime createdAt;
        public string[] configuredAuthProviders;
    }

    /// <summary>
    /// 团队成员
    /// </summary>
    public class TeamMember
    {
        public string memberId;
        public string email;
        public string name;
        public string role;
        public string department;
        public DateTime joinedAt;
        public MemberStatus status;
        public string[] permissions;
        public DateTime? lastLoginAt;
    }

    /// <summary>
    /// 成员状态
    /// </summary>
    public enum MemberStatus
    {
        Active,
        Inactive,
        Suspended,
        Pending
    }

    /// <summary>
    /// 审计日志条目
    /// </summary>
    public class AuditLogEntry
    {
        public string entryId;
        public DateTime timestamp;
        public string action;
        public string target;
        public string details;
        public string userId;
        public string ipAddress;
        public LogSeverity severity;
    }

    /// <summary>
    /// 日志严重级别
    /// </summary>
    public enum LogSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// 审计日志级别
    /// </summary>
    public enum AuditLogLevel
    {
        Basic,
        Detailed,
        Verbose
    }

    /// <summary>
    /// SSO登录结果
    /// </summary>
    public class SSOLoginResult
    {
        public bool success;
        public string error;
        public string userId;
        public string userEmail;
        public string[] roles;
        public string token;
        public string provider;
    }

    /// <summary>
    /// 密码策略结果
    /// </summary>
    public class PasswordPolicyResult
    {
        public bool valid;
        public string[] errors;
    }

    #endregion
}
