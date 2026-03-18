using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace TripMeta.Web3
{
    /// <summary>
    /// Web3 管理器
    /// 管理区块链连接、钱包交互、NFT 和虚拟经济系统
    /// </summary>
    public class Web3Manager : MonoBehaviour
    {
        [Header("区块链配置")]
        public BlockchainNetwork defaultNetwork = BlockchainNetwork.Ethereum;
        public string customRpcUrl = "";
        public int chainId = 1;

        [Header("合约地址")]
        public string travelNFTContractAddress = "";
        public string tokenContractAddress = "";
        public string marketplaceContractAddress = "";

        [Header("IPFS 配置")]
        public string ipfsGateway = "https://ipfs.io/ipfs/";
        public string pinataApiKey = "";
        public string pinataSecretKey = "";

        [Header("功能开关")]
        public bool enableNFT = true;
        public bool enableToken = true;
        public bool enableMarketplace = true;
        public bool enableStaking = true;

        // 当前连接的钱包
        private IWalletAdapter currentWallet;
        private WalletConnectionStatus connectionStatus = WalletConnectionStatus.Disconnected;

        // 用户数据
        private UserWalletData userData = new UserWalletData();

        // 服务
        private NFTService nftService;
        private TokenService tokenService;
        private MarketplaceService marketplaceService;
        private StakingService stakingService;

        public static Web3Manager Instance { get; private set; }

        public bool IsConnected => connectionStatus == WalletConnectionStatus.Connected;
        public string ConnectedAddress => currentWallet?.Address;
        public WalletConnectionStatus ConnectionStatus => connectionStatus;
        public UserWalletData UserData => userData;

        // 事件
        public event Action<string> OnWalletConnected;
        public event Action OnWalletDisconnected;
        public event Action<string> OnTransactionSubmitted;
        public event Action<string, bool> OnTransactionConfirmed;
        public event Action<NFTItem> OnNFTAcquired;
        public event Action<decimal> OnTokenBalanceChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServices();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化 Web3 服务
        /// </summary>
        private void InitializeServices()
        {
            if (enableNFT)
                nftService = new NFTService(this);
            if (enableToken)
                tokenService = new TokenService(this);
            if (enableMarketplace)
                marketplaceService = new MarketplaceService(this);
            if (enableStaking)
                stakingService = new StakingService(this);

            Debug.Log("[Web3Manager] Web3 服务初始化完成");
        }

        #region 钱包连接

        /// <summary>
        /// 连接钱包
        /// </summary>
        public async Task<bool> ConnectWallet(WalletType walletType)
        {
            try
            {
                connectionStatus = WalletConnectionStatus.Connecting;

                // 创建钱包适配器
                currentWallet = CreateWalletAdapter(walletType);

                if (currentWallet == null)
                {
                    Debug.LogError($"[Web3Manager] 不支持的钱包类型: {walletType}");
                    connectionStatus = WalletConnectionStatus.Error;
                    return false;
                }

                // 连接钱包
                bool success = await currentWallet.Connect();

                if (success)
                {
                    connectionStatus = WalletConnectionStatus.Connected;
                    await LoadUserData();
                    OnWalletConnected?.Invoke(currentWallet.Address);
                    Debug.Log($"[Web3Manager] 钱包连接成功: {currentWallet.Address}");
                    return true;
                }
                else
                {
                    connectionStatus = WalletConnectionStatus.Error;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Web3Manager] 连接钱包失败: {e.Message}");
                connectionStatus = WalletConnectionStatus.Error;
                return false;
            }
        }

        /// <summary>
        /// 断开钱包连接
        /// </summary>
        public async Task DisconnectWallet()
        {
            if (currentWallet != null)
            {
                await currentWallet.Disconnect();
                currentWallet = null;
            }

            connectionStatus = WalletConnectionStatus.Disconnected;
            userData = new UserWalletData();
            OnWalletDisconnected?.Invoke();
            Debug.Log("[Web3Manager] 钱包已断开连接");
        }

        /// <summary>
        /// 创建钱包适配器
        /// </summary>
        private IWalletAdapter CreateWalletAdapter(WalletType type)
        {
            switch (type)
            {
                case WalletType.MetaMask:
                    return new MetaMaskAdapter();
                case WalletType.WalletConnect:
                    return new WalletConnectAdapter();
                case WalletType.CoinbaseWallet:
                    return new CoinbaseWalletAdapter();
                case WalletType.Phantom:
                    return new PhantomAdapter();
                default:
                    return null;
            }
        }

        /// <summary>
        /// 加载用户数据
        /// </summary>
        private async Task LoadUserData()
        {
            if (currentWallet == null) return;

            userData.address = currentWallet.Address;

            // 加载 NFT 收藏
            if (enableNFT && nftService != null)
            {
                userData.nftCollection = await nftService.GetUserNFTs(currentWallet.Address);
            }

            // 加载代币余额
            if (enableToken && tokenService != null)
            {
                userData.tokenBalance = await tokenService.GetBalance(currentWallet.Address);
                userData.stakedAmount = await stakingService?.GetStakedAmount(currentWallet.Address) ?? 0;
            }

            // 加载交易历史
            userData.transactionHistory = await LoadTransactionHistory();
        }

        /// <summary>
        /// 加载交易历史
        /// </summary>
        private async Task<List<TransactionRecord>> LoadTransactionHistory()
        {
            // 这里应该查询区块链或服务器 API
            // 简化实现：返回本地缓存
            return new List<TransactionRecord>();
        }

        #endregion

        #region NFT 功能

        /// <summary>
        /// 铸造旅游体验 NFT
        /// </summary>
        public async Task<NFTItem> MintTravelNFT(TravelExperienceMetadata metadata)
        {
            if (!IsConnected)
            {
                Debug.LogError("[Web3Manager] 请先连接钱包");
                return null;
            }

            if (!enableNFT || nftService == null)
            {
                Debug.LogError("[Web3Manager] NFT 功能未启用");
                return null;
            }

            try
            {
                // 上传元数据到 IPFS
                string metadataUri = await UploadToIPFS(metadata);

                // 铸造 NFT
                var nft = await nftService.MintNFT(currentWallet.Address, metadataUri, metadata);

                // 更新用户数据
                userData.nftCollection.Add(nft);
                OnNFTAcquired?.Invoke(nft);

                Debug.Log($"[Web3Manager] NFT 铸造成功: {nft.tokenId}");
                return nft;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Web3Manager] NFT 铸造失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 转移 NFT
        /// </summary>
        public async Task<bool> TransferNFT(string tokenId, string toAddress)
        {
            if (!IsConnected || !enableNFT) return false;

            return await nftService.TransferNFT(currentWallet.Address, toAddress, tokenId);
        }

        /// <summary>
        /// 列出 NFT 出售
        /// </summary>
        public async Task<bool> ListNFTForSale(string tokenId, decimal price)
        {
            if (!IsConnected || !enableMarketplace) return false;

            return await marketplaceService.ListItem(tokenId, price, currentWallet.Address);
        }

        /// <summary>
        /// 购买 NFT
        /// </summary>
        public async Task<bool> BuyNFT(string listingId, decimal price)
        {
            if (!IsConnected || !enableMarketplace) return false;

            var result = await marketplaceService.BuyItem(listingId, currentWallet.Address, price);
            if (result)
            {
                await LoadUserData(); // 刷新用户数据
            }
            return result;
        }

        #endregion

        #region 代币功能

        /// <summary>
        /// 获取代币余额
        /// </summary>
        public async Task<decimal> GetTokenBalance()
        {
            if (!IsConnected || !enableToken) return 0;

            return await tokenService.GetBalance(currentWallet.Address);
        }

        /// <summary>
        /// 转账代币
        /// </summary>
        public async Task<bool> TransferTokens(string toAddress, decimal amount)
        {
            if (!IsConnected || !enableToken) return false;

            var result = await tokenService.Transfer(currentWallet.Address, toAddress, amount);
            if (result)
            {
                userData.tokenBalance = await GetTokenBalance();
                OnTokenBalanceChanged?.Invoke(userData.tokenBalance);
            }
            return result;
        }

        /// <summary>
        /// 质押代币
        /// </summary>
        public async Task<bool> StakeTokens(decimal amount)
        {
            if (!IsConnected || !enableStaking) return false;

            var result = await stakingService.Stake(currentWallet.Address, amount);
            if (result)
            {
                userData.stakedAmount = await stakingService.GetStakedAmount(currentWallet.Address);
                userData.tokenBalance = await GetTokenBalance();
                OnTokenBalanceChanged?.Invoke(userData.tokenBalance);
            }
            return result;
        }

        /// <summary>
        /// 解除质押
        /// </summary>
        public async Task<bool> UnstakeTokens(decimal amount)
        {
            if (!IsConnected || !enableStaking) return false;

            var result = await stakingService.Unstake(currentWallet.Address, amount);
            if (result)
            {
                userData.stakedAmount = await stakingService.GetStakedAmount(currentWallet.Address);
                userData.tokenBalance = await GetTokenBalance();
                OnTokenBalanceChanged?.Invoke(userData.tokenBalance);
            }
            return result;
        }

        /// <summary>
        /// 领取质押奖励
        /// </summary>
        public async Task<decimal> ClaimStakingRewards()
        {
            if (!IsConnected || !enableStaking) return 0;

            var rewards = await stakingService.ClaimRewards(currentWallet.Address);
            if (rewards > 0)
            {
                userData.tokenBalance = await GetTokenBalance();
                OnTokenBalanceChanged?.Invoke(userData.tokenBalance);
            }
            return rewards;
        }

        #endregion

        #region IPFS 功能

        /// <summary>
        /// 上传到 IPFS
        /// </summary>
        public async Task<string> UploadToIPFS(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return await UploadToIPFS(json);
        }

        /// <summary>
        /// 上传 JSON 到 IPFS
        /// </summary>
        public async Task<string> UploadToIPFS(string json)
        {
            // 这里应该使用 Pinata 或其他 IPFS 服务
            // 简化实现：返回模拟的 IPFS hash
            await Task.Delay(1000);
            var hash = "Qm" + Guid.NewGuid().ToString("N").Substring(0, 44);
            return $"{ipfsGateway}{hash}";
        }

        /// <summary>
        /// 上传图片到 IPFS
        /// </summary>
        public async Task<string> UploadImageToIPFS(Texture2D texture)
        {
            var bytes = texture.EncodeToPNG();
            // 上传到 IPFS
            await Task.Delay(1000);
            var hash = "Qm" + Guid.NewGuid().ToString("N").Substring(0, 44);
            return $"{ipfsGateway}{hash}";
        }

        #endregion

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
    /// 用户钱包数据
    /// </summary>
    [Serializable]
    public class UserWalletData
    {
        public string address;
        public decimal tokenBalance;
        public decimal stakedAmount;
        public decimal pendingRewards;
        public List<NFTItem> nftCollection = new List<NFTItem>();
        public List<TransactionRecord> transactionHistory = new List<TransactionRecord>();
        public int reputationScore;
        public DateTime joinedAt;
    }

    /// <summary>
    /// NFT 项目
    /// </summary>
    [Serializable]
    public class NFTItem
    {
        public string tokenId;
        public string contractAddress;
        public string name;
        public string description;
        public string imageUri;
        public string metadataUri;
        public NFTType nftType;
        public NFTAttributes attributes;
        public string ownerAddress;
        public DateTime mintedAt;
        public decimal? listingPrice;
        public bool isListed;
    }

    /// <summary>
    /// NFT 类型
    /// </summary>
    public enum NFTType
    {
        TravelExperience,
        LandmarkTicket,
        Achievement,
        Collectible,
        VirtualRealEstate
    }

    /// <summary>
    /// NFT 属性
    /// </summary>
    [Serializable]
    public class NFTAttributes
    {
        public string location;
        public string landmarkId;
        public DateTime visitDate;
        public int rarity; // 1-5
        public string[] tags;
        public Dictionary<string, string> customProperties;
    }

    /// <summary>
    /// 旅游体验元数据
    /// </summary>
    [Serializable]
    public class TravelExperienceMetadata
    {
        public string name;
        public string description;
        public string image;
        public string location;
        public string landmarkId;
        public DateTime visitDate;
        public string[] photos;
        public string[] videos;
        public string guideId;
        public float duration;
        public float rating;
    }

    /// <summary>
    /// 交易记录
    /// </summary>
    [Serializable]
    public class TransactionRecord
    {
        public string txHash;
        public TransactionType type;
        public string fromAddress;
        public string toAddress;
        public decimal amount;
        public string tokenSymbol;
        public string tokenId;
        public DateTime timestamp;
        public TransactionStatus status;
        public decimal gasFee;
    }

    /// <summary>
    /// 交易类型
    /// </summary>
    public enum TransactionType
    {
        Mint,
        Transfer,
        Sale,
        Purchase,
        Stake,
        Unstake,
        ClaimReward,
        Approval
    }

    /// <summary>
    /// 交易状态
    /// </summary>
    public enum TransactionStatus
    {
        Pending,
        Confirmed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// 钱包类型
    /// </summary>
    public enum WalletType
    {
        MetaMask,
        WalletConnect,
        CoinbaseWallet,
        Phantom
    }

    /// <summary>
    /// 钱包连接状态
    /// </summary>
    public enum WalletConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    /// <summary>
    /// 区块链网络
    /// </summary>
    public enum BlockchainNetwork
    {
        Ethereum,
        Polygon,
        BinanceSmartChain,
        Avalanche,
        Solana,
        Custom
    }

    /// <summary>
    /// 市场列表项
    /// </summary>
    [Serializable]
    public class MarketListing
    {
        public string listingId;
        public string tokenId;
        public string seller;
        public decimal price;
        public string currency;
        public DateTime listedAt;
        public NFTItem nft;
    }

    #endregion
}
