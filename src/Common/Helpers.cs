using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using System;

namespace Hardened
{
  public class Helpers
  {
    private const string ADDRESS_INVALID = "The address is invalid";

    public static void ValidateScriptHash(UInt160 scriptHash)
    {
      Assert(scriptHash is not null && scriptHash.IsValid, ADDRESS_INVALID);
    }

    public static void Assert(bool condition, string errorMessage)
    {
      if (!condition)
      {
        throw new Exception(errorMessage);
      }
    }

    public static UInt160 ToScriptHash(string address)
    {
      if (address.ToByteArray()[0] == 0x4e) // N3 address start with 'N'
      {
        var decoded = (byte[])StdLib.Base58CheckDecode(address);
        var scriptHash = (UInt160)decoded.Last(20);
        ValidateScriptHash(scriptHash);
        return scriptHash;
      }
      else
      {
        throw new Exception(ADDRESS_INVALID);
      }
    }
  }
}