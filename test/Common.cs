using Neo.VM;
using Neo.VM.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using System.Text;

namespace test
{
  static class Common
  {
    public static readonly byte[] Prefix_Owner = new byte[] { 0x10, 0x00 };
    public static readonly byte[] Prefix_Trade_Pool = new byte[] { 0x10, 0x02 };
    public static readonly string LEGENDS_ONE = "LegendsOne";
    public static readonly string LEGENDS_OWNER = "LegendsOwner";
    public static readonly string LEGENDS_ADMIN = "LegendsAdmin";
    public static readonly string LEGENDS_USER = "LegendsUser";
    public static readonly string LEGENDS_NOT_EXIST = "LegendsNotExist";
    public static readonly BigInteger MAX_PAGE_LIMIT = 100;

    /// <summary>
    /// Use for extract StackItem that store Map<string, object> data type. e.g. return value from Properties
    /// </summary>
    /// <param name="stackItem">Must be a Map<string, object> type</param>
    public static string GetStringValueFromMapKey(StackItem stackItem, string searchKey)
    {
      JObject jObject = JObject.Parse(stackItem.ToJson().ToString());
      JArray mapItems = (JArray)jObject["value"];
      foreach (var item in mapItems)
      {
        byte[] keyBytes = Convert.FromBase64String(item["key"]["value"].ToString());
        byte[] valueBytes = Convert.FromBase64String(item["value"]["value"].ToString());

        string key = Encoding.UTF8.GetString(keyBytes);
        string value = Encoding.UTF8.GetString(valueBytes);

        if (key == searchKey) return value;
      }
      return null;
    }
  }
}
