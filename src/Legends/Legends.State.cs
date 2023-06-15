using Neo.SmartContract.Framework;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  /// <summary>
  /// Class <c>LegendsState</c>
  /// Inherit Owner, Name from Nep11TokenState
  /// Add TokenId and ImageUrl
  /// </summary>
  public class LegendsState : Nep11TokenState
  {
    public string TokenId;
    public string ImageUrl;
  }
}
