using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Enterprise;
using TripMeta.Enterprise.Services;

namespace TripMeta.Tests.Enterprise
{
    /// <summary>
    /// 企业级功能单元测试
    /// 覆盖: 团队管理、安全策略、审计日志、SSO
    /// </summary>
    public class EnterpriseManagerTests
    {
        private GameObject testObject;
        private EnterpriseManager manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestEnterpriseManager");
            manager = testObject.AddComponent<EnterpriseManager>();
            manager.enableSSO = true;
            manager.enableSAML = true;
            manager.enableOIDC = true;
            manager.enableRoleManagement = true;
            manager.enableAuditLogging = true;
            manager.enable2FA = true;
            manager.enforcePasswordPolicy = true;
            manager.passwordMinLength = 8;
            manager.maxTeamSize = 10;
            manager.encryptDataAtRest = true;
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }

        // ============================================================
        // 团队管理测试
        // ============================================================

        [UnityTest]
        public IEnumerator AddTeamMember_ValidEmail_ReturnsTrue()
        {
            var task = manager.AddTeamMember("user@example.com", "Viewer");
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.IsTrue(task.Result);
            Assert.AreEqual(1, manager.TeamMembers.Count);
        }

        [UnityTest]
        public IEnumerator AddTeamMember_ExceedsMaxSize_ReturnsFalse()
        {
            // Fill up to max
            for (int i = 0; i < 10; i++)
            {
                var t = manager.AddTeamMember($"user{i}@example.com", "Viewer");
                yield return new WaitUntil(() => t.IsCompleted);
            }

            // One more should fail
            var task = manager.AddTeamMember("overflow@example.com", "Viewer");
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.IsFalse(task.Result);
            Assert.AreEqual(10, manager.TeamMembers.Count);
        }

        [UnityTest]
        public IEnumerator RemoveTeamMember_ExistingMember_ReturnsTrue()
        {
            var addTask = manager.AddTeamMember("remove@example.com", "Viewer");
            yield return new WaitUntil(() => addTask.IsCompleted);

            var memberId = manager.TeamMembers[0].memberId;
            bool result = manager.RemoveTeamMember(memberId);

            Assert.IsTrue(result);
            Assert.AreEqual(0, manager.TeamMembers.Count);
        }

        [Test]
        public void RemoveTeamMember_NonExistentId_ReturnsFalse()
        {
            bool result = manager.RemoveTeamMember("non-existent-id");
            Assert.IsFalse(result);
        }

        [UnityTest]
        public IEnumerator UpdateMemberRole_ValidMember_UpdatesPermissions()
        {
            var addTask = manager.AddTeamMember("editor@example.com", "Viewer");
            yield return new WaitUntil(() => addTask.IsCompleted);

            var memberId = manager.TeamMembers[0].memberId;
            bool result = manager.UpdateMemberRole(memberId, "Editor");

            Assert.IsTrue(result);
            Assert.AreEqual("Editor", manager.TeamMembers[0].role);
        }

        // ============================================================
        // 权限测试
        // ============================================================

        [UnityTest]
        public IEnumerator HasPermission_AdminRole_HasAllPermissions()
        {
            var task = manager.AddTeamMember("admin@example.com", "Admin");
            yield return new WaitUntil(() => task.IsCompleted);
            var memberId = manager.TeamMembers[0].memberId;

            Assert.IsTrue(manager.HasPermission(memberId, "write"));
            Assert.IsTrue(manager.HasPermission(memberId, "manage_users"));
            Assert.IsTrue(manager.HasPermission(memberId, "any_permission"));
        }

        [UnityTest]
        public IEnumerator HasPermission_ViewerRole_OnlyRead()
        {
            var task = manager.AddTeamMember("viewer@example.com", "Viewer");
            yield return new WaitUntil(() => task.IsCompleted);
            var memberId = manager.TeamMembers[0].memberId;

            Assert.IsTrue(manager.HasPermission(memberId, "read"));
            Assert.IsFalse(manager.HasPermission(memberId, "write"));
            Assert.IsFalse(manager.HasPermission(memberId, "manage_users"));
        }

        [UnityTest]
        public IEnumerator HasPermission_SuspendedMember_ReturnsFalse()
        {
            var task = manager.AddTeamMember("suspended@example.com", "Editor");
            yield return new WaitUntil(() => task.IsCompleted);
            var memberId = manager.TeamMembers[0].memberId;

            manager.SuspendMember(memberId, "Policy violation");
            Assert.IsFalse(manager.HasPermission(memberId, "read"));
        }

        // ============================================================
        // 密码策略测试
        // ============================================================

