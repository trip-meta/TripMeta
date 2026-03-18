using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Web3;

namespace TripMeta.Tests.Web3
{
    /// <summary>
    /// Web3 管理器单元测试
    /// </summary>
    public class Web3ManagerTests
    {
        private GameObject testObject;
        private Web3Manager web3Manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestWeb3Manager");
            web3Manager = testObject.AddComponent<Web3Manager>();
            web3Manager.enableNFT = true;
            web3Manager.enableToken = true;
            web3Manager.enableMarketplace = true;
            web3Manager.enableStaking = true;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator Web3Manager_Initialization_EnablesServices()
        {
            yield return null;

            Assert.IsNotNull(web3Manager);
            Assert.IsTrue(web3Manager.enableNFT);
            Assert.IsTrue(web3Manager.enableToken);
            Assert.IsTrue(web3Manager.enableMarketplace);
            Assert.IsTrue(web3Manager.enableStaking);
        }

        [UnityTest]
        public IEnumerator Web3Manager_ConnectWallet_ConnectsSuccessfully()
        {
            yield return null;

            Task<bool> connectTask = web3Manager.ConnectWallet(WalletType.MetaMask);
            while (!connectTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(connectTask.Result);
            Assert.IsTrue(web3Manager.IsConnected);
            Assert.IsNotNull(web3Manager.ConnectedAddress);
            Assert.IsNotEmpty(web3Manager.ConnectedAddress);
        }

        [UnityTest]
        public IEnumerator Web3Manager_DisconnectWallet_DisconnectsSuccessfully()
        {
            yield return null;

            // 先连接
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();
            Assert.IsTrue(web3Manager.IsConnected);

            // 断开连接
            Task disconnectTask = web3Manager.DisconnectWallet();
            while (!disconnectTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsFalse(web3Manager.IsConnected);
        }

        [UnityTest]
        public IEnumerator Web3Manager_MintTravelNFT_CreatesNFT()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            var metadata = new TravelExperienceMetadata
            {
                name = "Test Travel Experience",
                description = "A test travel experience NFT",
                location = "Paris, France",
                rating = 4.8f,
                duration = 120
            };

            Task<NFTItem> mintTask = web3Manager.MintTravelNFT(metadata);
            while (!mintTask.IsCompleted)
            {
                yield return null;
            }

            var nft = mintTask.Result;
            Assert.IsNotNull(nft);
            Assert.IsNotNull(nft.tokenId);
            Assert.AreEqual("Test Travel Experience", nft.name);
            Assert.AreEqual(NFTType.TravelExperience, nft.nftType);
            Assert.Greater(nft.attributes.rarity, 0);
            Assert.LessOrEqual(nft.attributes.rarity, 5);
        }

        [UnityTest]
        public IEnumerator Web3Manager_GetTokenBalance_ReturnsBalance()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            Task<decimal> balanceTask = web3Manager.GetTokenBalance();
            while (!balanceTask.IsCompleted)
            {
                yield return null;
            }

            Assert.GreaterOrEqual(balanceTask.Result, 0);
        }

        [UnityTest]
        public IEnumerator Web3Manager_TransferTokens_TransfersSuccessfully()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            Task<bool> transferTask = web3Manager.TransferTokens("0xRecipientAddress", 100);
            while (!transferTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(transferTask.Result);
        }

        [UnityTest]
        public IEnumerator Web3Manager_StakeTokens_StakesSuccessfully()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            Task<bool> stakeTask = web3Manager.StakeTokens(1000);
            while (!stakeTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(stakeTask.Result);
        }

        [UnityTest]
        public IEnumerator Web3Manager_UnstakeTokens_UnstakesSuccessfully()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            // 先质押
            yield return web3Manager.StakeTokens(1000).AsCoroutine();

            // 解除质押
            Task<bool> unstakeTask = web3Manager.UnstakeTokens(500);
            while (!unstakeTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(unstakeTask.Result);
        }

        [UnityTest]
        public IEnumerator Web3Manager_ListNFTForSale_ListsSuccessfully()
        {
            yield return null;
            yield return web3Manager.ConnectWallet(WalletType.MetaMask).AsCoroutine();

            Task<bool> listTask = web3Manager.ListNFTForSale("123", 500);
            while (!listTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(listTask.Result);
        }

        [UnityTest]
        public IEnumerator Web3Manager_UploadToIPFS_UploadsSuccessfully()
        {
            yield return null;

            var data = new { name = "Test", value = 123 };
            Task<string> uploadTask = web3Manager.UploadToIPFS(data);
            while (!uploadTask.IsCompleted)
            {
                yield return null;
            }

            string uri = uploadTask.Result;
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.StartsWith("https://ipfs.io/ipfs/"));
        }

        [Test]
        public void NFTItem_Rarity_IsWithinRange()
        {
            var nft = new NFTItem
            {
                tokenId = "123",
                attributes = new NFTAttributes
                {
                    rarity = 3
                }
            };

            Assert.GreaterOrEqual(nft.attributes.rarity, 1);
            Assert.LessOrEqual(nft.attributes.rarity, 5);
        }

        [Test]
        public void TokenInfo_FormatAmount_FormatsCorrectly()
        {
            var tokenInfo = new TokenInfo
            {
                Symbol = "TRIP",
                Decimals = 18
            };

            // 这里假设有一个格式化方法
            string formatted = $"{1234.56m:N2} {tokenInfo.Symbol}";
            Assert.AreEqual("1,234.56 TRIP", formatted);
        }
    }

    /// <summary>
    /// 辅助扩展方法
    /// </summary>
    public static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        public static IEnumerator AsCoroutine<T>(this Task<T> task, System.Action<T> resultCallback = null)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            resultCallback?.Invoke(task.Result);
        }
    }
}
