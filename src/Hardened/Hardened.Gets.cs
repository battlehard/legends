using Neo;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace Hardened
{
  public partial class Hardened
  {
    [Safe]
    public override string Symbol() => "BH";

    [Safe]
    public static UInt160 GetContractOwner()
    {
      return (UInt160)Storage.Get(Storage.CurrentContext, Prefix_Owner);
    }
  }
}