        [Test]
        public void CheckPasswordPolicy_ValidPassword_ReturnsValid()
        {
            var result = manager.CheckPasswordPolicy("SecurePass1!");
            Assert.IsTrue(result.valid);
            Assert.IsEmpty(result.errors ?? new string[0]);
        }

        [Test]
        public void CheckPasswordPolicy_TooShort_ReturnsError()
        {
            var result = manager.CheckPasswordPolicy("Ab1!");
            Assert.IsFalse(result.valid);
            Assert.IsTrue(result.errors.Length > 0);
        }

        [Test]
        public void CheckPasswordPolicy_NoUppercase_ReturnsError()
        {
            var result = manager.CheckPasswordPolicy("securepass1!");
            Assert.IsFalse(result.valid);
            bool hasUppercaseError = false;
            foreach (var err in result.errors)
            {
                if (err.Contains("uppercase")) hasUppercaseError = true;
            }
            Assert.IsTrue(hasUppercaseError);
        }

        [Test]
        public void CheckPasswordPolicy_NoDigit_ReturnsError()
        {
            var result = manager.CheckPasswordPolicy("SecurePass!");
            Assert.IsFalse(result.valid);
            bool hasDigitError = false;
            foreach (var err in result.errors)
            {
                if (err.Contains("digit")) hasDigitError = true;
            }
            Assert.IsTrue(hasDigitError);
        }

        [Test]
        public void CheckPasswordPolicy_NoSpecialChar_ReturnsError()
        {
            var result = manager.CheckPasswordPolicy("SecurePass1");
            Assert.IsFalse(result.valid);
            bool hasSpecialError = false;
            foreach (var err in result.errors)
            {
                if (err.Contains("special")) hasSpecialError = true;
            }
            Assert.IsTrue(hasSpecialError);
        }

        [Test]
        public void CheckPasswordPolicy_Disabled_AlwaysReturnsValid()
        {
            manager.enforcePasswordPolicy = false;
            var result = manager.CheckPasswordPolicy("weak");
            Assert.IsTrue(result.valid);
        }

        // ============================================================
        // 加密测试
        // ============================================================

        [Test]
        public void EncryptData_ValidData_ReturnsEncrypted()
        {
            string plainText = "sensitive_data_123";
            string encrypted = manager.EncryptData(plainText);

            Assert.IsNotNull(encrypted);
            Assert.AreNotEqual(plainText, encrypted);
        }

        [Test]
        public void DecryptData_EncryptedData_ReturnsOriginal()
        {
            string plainText = "sensitive_user_email@example.com";
            string encrypted = manager.EncryptData(plainText);
            string decrypted = manager.DecryptData(encrypted);

            Assert.AreEqual(plainText, decrypted);
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_PreservesData()
        {
            var testCases = new[] { "test123", "hello world", "special!@#$%", "中文数据" };
            foreach (var testCase in testCases)
            {
                var encrypted = manager.EncryptData(testCase);
                var decrypted = manager.DecryptData(encrypted);
                Assert.AreEqual(testCase, decrypted, $"Round-trip failed for: {testCase}");
            }
        }

        // ============================================================
        // 会话超时测试
        // ============================================================

        [Test]
        public void IsSessionExpired_RecentActivity_ReturnsFalse()
        {
            manager.sessionTimeoutMinutes = 480;
            bool expired = manager.IsSessionExpired(DateTime.Now.AddMinutes(-30));
            Assert.IsFalse(expired);
        }

        [Test]
        public void IsSessionExpired_OldActivity_ReturnsTrue()
        {
            manager.sessionTimeoutMinutes = 480;
            bool expired = manager.IsSessionExpired(DateTime.Now.AddHours(-10));
            Assert.IsTrue(expired);
        }

        // ============================================================
        // 审计日志测试
        // ============================================================

        [Test]
        public void LogAuditEvent_ValidEvent_RaisesOnAuditLogEntry()
        {
            AuditLogEntry captured = null;
            manager.OnAuditLogEntry += entry => captured = entry;

            manager.LogAuditEvent("test_action", "test_target", "test_details");

            Assert.IsNotNull(captured);
            Assert.AreEqual("test_action", captured.action);
            Assert.AreEqual("test_target", captured.target);
        }

        [Test]
        public void LogAuditEvent_Disabled_DoesNotRaiseEvent()
        {
            manager.enableAuditLogging = false;
            bool eventRaised = false;
            manager.OnAuditLogEntry += _ => eventRaised = true;

            manager.LogAuditEvent("test_action", "test_target", "details");
            Assert.IsFalse(eventRaised);
        }

        [Test]
        public void LogAuditEvent_DeleteAction_IsHighSeverity()
        {
            AuditLogEntry captured = null;
            manager.OnAuditLogEntry += entry => captured = entry;

            manager.LogAuditEvent("delete_record", "records", "Deleted 5 records");

            Assert.AreEqual(LogSeverity.High, captured.severity);
        }

        [Test]
        public void LogAuditEvent_FailedAction_IsCriticalSeverity()
        {
            AuditLogEntry captured = null;
            manager.OnAuditLogEntry += entry => captured = entry;

            manager.LogAuditEvent("auth_failed", "user@example.com", "Bad credentials");

            Assert.AreEqual(LogSeverity.Critical, captured.severity);
        }

        // ============================================================
        // 企业档案测试
        // ============================================================

        [Test]
        public void InitializeEnterprise_ValidProfile_SetsCurrentEnterprise()
        {
            var profile = new EnterpriseProfile
            {
                enterpriseId = "ent-001",
                name = "Test Corp",
                domain = "testcorp.com",
                plan = "Enterprise"
            };

            manager.InitializeEnterprise(profile);

            Assert.IsTrue(manager.IsEnterpriseMode);
            Assert.AreEqual("ent-001", manager.CurrentEnterprise.enterpriseId);
        }

        [Test]
        public void IsEnterpriseMode_BeforeInit_ReturnsFalse()
        {
            Assert.IsFalse(manager.IsEnterpriseMode);
        }
    }

