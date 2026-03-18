using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// NFT 服务
    /// 处理 NFT 的铸造、查询、转移等功能
    /// </summary>
    public class NFTService
    {
        private Web3Manager web3Manager;

        public NFTService(Web3Manager manager)
        {
            web3Manager = manager;
        }

        /// <summary>
        /// 铸造 NFT
        /// </summary>
        public async Task<NFTItem> MintNFT(string toAddress, string metadataUri, TravelExperienceMetadata metadata)
        {
            try
            {
                // 这里应该调用智能合约的 mint 函数
                // 简化实现：创建 NFTItem 并返回
                await Task.Delay(2000);

                var nft = new NFTItem
                {
                    tokenId = UnityEngine.Random.Range(1, 1000000).ToString(),
                    contractAddress = web3Manager.travelNFTContractAddress,
                    name = metadata.name,
                    description = metadata.description,
                    imageUri = metadata.image,
                    metadataUri = metadataUri,
                    nftType = NFTType.TravelExperience,
                    attributes = new NFTAttributes
                    {
                        location = metadata.location,
                        landmarkId = metadata.landmarkId,
                        visitDate = metadata.visitDate,
                        rarity = CalculateRarity(metadata),
                        tags = new[] { "travel", "experience", metadata.location.ToLower().Replace(" ", "-") }
                    },
                    ownerAddress = toAddress,
                    mintedAt = DateTime.Now,
                    isListed = false
                };

                Debug.Log($"[NFTService] NFT 铸造成功: {nft.tokenId}");
                return nft;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NFTService] NFT 铸造失败: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取用户的 NFT 列表
        /// </summary>
        public async Task<List<NFTItem>> GetUserNFTs(string ownerAddress)
        {
            // 这里应该查询区块链获取用户的 NFT
            // 简化实现：返回模拟数据
            await Task.Delay(500);

            var nfts = new List<NFTItem>();

            // 模拟一些 NFT
            for (int i = 0; i < UnityEngine.Random.Range(0, 5); i++)
            {
                nfts.Add(new NFTItem
                {
                    tokenId = UnityEngine.Random.Range(1, 1000000).ToString(),
                    name = $"Travel Experience #{i + 1}",
                    description = "A unique travel experience NFT",
                    nftType = NFTType.TravelExperience,
                    ownerAddress = ownerAddress,
                    attributes = new NFTAttributes
                    {
                        rarity = UnityEngine.Random.Range(1, 6),
                        location = "Paris, France"
                    }
                });
            }

            return nfts;
        }

        /// <summary>
        /// 转移 NFT
        /// </summary>
        public async Task<bool> TransferNFT(string from, string to, string tokenId)
        {
            try
            {
                // 调用智能合约的 transferFrom 函数
                await Task.Delay(1500);
                Debug.Log($"[NFTService] NFT {tokenId} 已从 {from} 转移到 {to}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NFTService] NFT 转移失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取 NFT 详情
        /// </summary>
        public async Task<NFTItem> GetNFTDetails(string tokenId)
        {
            await Task.Delay(300);

            return new NFTItem
            {
                tokenId = tokenId,
                name = $"Travel Experience #{tokenId}",
                description = "A unique travel experience",
                nftType = NFTType.TravelExperience,
                attributes = new NFTAttributes
                {
                    rarity = 3,
                    location = "Tokyo, Japan"
                }
            };
        }

        /// <summary>
        /// 计算 NFT 稀有度
        /// </summary>
        private int CalculateRarity(TravelExperienceMetadata metadata)
        {
            // 基于元数据计算稀有度
            int rarity = 1;

            // 评分越高，稀有度越高
            if (metadata.rating >= 4.5f) rarity += 2;
            else if (metadata.rating >= 4.0f) rarity += 1;

            // 时长越长，稀有度越高
            if (metadata.duration >= 120) rarity += 1;

            // 有视频内容增加稀有度
            if (metadata.videos != null && metadata.videos.Length > 0) rarity += 1;

            return Mathf.Clamp(rarity, 1, 5);
        }

        /// <summary>
        /// 批量铸造 NFT
        /// </summary>
        public async Task<List<NFTItem>> BatchMintNFT(List<MintRequest> requests)
        {
            var results = new List<NFTItem>();

            foreach (var request in requests)
            {
                var nft = await MintNFT(request.to, request.metadataUri, request.metadata);
                if (nft != null)
                {
                    results.Add(nft);
                }
            }

            return results;
        }
    }

    /// <summary>
    /// 铸造请求
    /// </summary>
    public class MintRequest
    {
        public string to;
        public string metadataUri;
        public TravelExperienceMetadata metadata;
    }
}
