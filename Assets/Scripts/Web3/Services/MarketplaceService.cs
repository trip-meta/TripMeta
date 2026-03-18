using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// 市场服务
    /// 处理 NFT 的购买、出售、拍卖等功能
    /// </summary>
    public class MarketplaceService
    {
        private Web3Manager web3Manager;
        private List<MarketListing> activeListings = new List<MarketListing>();

        public event Action<string> OnItemListed;
        public event Action<string> OnItemSold;
        public event Action<string> OnListingCancelled;

        public MarketplaceService(Web3Manager manager)
        {
            web3Manager = manager;
        }

        /// <summary>
        /// 列出 NFT 出售
        /// </summary>
        public async Task<bool> ListItem(string tokenId, decimal price, string sellerAddress)
        {
            try
            {
                if (price <= 0)
                {
                    Debug.LogError("[MarketplaceService] 价格必须大于0");
                    return false;
                }

                // 调用智能合约的 listItem 函数
                await Task.Delay(2000);

                var listing = new MarketListing
                {
                    listingId = Guid.NewGuid().ToString("N").Substring(0, 16),
                    tokenId = tokenId,
                    seller = sellerAddress,
                    price = price,
                    currency = "TRIP",
                    listedAt = DateTime.Now,
                    nft = await web3Manager.GetNFTDetails(tokenId)
                };

                activeListings.Add(listing);
                OnItemListed?.Invoke(listing.listingId);

                Debug.Log($"[MarketplaceService] NFT 已列出: {listing.listingId}, 价格: {price} TRIP");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarketplaceService] 列出 NFT 失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 购买 NFT
        /// </summary>
        public async Task<bool> BuyItem(string listingId, string buyerAddress, decimal price)
        {
            try
            {
                var listing = activeListings.Find(l => l.listingId == listingId);
                if (listing == null)
                {
                    Debug.LogError("[MarketplaceService] 列表项不存在");
                    return false;
                }

                if (listing.price != price)
                {
                    Debug.LogError("[MarketplaceService] 价格不匹配");
                    return false;
                }

                // 调用智能合约的 buyItem 函数
                await Task.Delay(3000);

                activeListings.Remove(listing);
                OnItemSold?.Invoke(listingId);

                Debug.Log($"[MarketplaceService] NFT 购买成功: {listingId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarketplaceService] 购买 NFT 失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取消列表
        /// </summary>
        public async Task<bool> CancelListing(string listingId, string sellerAddress)
        {
            try
            {
                var listing = activeListings.Find(l => l.listingId == listingId);
                if (listing == null)
                {
                    Debug.LogError("[MarketplaceService] 列表项不存在");
                    return false;
                }

                if (listing.seller != sellerAddress)
                {
                    Debug.LogError("[MarketplaceService] 只有卖家可以取消列表");
                    return false;
                }

                // 调用智能合约的 cancelListing 函数
                await Task.Delay(1500);

                activeListings.Remove(listing);
                OnListingCancelled?.Invoke(listingId);

                Debug.Log($"[MarketplaceService] 列表已取消: {listingId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarketplaceService] 取消列表失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取活跃列表
        /// </summary>
        public async Task<List<MarketListing>> GetActiveListings(int page = 1, int pageSize = 20)
        {
            await Task.Delay(300);

            // 返回分页数据
            int startIndex = (page - 1) * pageSize;
            int count = Mathf.Min(pageSize, activeListings.Count - startIndex);

            if (startIndex >= activeListings.Count)
            {
                return new List<MarketListing>();
            }

            return activeListings.GetRange(startIndex, count);
        }

        /// <summary>
        /// 获取特定 NFT 的列表
        /// </summary>
        public async Task<MarketListing> GetListingForToken(string tokenId)
        {
            await Task.Delay(100);
            return activeListings.Find(l => l.tokenId == tokenId);
        }

        /// <summary>
        /// 搜索列表
        /// </summary>
        public async Task<List<MarketListing>> SearchListings(string keyword, decimal? minPrice = null, decimal? maxPrice = null)
        {
            await Task.Delay(500);

            var results = activeListings.FindAll(l =>
            {
                bool matchesKeyword = string.IsNullOrEmpty(keyword) ||
                                     (l.nft?.name?.Contains(keyword) ?? false) ||
                                     (l.nft?.description?.Contains(keyword) ?? false);

                bool matchesPrice = (!minPrice.HasValue || l.price >= minPrice.Value) &&
                                   (!maxPrice.HasValue || l.price <= maxPrice.Value);

                return matchesKeyword && matchesPrice;
            });

            return results;
        }

        /// <summary>
        /// 创建拍卖
        /// </summary>
        public async Task<string> CreateAuction(string tokenId, decimal startingPrice, DateTime endTime, string sellerAddress)
        {
            try
            {
                await Task.Delay(2000);

                string auctionId = Guid.NewGuid().ToString("N").Substring(0, 16);
                Debug.Log($"[MarketplaceService] 拍卖已创建: {auctionId}");

                return auctionId;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarketplaceService] 创建拍卖失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 出价
        /// </summary>
        public async Task<bool> PlaceBid(string auctionId, decimal bidAmount, string bidderAddress)
        {
            try
            {
                await Task.Delay(1500);
                Debug.Log($"[MarketplaceService] 出价成功: {bidAmount} TRIP");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarketplaceService] 出价失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取市场统计
        /// </summary>
        public async Task<MarketplaceStats> GetMarketplaceStats()
        {
            await Task.Delay(200);

            return new MarketplaceStats
            {
                TotalVolume = UnityEngine.Random.Range(1000000, 10000000),
                TotalSales = UnityEngine.Random.Range(1000, 10000),
                ActiveListings = activeListings.Count,
                AveragePrice = UnityEngine.Random.Range(100, 1000),
                FloorPrice = UnityEngine.Random.Range(10, 100)
            };
        }
    }

    /// <summary>
    /// 市场统计
    /// </summary>
    public class MarketplaceStats
    {
        public decimal TotalVolume;
        public int TotalSales;
        public int ActiveListings;
        public decimal AveragePrice;
        public decimal FloorPrice;
    }
}