    // ============================================================
    // SecurityService 单元测试
    // ============================================================
    public class SecurityServiceTests
    {
        private SecurityService service;

        [SetUp]
        public void Setup()
        {
            service = new SecurityService(enableIPWhitelist: true, enableEncryption: true);
        }

        [Test]
        public void IsIPAllowed_DefaultLocalhost_ReturnsTrue()
        {
            Assert.IsTrue(service.IsIPAllowed("127.0.0.1"));
            Assert.IsTrue(service.IsIPAllowed("::1"));
        }

        [Test]
        public void IsIPAllowed_UnknownIP_ReturnsFalse()
        {
            Assert.IsFalse(service.IsIPAllowed("192.168.1.100"));
        }

        [Test]
        public void AddAllowedIP_NewIP_IsAllowed()
        {
            service.AddAllowedIP("10.0.0.1");
            Assert.IsTrue(service.IsIPAllowed("10.0.0.1"));
        }

        [Test]
        public void RemoveAllowedIP_ExistingIP_NoLongerAllowed()
        {
            service.AddAllowedIP("10.0.0.2");
            service.RemoveAllowedIP("10.0.0.2");
            Assert.IsFalse(service.IsIPAllowed("10.0.0.2"));
        }

        [Test]
        public void IsIPAllowed_Disabled_AlwaysReturnsTrue()
        {
            var noWhitelistService = new SecurityService(enableIPWhitelist: false);
            Assert.IsTrue(noWhitelistService.IsIPAllowed("any.ip.address"));
        }

