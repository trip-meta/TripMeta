using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Enterprise.Services
{
    /// <summary>
    /// 团队管理服务
    /// 处理成员同步、部门层级、邀请管理
    /// </summary>
    public class TeamManagementService
    {
        private readonly Dictionary<string, TeamMember> memberCache = new Dictionary<string, TeamMember>();
        private readonly List<PendingInvitation> pendingInvitations = new List<PendingInvitation>();

        public int TotalMembers => memberCache.Count;

        /// <summary>
        /// 同步成员数据
        /// </summary>
        public async Task SyncMember(TeamMember member)
        {
            if (member == null || string.IsNullOrEmpty(member.memberId)) return;

            await Task.Delay(10);
            memberCache[member.memberId] = member;
            Debug.Log($"[TeamManagementService] Member synced: {member.email}");
        }

        /// <summary>
        /// 批量同步成员
        /// </summary>
        public async Task BulkSyncMembers(IEnumerable<TeamMember> members)
        {
            await Task.Delay(20);
            foreach (var member in members)
            {
                memberCache[member.memberId] = member;
            }
        }

        /// <summary>
        /// 发送邀请
        /// </summary>
        public async Task<InvitationResult> SendInvitation(string email, string role, string invitedBy)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new InvitationResult { success = false, error = "Email is required" };
            }

            // 检查是否已有未处理的邀请
            var existing = pendingInvitations.FirstOrDefault(i =>
                i.email == email && i.status == InvitationStatus.Pending);
            if (existing != null)
            {
                return new InvitationResult { success = false, error = "Invitation already sent" };
            }

            await Task.Delay(30);

            var invitation = new PendingInvitation
            {
                invitationId = Guid.NewGuid().ToString(),
                email = email,
                role = role,
                invitedBy = invitedBy,
                sentAt = DateTime.Now,
                expiresAt = DateTime.Now.AddDays(7),
                status = InvitationStatus.Pending,
                token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            };

            pendingInvitations.Add(invitation);
            Debug.Log($"[TeamManagementService] Invitation sent to: {email} (role: {role})");

            return new InvitationResult
            {
                success = true,
                invitationId = invitation.invitationId,
                expiresAt = invitation.expiresAt
            };
        }

        /// <summary>
        /// 接受邀请
        /// </summary>
        public async Task<bool> AcceptInvitation(string token, string userId)
        {
            await Task.Delay(10);

            var invitation = pendingInvitations.FirstOrDefault(i =>
                i.token == token &&
                i.status == InvitationStatus.Pending &&
                i.expiresAt > DateTime.Now);

            if (invitation == null) return false;

            invitation.status = InvitationStatus.Accepted;
            return true;
        }

        /// <summary>
        /// 撤销邀请
        /// </summary>
        public bool RevokeInvitation(string invitationId)
        {
            var invitation = pendingInvitations.FirstOrDefault(i => i.invitationId == invitationId);
            if (invitation == null) return false;

            invitation.status = InvitationStatus.Revoked;
            return true;
        }

        /// <summary>
        /// 获取待处理邀请
        /// </summary>
        public IReadOnlyList<PendingInvitation> GetPendingInvitations()
        {
            return pendingInvitations
                .Where(i => i.status == InvitationStatus.Pending && i.expiresAt > DateTime.Now)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// 获取成员统计
        /// </summary>
        public TeamStats GetStats()
        {
            var members = memberCache.Values.ToList();
            var roleGroups = members.GroupBy(m => m.role).ToDictionary(g => g.Key, g => g.Count());
            var deptGroups = members.Where(m => m.department != null)
                                    .GroupBy(m => m.department)
                                    .ToDictionary(g => g.Key, g => g.Count());

            return new TeamStats
            {
                totalMembers = members.Count,
                activeMembers = members.Count(m => m.status == MemberStatus.Active),
                suspendedMembers = members.Count(m => m.status == MemberStatus.Suspended),
                membersByRole = roleGroups,
                membersByDepartment = deptGroups,
                pendingInvitations = pendingInvitations.Count(i => i.status == InvitationStatus.Pending)
            };
        }
    }

    /// <summary>
    /// 待处理邀请
    /// </summary>
    public class PendingInvitation
    {
        public string invitationId;
        public string email;
        public string role;
        public string invitedBy;
        public DateTime sentAt;
        public DateTime expiresAt;
        public InvitationStatus status;
        public string token;
    }

    /// <summary>
    /// 邀请状态
    /// </summary>
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Expired,
        Revoked
    }

    /// <summary>
    /// 邀请结果
    /// </summary>
    public class InvitationResult
    {
        public bool success;
        public string error;
        public string invitationId;
        public DateTime expiresAt;
    }

    /// <summary>
    /// 团队统计
    /// </summary>
    public class TeamStats
    {
        public int totalMembers;
        public int activeMembers;
        public int suspendedMembers;
        public Dictionary<string, int> membersByRole;
        public Dictionary<string, int> membersByDepartment;
        public int pendingInvitations;
    }
}
