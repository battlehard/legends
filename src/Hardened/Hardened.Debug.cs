using Neo;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;

namespace Hardened
{
  public partial class Hardened
  {
    public static void TestSuite()
    {
      CheckContractAuthorization();
      Debug_ManageAdmin();
    }
    private static void Debug_ManageAdmin()
    {

      UInt160 admin1 = ToScriptHash("Nh9XK6sZ6vkPu9N2L9GxnkogxXKVCgDMws");
      UInt160 admin2 = ToScriptHash("NQbHe4RTkzwGD3tVYsTcHEhxWpN4ZEuMuG");

      // Case 1: No admin
      Assert(GetAdmin().Count == 0, "Expected empty list");
      Runtime.Notify("adminList case 1", new object[] { GetAdmin() });

      // Case 2: Add the first admin
      SetAdmin(admin1);
      Assert(GetAdmin().Count == 1 && GetAdmin()[0] == admin1, "Expected admin1");
      Runtime.Notify("adminList case 2", new object[] { GetAdmin() });

      // Case 3: Add the second admin
      SetAdmin(admin2);
      Assert(GetAdmin().Count == 2, "Expected two admin");
      Assert(AdminHashesStorage.IsExist(admin1), "Expected admin1");
      Assert(AdminHashesStorage.IsExist(admin2), "Expected admin2");
      Runtime.Notify("adminList case 3", new object[] { GetAdmin() });

      // Case 4: Delete the first admin
      DeleteAdmin(admin1);
      Assert(AdminHashesStorage.IsExist(admin1) == false, "Expected no admin1");
      Assert(GetAdmin().Count == 1 && GetAdmin()[0] == admin2, "Expected admin2");
      Runtime.Notify("adminList case 4", new object[] { GetAdmin() });
    }
  }
}