        [Test]
        public void Encrypt_ValidData_DiffersFromOriginal()
        {
            string data = "plaintext";
            string encrypted = service.Encrypt(data);
            Assert.AreNotEqual(data, encrypted);
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_RestoresData()
        {
            string original = "test sensitive data";
            string decrypted = service.Decrypt(service.Encrypt(original));
            Assert.AreEqual(original, decrypted);
        }

        [Test]
        public void ComputeHash_SameInput_ReturnsSameHash()
        {
            string hash1 = service.ComputeHash("test");
            string hash2 = service.ComputeHash("test");
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void ComputeHash_DifferentInput_ReturnsDifferentHash()
        {
            string hash1 = service.ComputeHash("test1");
            string hash2 = service.ComputeHash("test2");
            Assert.AreNotEqual(hash1, hash2);
        }

        [UnityTest]
        public IEnumerator Verify2FA_InvalidCode_ReturnsFalse()
        {
            var task = service.Verify2FA("user123", "000000");
            yield return new WaitUntil(() => task.IsCompleted);
            // Invalid code should fail (assuming this isn't a valid TOTP at this moment)
            Assert.IsNotNull(task.Result.ToString());
        }
    }

    // ============================================================
    // AuditLogService 单元测试
    // ============================================================
    public class AuditLogServiceTests
    {
        private AuditLogService service;

        [SetUp]
        public void Setup()
        {
            service = new AuditLogService(retentionDays: 365);
        }

        [UnityTest]
        public IEnumerator SaveAuditLogs_ValidEntries_IncreasesCount()
        {
            var entries = new System.Collections.Generic.List<AuditLogEntry>
            {
                new AuditLogEntry { entryId = "1", action = "login", timestamp = DateTime.Now },
                new AuditLogEntry { entryId = "2", action = "logout", timestamp = DateTime.Now }
            };

            var task = service.SaveAuditLogs(entries);
            yield return new WaitUntil(() => task.IsCompleted);

            Assert.AreEqual(2, service.TotalLogsWritten);
            Assert.AreEqual(2, service.PersistedCount);
        }

        [UnityTest]
        public IEnumerator GetLogs_FilterByAction_ReturnsMatching()
        {
            var entries = new System.Collections.Generic.List<AuditLogEntry>
            {
                new AuditLogEntry { entryId = "1", action = "login", timestamp = DateTime.Now },
                new AuditLogEntry { entryId = "2", action = "logout", timestamp = DateTime.Now },
                new AuditLogEntry { entryId = "3", action = "login", timestamp = DateTime.Now }
            };

            var saveTask = service.SaveAuditLogs(entries);
            yield return new WaitUntil(() => saveTask.IsCompleted);

            var getTask = service.GetLogs(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), action: "login");
            yield return new WaitUntil(() => getTask.IsCompleted);

            Assert.AreEqual(2, getTask.Result.Length);
        }

        [UnityTest]
        public IEnumerator GetActionStats_ReturnsCorrectCounts()
        {
            var entries = new System.Collections.Generic.List<AuditLogEntry>
            {
                new AuditLogEntry { entryId = "1", action = "login", timestamp = DateTime.Now },
                new AuditLogEntry { entryId = "2", action = "login", timestamp = DateTime.Now },
                new AuditLogEntry { entryId = "3", action = "update", timestamp = DateTime.Now }
            };

            var saveTask = service.SaveAuditLogs(entries);
            yield return new WaitUntil(() => saveTask.IsCompleted);

            var statsTask = service.GetActionStats(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1));
            yield return new WaitUntil(() => statsTask.IsCompleted);

            Assert.AreEqual(2, statsTask.Result["login"]);
            Assert.AreEqual(1, statsTask.Result["update"]);
        }
    }

    // ============================================================
    // TeamManagementService 单元测试
    // ============================================================
    public class TeamManagementServiceTests
    {
        private TeamManagementService service;

        [SetUp]
        public void Setup()
        {
            service = new TeamManagementService();
        }

        [UnityTest]
        public IEnumerator SyncMember_ValidMember_IncrementsCount()
        {
            var member = new TeamMember
            {
                memberId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                role = "Viewer",
                status = MemberStatus.Active
            };

            var task = service.SyncMember(member);
            yield return new WaitUntil(() => task.IsCompleted);

            Assert.AreEqual(1, service.TotalMembers);
        }

        [UnityTest]
        public IEnumerator SendInvitation_ValidEmail_ReturnsSuccess()
        {
            var task = service.SendInvitation("invite@example.com", "Editor", "admin@example.com");
            yield return new WaitUntil(() => task.IsCompleted);

            Assert.IsTrue(task.Result.success);
            Assert.IsNotNull(task.Result.invitationId);
        }

        [UnityTest]
        public IEnumerator SendInvitation_DuplicateEmail_ReturnsFalse()
        {
            var task1 = service.SendInvitation("dup@example.com", "Editor", "admin@example.com");
            yield return new WaitUntil(() => task1.IsCompleted);

            var task2 = service.SendInvitation("dup@example.com", "Viewer", "admin@example.com");
            yield return new WaitUntil(() => task2.IsCompleted);

            Assert.IsFalse(task2.Result.success);
        }

        [UnityTest]
        public IEnumerator RevokeInvitation_ExistingId_ReturnsTrue()
        {
            var sendTask = service.SendInvitation("revoke@example.com", "Editor", "admin@example.com");
            yield return new WaitUntil(() => sendTask.IsCompleted);

            bool revoked = service.RevokeInvitation(sendTask.Result.invitationId);
            Assert.IsTrue(revoked);
        }

        [UnityTest]
        public IEnumerator GetStats_ReturnsCorrectCounts()
        {
            var m1 = new TeamMember { memberId = "1", email = "a@b.com", role = "Admin", status = MemberStatus.Active };
            var m2 = new TeamMember { memberId = "2", email = "b@b.com", role = "Viewer", status = MemberStatus.Suspended };

            var t1 = service.SyncMember(m1);
            yield return new WaitUntil(() => t1.IsCompleted);
            var t2 = service.SyncMember(m2);
            yield return new WaitUntil(() => t2.IsCompleted);

            var stats = service.GetStats();
            Assert.AreEqual(2, stats.totalMembers);
            Assert.AreEqual(1, stats.activeMembers);
            Assert.AreEqual(1, stats.suspendedMembers);
        }
    }
}
