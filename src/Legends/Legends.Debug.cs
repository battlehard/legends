using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;
using static Swappables.Helpers;

namespace Swappables
{
  public partial class Legends
  {
    public static void Debug_ListNftPool()
    {
      IsOwner();

      try { ListNftPool(0, 0); }
      catch (Exception e)
      { Runtime.Notify("Expected error", new object[] { e }); }
      try { ListNftPool(1, MAX_PAGE_LIMIT + 1); }
      catch (Exception e)
      { Runtime.Notify("Expected error", new object[] { e }); }
      try { ListNftPool(MAX_PAGE_LIMIT - 1, MAX_PAGE_LIMIT); }
      catch (Exception e)
      { Runtime.Notify("Expected error", new object[] { e }); }

      BigInteger nftInPool = 12; // 12 NFTs minted in setup-express.batch script file during build
      Assert(nftInPool == BalanceOf(Runtime.ExecutingScriptHash), $"Expected {nftInPool} NFTs in the pool");
      ListNftPool(3, 3);
      ListNftPool(4, 3);
      ListNftPool(3, 5);
    }
  }
}