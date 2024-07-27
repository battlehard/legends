using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Swappables.Helpers;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  public partial class Legends
  {
    private static bool IsOwner()
    {
      var contractOwner = GetContractOwner();
      var tx = (Transaction)Runtime.ScriptContainer;
      return contractOwner.Equals(tx.Sender) && Runtime.CheckWitness(contractOwner);
    }

    private static void CheckOwner()
    {
      Assert(IsOwner(), $"{CONTRACT_NAME}: No owner authorization");
    }

    public static void _deploy(object data, bool update)
    {
      if (update) return;
      var tx = (Transaction)Runtime.ScriptContainer;
      Storage.Put(Storage.CurrentContext, Prefix_Owner, tx.Sender);
    }

    public static void Update(ByteString nefFile, string manifest)
    {
      CheckOwner();
      ContractManagement.Update(nefFile, manifest, null);
    }

    public static void AddAdminWhiteList(UInt160 contractHash)
    {
      CheckOwner();
      AdminWhiteListStorage.Put(contractHash);
    }

    public static void RemoveAdminWhiteList(UInt160 contractHash)
    {
      CheckOwner();
      AdminWhiteListStorage.Delete(contractHash);
    }

    public static void StorageMigrationNo1()
    {
      byte[] Old_Prefix_Owner = new byte[] { 0x01, 0x00 };
      byte[] Old_Prefix_Admin_White_List = new byte[] { 0x01, 0x01 };
      byte[] Old_Prefix_Trade_Pool = new byte[] { 0x01, 0x02 };
      UInt160 owner = (UInt160)Storage.Get(Storage.CurrentContext, Old_Prefix_Owner);
      if ((BigInteger)Storage.Get(Storage.CurrentContext, Prefix_MigrationNo1) == 0 &&
          Runtime.CheckWitness(owner)) // If not migrate and owner invoke, then do the migration.
      {
        Storage.Put(Storage.CurrentContext, Prefix_MigrationNo1, 1); // Update storage to prevent double migration.
        // Migrate Owner
        Storage.Put(Storage.CurrentContext, Prefix_Owner, owner);
        // Migrate Admin White List
        StorageMap newAdminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        StorageMap oldAdminWhiteListMap = new(Storage.CurrentContext, Old_Prefix_Admin_White_List);
        Iterator admins = oldAdminWhiteListMap.Find();
        while (admins.Next())
        {
          newAdminWhiteListMap.Put(((ByteString[])admins.Value)[0], 1);
        }
        // Migrate Trade Pool
        StorageMap newTradePoolMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        StorageMap oldTradePoolMap = new(Storage.CurrentContext, Old_Prefix_Trade_Pool);
        Iterator poolItems = oldTradePoolMap.Find(FindOptions.RemovePrefix);
        while (poolItems.Next())
        {
          newTradePoolMap.Put(((ByteString[])poolItems.Value)[0], 1);
        }
      }
    }
  }
}