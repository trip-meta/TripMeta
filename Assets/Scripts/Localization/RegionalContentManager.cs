using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Localization
{
    /// <summary>
    /// 区域化内容管理器
    /// 管理亚太/欧美/新兴市场的本地化内容策略
    /// </summary>
    public class RegionalContentManager : MonoBehaviour
    {
        [Header("区域配置")]
        public RegionType defaultRegion = RegionType.AsiaPacific;
        public bool autoDetectRegion = true;
        public bool enableContentFiltering = true;
        public bool enableCulturalCompliance = true;

        [Header("内容策略")]
        public bool enableRegionalPricing = true;
        public bool enableLocalEvents = true;
        public bool enableRegionalPartners = true;
        public bool enableComplianceChecks = true;

        // 当前区域
        private RegionType currentRegion;
        private RegionalConfig currentConfig;

        // 区域内容库
        private Dictionary<RegionType, RegionalContentLibrary> contentLibraries = new Dictionary<RegionType, RegionalContentLibrary>();
        private Dictionary<string, LocalizedExperience> experienceCache = new Dictionary<string, LocalizedExperience>();

        // 服务
        private ComplianceService complianceService;
        private PricingService pricingService;
        private PartnerService partnerService;

        public static RegionalContentManager Instance { get; private set; }

        public RegionType CurrentRegion => currentRegion;
        public RegionalConfig CurrentConfig => currentConfig;
        public IReadOnlyDictionary<RegionType, RegionalContentLibrary> ContentLibraries => contentLibraries;

        // 事件
        public event Action<RegionType> OnRegionChanged;
        public event Action<LocalizedExperience> OnExperienceLocalized;
        public event Action<ComplianceReport> OnComplianceChecked;

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
            InitializeRegionalConfigs();
            InitializeContentLibraries();
            InitializeServices();

            if (autoDetectRegion)
            {
                AutoDetectRegion();
            }
            else
            {
                SetRegion(defaultRegion);
            }

            Debug.Log($"[RegionalContentManager] 初始化完成，支持 {contentLibraries.Count} 个区域");
        }

        /// <summary>
        /// 初始化区域配置
        /// </summary>
        private void InitializeRegionalConfigs()
        {
            currentConfig = new RegionalConfig
            {
                pricingTier = PricingTier.Standard,
                contentRating = ContentRating.General,
                allowUserGeneratedContent = true,
                requireAgeVerification = false,
                supportedPaymentMethods = new[] { "credit_card", "paypal", "crypto" },
                legalDocuments = new[] { "terms_of_service", "privacy_policy", "cookie_policy" }
            };
        }

        /// <summary>
        /// 初始化内容库
        /// </summary>
        private void InitializeContentLibraries()
        {
            // 亚太区域
            contentLibraries[RegionType.AsiaPacific] = new RegionalContentLibrary
            {
                region = RegionType.AsiaPacific,
                name = "Asia Pacific",
                cultures = new[] { "chinese", "japanese", "korean", "southeast_asian" },
                languages = new[] { LanguageCode.zh_CN, LanguageCode.zh_TW, LanguageCode.ja_JP, LanguageCode.ko_KR, LanguageCode.th_TH, LanguageCode.vi_VN },
                popularDestinations = new[] { "great_wall", " mount_fuji", "seoul_palace", "angkor_wat", "halong_bay" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Chinese New Year", startMonth = 1, endMonth = 2, type = EventType.Festival },
                    new RegionalEvent { name = "Cherry Blossom Season", startMonth = 3, endMonth = 4, type = EventType.Seasonal },
                    new RegionalEvent { name = "Diwali", startMonth = 10, endMonth = 11, type = EventType.Festival },
                    new RegionalEvent { name = "Golden Week", startMonth = 5, endMonth = 5, type = EventType.Holiday }
                },
                culturalConsiderations = new[] { "respect_hierarchy", "group_harmony", "save_face", "gift_giving_etiquette" },
                contentRestrictions = new[] { "no_gambling_promotion", "no_political_content" },
                preferredContentTypes = new[] { ContentType.Historical, ContentType.Cultural, ContentType.Food },
                pricingMultiplier = 0.8f
            };

            // 北美区域
            contentLibraries[RegionType.NorthAmerica] = new RegionalContentLibrary
            {
                region = RegionType.NorthAmerica,
                name = "North America",
                cultures = new[] { "american", "canadian" },
                languages = new[] { LanguageCode.en_US, LanguageCode.fr_FR },
                popularDestinations = new[] { "grand_canyon", "yellowstone", "statue_of_liberty", "niagara_falls", "yosemite" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Independence Day", startMonth = 7, endMonth = 7, type = EventType.Holiday },
                    new RegionalEvent { name = "Thanksgiving", startMonth = 11, endMonth = 11, type = EventType.Holiday },
                    new RegionalEvent { name = "Summer Vacation", startMonth = 6, endMonth = 8, type = EventType.Seasonal }
                },
                culturalConsiderations = new[] { "individualism", "time_efficiency", "casual_communication", "diversity_appreciation" },
                contentRestrictions = new[] { "gdpr_compliance", "coppa_compliance", "ada_accessibility" },
                preferredContentTypes = new[] { ContentType.Adventure, ContentType.Nature, ContentType.Entertainment },
                pricingMultiplier = 1.0f
            };

            // 欧洲区域
            contentLibraries[RegionType.Europe] = new RegionalContentLibrary
            {
                region = RegionType.Europe,
                name = "Europe",
                cultures = new[] { "western_european", "eastern_european", "nordic", "mediterranean" },
                languages = new[] { LanguageCode.en_US, LanguageCode.de_DE, LanguageCode.fr_FR, LanguageCode.es_ES, LanguageCode.it_IT },
                popularDestinations = new[] { "eiffel_tower", "colosseum", "sagrada_familia", "neuschwanstein", "louvre" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Christmas Markets", startMonth = 11, endMonth = 12, type = EventType.Festival },
                    new RegionalEvent { name = "Summer Holidays", startMonth = 7, endMonth = 8, type = EventType.Seasonal },
                    new RegionalEvent { name = "Easter", startMonth = 3, endMonth = 4, type = EventType.Holiday }
                },
                culturalConsiderations = new[] { "gdpr_privacy", "sustainability", "cultural_preservation", "work_life_balance" },
                contentRestrictions = new[] { "gdpr_strict", "right_to_be_forgotten", "accessibility_requirements" },
                preferredContentTypes = new[] { ContentType.Historical, ContentType.Art, ContentType.Cultural },
                pricingMultiplier = 1.1f
            };

            // 拉美区域
            contentLibraries[RegionType.LatinAmerica] = new RegionalContentLibrary
            {
                region = RegionType.LatinAmerica,
                name = "Latin America",
                cultures = new[] { "mexican", "brazilian", "argentine", "colombian" },
                languages = new[] { LanguageCode.es_ES, LanguageCode.pt_BR },
                popularDestinations = new[] { "machu_picchu", "christ_redeemer", "chichen_itza", "galapagos", "patagonia" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Carnival", startMonth = 2, endMonth = 3, type = EventType.Festival },
                    new RegionalEvent { name = "Day of the Dead", startMonth = 10, endMonth = 11, type = EventType.Festival },
                    new RegionalEvent { name = "Summer Season", startMonth = 12, endMonth = 3, type = EventType.Seasonal }
                },
                culturalConsiderations = new[] { "family_focused", "warm_communication", "flexible_timing", "cultural_pride" },
                contentRestrictions = new[] { "local_content_laws" },
                preferredContentTypes = new[] { ContentType.Cultural, ContentType.Food, ContentType.Music },
                pricingMultiplier = 0.6f
            };

            // 中东区域
            contentLibraries[RegionType.MiddleEast] = new RegionalContentLibrary
            {
                region = RegionType.MiddleEast,
                name = "Middle East",
                cultures = new[] { "arab", "persian", "turkish" },
                languages = new[] { LanguageCode.ar_SA, LanguageCode.fa_IR, LanguageCode.tr_TR, LanguageCode.en_US },
                popularDestinations = new[] { "pyramids", "petra", "burj_khalifa", "dome_of_rock", "hagia_sophia" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Ramadan", startMonth = 3, endMonth = 4, type = EventType.Religious },
                    new RegionalEvent { name = "Eid al-Fitr", startMonth = 4, endMonth = 5, type = EventType.Festival },
                    new RegionalEvent { name = "Eid al-Adha", startMonth = 6, endMonth = 7, type = EventType.Festival }
                },
                culturalConsiderations = new[] { "religious_sensitivity", "gender_considerations", "halal_compliance", "ramadan_observance" },
                contentRestrictions = new[] { "no_alcohol", "modest_dress_code", "prayer_time_consideration", "no_gambling" },
                preferredContentTypes = new[] { ContentType.Historical, ContentType.Religious, ContentType.Cultural },
                pricingMultiplier = 0.9f
            };

            // 非洲区域
            contentLibraries[RegionType.Africa] = new RegionalContentLibrary
            {
                region = RegionType.Africa,
                name = "Africa",
                cultures = new[] { "north_african", "west_african", "east_african", "southern_african" },
                languages = new[] { LanguageCode.ar_SA, LanguageCode.fr_FR, LanguageCode.en_US, LanguageCode.sw_KE },
                popularDestinations = new[] { "pyramids", "victoria_falls", "serengeti", "table_mountain", "marrakech" },
                seasonalEvents = new[]
                {
                    new RegionalEvent { name = "Great Migration", startMonth = 7, endMonth = 10, type = EventType.Seasonal },
                    new RegionalEvent { name = "Cairo Film Festival", startMonth = 11, endMonth = 12, type = EventType.Cultural }
                },
                culturalConsiderations = new[] { "community_focused", "storytelling_tradition", "respect_for_elders", "local_customs" },
                contentRestrictions = new[] { "sensitive_political_content" },
                preferredContentTypes = new[] { ContentType.Nature, ContentType.Adventure, ContentType.Cultural },
                pricingMultiplier = 0.5f
            };
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitializeServices()
        {
            complianceService = new ComplianceService();
            pricingService = new PricingService();
            partnerService = new PartnerService();
        }

        /// <summary>
        /// 自动检测区域
        /// </summary>
        private void AutoDetectRegion()
        {
            // 基于系统设置或IP检测
            #if UNITY_EDITOR
            currentRegion = RegionType.AsiaPacific;
            #else
            currentRegion = DetectRegionFromSystem();
            #endif

            SetRegion(currentRegion);
        }

        /// <summary>
        /// 从系统检测区域
        /// </summary>
        private RegionType DetectRegionFromSystem()
        {
            SystemLanguage lang = Application.systemLanguage;

            return lang switch
            {
                SystemLanguage.Chinese or SystemLanguage.Japanese or SystemLanguage.Korean or
                SystemLanguage.Thai or SystemLanguage.Vietnamese => RegionType.AsiaPacific,
                SystemLanguage.German or SystemLanguage.French or SystemLanguage.Italian or
                SystemLanguage.Spanish or SystemLanguage.Dutch => RegionType.Europe,
                SystemLanguage.Arabic or SystemLanguage.Persian => RegionType.MiddleEast,
                SystemLanguage.Portuguese => RegionType.LatinAmerica,
                _ => RegionType.NorthAmerica
            };
        }

        /// <summary>
        /// 设置区域
        /// </summary>
        public void SetRegion(RegionType region)
        {
            if (!contentLibraries.ContainsKey(region))
            {
                Debug.LogWarning($"[RegionalContentManager] 未配置的区域: {region}");
                region = defaultRegion;
            }

            currentRegion = region;
            var library = contentLibraries[region];

            // 更新配置
            UpdateRegionalConfig(region);

            Debug.Log($"[RegionalContentManager] 区域已切换至: {library.name}");
            OnRegionChanged?.Invoke(region);
        }

        /// <summary>
        /// 更新区域配置
        /// </summary>
        private void UpdateRegionalConfig(RegionType region)
        {
            var library = contentLibraries[region];

            currentConfig = new RegionalConfig
            {
                pricingTier = GetPricingTierForRegion(region),
                contentRating = GetContentRatingForRegion(region),
                allowUserGeneratedContent = region != RegionType.MiddleEast,
                requireAgeVerification = region == RegionType.Europe,
                supportedPaymentMethods = GetPaymentMethodsForRegion(region),
                legalDocuments = GetLegalDocumentsForRegion(region)
            };
        }

        /// <summary>
        /// 获取本地化体验
        /// </summary>
        public async Task<LocalizedExperience> GetLocalizedExperience(string experienceId)
        {
            if (experienceCache.TryGetValue(experienceId, out var cached))
            {
                return cached;
            }

            var experience = new LocalizedExperience
            {
                experienceId = experienceId,
                region = currentRegion,
                basePrice = await pricingService.GetBasePrice(experienceId),
                localPrice = CalculateRegionalPrice(experienceId),
                localCurrency = GetRegionalCurrency(),
                localizedDescription = await GetLocalizedDescription(experienceId),
                culturalNotes = GetCulturalNotes(experienceId),
                seasonalRelevance = GetSeasonalRelevance(experienceId),
                complianceStatus = enableComplianceChecks ? await CheckCompliance(experienceId) : ComplianceStatus.Approved,
                availablePartners = enableRegionalPartners ? await partnerService.GetRegionalPartners(currentRegion, experienceId) : new RegionalPartner[0]
            };

            experienceCache[experienceId] = experience;
            OnExperienceLocalized?.Invoke(experience);

            return experience;
        }

        /// <summary>
        /// 计算区域价格
        /// </summary>
        private decimal CalculateRegionalPrice(string experienceId)
        {
            var library = contentLibraries[currentRegion];
            var basePrice = pricingService.GetBasePriceSync(experienceId);

            return basePrice * (decimal)library.pricingMultiplier;
        }

        /// <summary>
        /// 检查合规性
        /// </summary>
        private async Task<ComplianceStatus> CheckCompliance(string experienceId)
        {
            var report = await complianceService.CheckExperience(experienceId, currentRegion, contentLibraries[currentRegion].contentRestrictions);
            OnComplianceChecked?.Invoke(report);

            return report.isCompliant ? ComplianceStatus.Approved : ComplianceStatus.Blocked;
        }

        /// <summary>
        /// 获取当前季节活动
        /// </summary>
        public RegionalEvent[] GetCurrentSeasonalEvents()
        {
            if (!enableLocalEvents) return new RegionalEvent[0];

            var library = contentLibraries[currentRegion];
            int currentMonth = DateTime.Now.Month;

            return library.seasonalEvents.Where(e =>
                currentMonth >= e.startMonth && currentMonth <= e.endMonth).ToArray();
        }

        /// <summary>
        /// 获取区域货币
        /// </summary>
        public string GetRegionalCurrency()
        {
            return currentRegion switch
            {
                RegionType.AsiaPacific => "USD",
                RegionType.NorthAmerica => "USD",
                RegionType.Europe => "EUR",
                RegionType.LatinAmerica => "USD",
                RegionType.MiddleEast => "USD",
                RegionType.Africa => "USD",
                _ => "USD"
            };
        }

        // 辅助方法
        private PricingTier GetPricingTierForRegion(RegionType region) => PricingTier.Standard;
        private ContentRating GetContentRatingForRegion(RegionType region) => ContentRating.General;
        private string[] GetPaymentMethodsForRegion(RegionType region) => new[] { "credit_card", "paypal" };
        private string[] GetLegalDocumentsForRegion(RegionType region) => new[] { "terms", "privacy" };
        private Task<string> GetLocalizedDescription(string id) => Task.FromResult($"Description for {id}");
        private string[] GetCulturalNotes(string id) => new string[0];
        private float GetSeasonalRelevance(string id) => 1.0f;

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 区域类型
    /// </summary>
    public enum RegionType
    {
        AsiaPacific,
        NorthAmerica,
        Europe,
        LatinAmerica,
        MiddleEast,
        Africa
    }

    /// <summary>
    /// 区域配置
    /// </summary>
    public class RegionalConfig
    {
        public PricingTier pricingTier;
        public ContentRating contentRating;
        public bool allowUserGeneratedContent;
        public bool requireAgeVerification;
        public string[] supportedPaymentMethods;
        public string[] legalDocuments;
    }

    /// <summary>
    /// 区域内容库
    /// </summary>
    public class RegionalContentLibrary
    {
        public RegionType region;
        public string name;
        public string[] cultures;
        public LanguageCode[] languages;
        public string[] popularDestinations;
        public RegionalEvent[] seasonalEvents;
        public string[] culturalConsiderations;
        public string[] contentRestrictions;
        public ContentType[] preferredContentTypes;
        public float pricingMultiplier;
    }

    /// <summary>
    /// 区域活动
    /// </summary>
    public class RegionalEvent
    {
        public string name;
        public int startMonth;
        public int endMonth;
        public EventType type;
    }

    /// <summary>
    /// 活动类型
    /// </summary>
    public enum EventType
    {
        Festival,
        Holiday,
        Seasonal,
        Cultural,
        Religious
    }

    /// <summary>
    /// 内容类型
    /// </summary>
    public enum ContentType
    {
        Historical,
        Cultural,
        Food,
        Nature,
        Adventure,
        Entertainment,
        Art,
        Music,
        Religious
    }

    /// <summary>
    /// 本地化体验
    /// </summary>
    public class LocalizedExperience
    {
        public string experienceId;
        public RegionType region;
        public decimal basePrice;
        public decimal localPrice;
        public string localCurrency;
        public string localizedDescription;
        public string[] culturalNotes;
        public float seasonalRelevance;
        public ComplianceStatus complianceStatus;
        public RegionalPartner[] availablePartners;
    }

    /// <summary>
    /// 区域合作伙伴
    /// </summary>
    public class RegionalPartner
    {
        public string partnerId;
        public string name;
        public PartnerType type;
        public string[] supportedExperiences;
        public float commissionRate;
    }

    /// <summary>
    /// 合作伙伴类型
    /// </summary>
    public enum PartnerType
    {
        TourOperator,
        Hotel,
        Restaurant,
        Transport,
        Guide
    }

    /// <summary>
    /// 定价等级
    /// </summary>
    public enum PricingTier
    {
        Economy,
        Standard,
        Premium
    }

    /// <summary>
    /// 内容分级
    /// </summary>
    public enum ContentRating
    {
        General,
        Teen,
        Mature
    }

    /// <summary>
    /// 合规状态
    /// </summary>
    public enum ComplianceStatus
    {
        Approved,
        Pending,
        Blocked,
        RequiresModification
    }

    /// <summary>
    /// 合规报告
    /// </summary>
    public class ComplianceReport
    {
        public string experienceId;
        public bool isCompliant;
        public string[] violations;
        public string[] recommendations;
        public DateTime checkedAt;
    }

    #endregion
}
