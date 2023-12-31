using FluentAssertions;
using Neo;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using NeoTestHarness;
using System.Numerics;
using TestSwappables; // For reference contract method interface e.g. Legends
using Xunit;

namespace test
{
  [CheckpointPath("checkpoints/legends-minted-to-wallet.neoxp-checkpoint")]
  public class LegendsTradeTests : IClassFixture<CheckpointFixture<LegendsTradeTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public LegendsTradeTests(CheckpointFixture<LegendsTradeTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void owner_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Use global scope for test because there is no document about how to set custom contract scope
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_OWNER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3); // 2 transfer events and 1 trade event
    }

    [Fact]
    public void admin_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3);
    }

    [Fact]
    public void user_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 user = chain.GetDefaultAccount("user").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, user, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_USER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3);
    }

    [Fact]
    public void admin_cannot_trade_user_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_USER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("No NFT ownership");
    }

    [Fact]
    public void admin_cannot_trade_not_exist_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_NOT_EXIST));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("This token not existing");
    }

    [Fact]
    public void admin_cannot_trade_unavailable_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_OWNER));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("LegendsOwner in not available in the pool");
    }

    [Fact]
    public void List_page_with_failed_cases()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      TestApplicationEngine engineStep1 = new(snapshot, settings, owner, WitnessScope.Global);
      BigInteger nftQuantity = Common.MAX_PAGE_LIMIT + 1;

      using TestApplicationEngine engineStep2 = new(snapshot, settings, owner, WitnessScope.Global);
      engineStep2.ExecuteScript<Legends>(c => c.listNftPool(1, nftQuantity)); // List more than max page limit.
      engineStep2.State.Should().Be(VMState.FAULT);
      engineStep2.UncaughtException.GetString().Should().Contain($"Input page limit exceed the max limit of {Common.MAX_PAGE_LIMIT}");

      using TestApplicationEngine engineStep3 = new(snapshot, settings, owner, WitnessScope.Global);
      engineStep3.ExecuteScript<Legends>(c => c.listNftPool(0, nftQuantity)); // List from page 0
      engineStep3.State.Should().Be(VMState.FAULT);
      engineStep3.UncaughtException.GetString().Should().Contain("Pagination data must be provided, pageNumber and pageSize must have at least 1");

      using TestApplicationEngine engineStep4 = new(snapshot, settings, owner, WitnessScope.Global);
      engineStep4.ExecuteScript<Legends>(c => c.listNftPool(Common.MAX_PAGE_LIMIT, Common.MAX_PAGE_LIMIT)); // List page over the total pages
      engineStep4.State.Should().Be(VMState.FAULT);
      engineStep4.UncaughtException.GetString().Should().Contain($"Input page number exceed the totalPages of 1");
    }
  }
}
