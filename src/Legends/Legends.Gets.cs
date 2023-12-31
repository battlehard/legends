using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Swappables.Helpers;

namespace Swappables
{
  public partial class Legends
  {
    [Safe]
    public override string Symbol() => "SWAPPABLES";

    [Safe]
    public static UInt160 GetContractOwner()
    {
      return (UInt160)Storage.Get(Storage.CurrentContext, Prefix_Owner);
    }

    [Safe]
    public override Map<string, object> Properties(ByteString tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      LegendsState state = (LegendsState)StdLib.Deserialize(tokenMap[tokenId]);
      Map<string, object> map = new();
      map["owner"] = state.Owner;
      map["name"] = state.Name;
      map["image"] = state.ImageUrl;
      return map;
    }

    [Safe]
    public static Map<string, object> ListNftPool(BigInteger pageNumber, BigInteger pageSize)
    {
      Assert(pageNumber > 0 && pageSize > 0, "Pagination data must be provided, pageNumber and pageSize must have at least 1");
      Assert(pageSize <= MAX_PAGE_LIMIT, $"Input page limit exceed the max limit of {MAX_PAGE_LIMIT}");

      BigInteger totalNft = BalanceOf(Runtime.ExecutingScriptHash);
      // Calculate the total number of pages based on the total NFTs and page size
      BigInteger totalPages = totalNft / pageSize;
      if (totalNft % pageSize > 0)
      {
        totalPages += 1;
      }
      Assert(pageNumber <= totalPages, $"Input page number exceed the totalPages of {totalPages}");

      // Calculate the number of items to skip based on the requested page and page size
      BigInteger skipCount = (pageNumber - 1) * pageSize;

      // Initialize return variable
      Map<string, object> nftPoolPaginationData = new();
      nftPoolPaginationData["totalPages"] = totalPages;
      nftPoolPaginationData["totalNfts"] = totalNft;
      // Get list of all NFTs in the pool with pagination parameters
      nftPoolPaginationData["nftList"] = GetNftInPool(skipCount, pageSize);
      return nftPoolPaginationData;
    }

    [Safe]
    private static List<Map<string, object>> GetNftInPool(BigInteger skipCount, BigInteger pageSize)
    {
      StorageMap nftPoolMap = new(Storage.CurrentContext, Prefix_AccountToken);
      Iterator keys = nftPoolMap.Find(Runtime.ExecutingScriptHash, FindOptions.KeysOnly | FindOptions.RemovePrefix);
      List<Map<string, object>> returnListData = new();
      BigInteger foundKeySeq = 0;
      while (keys.Next())
      {
        if (foundKeySeq >= skipCount && foundKeySeq < (skipCount + pageSize))
        {
          ByteString tokenId = (ByteString)keys.Value;
          LegendsState tokenState = (LegendsState)StdLib.Deserialize(new StorageMap(Storage.CurrentContext, Prefix_Token)[tokenId]);

          Map<string, object> nftPoolMapData = new();
          nftPoolMapData["owner"] = tokenState.Owner;
          nftPoolMapData["name"] = tokenState.Name;
          nftPoolMapData["image"] = tokenState.ImageUrl;
          returnListData.Add(nftPoolMapData);
        }
        if (returnListData.Count >= pageSize)
          break;
        foundKeySeq++;
      }
      return returnListData;
    }
  }
}