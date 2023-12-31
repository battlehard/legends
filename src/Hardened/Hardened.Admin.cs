using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;

namespace Hardened
{
  public partial class Hardened
  {
    private static void CheckContractAuthorization()
    {
      if (!IsOwner())
      {
        var tx = (Transaction)Runtime.ScriptContainer;
        // tx.Sender is transaction signer
        Assert(AdminHashesStorage.IsExist(tx.Sender) == true, $"{CONTRACT_NAME}: No admin authorization");
      }
    }

    public static void SetAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Put(contractHash);
    }

    public static List<UInt160> GetAdmin()
    {
      CheckContractAuthorization();
      return AdminHashesStorage.List();
    }

    public static void DeleteAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Delete(contractHash);
    }
  }
}
