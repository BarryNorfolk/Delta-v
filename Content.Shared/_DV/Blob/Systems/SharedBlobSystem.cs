using System.Linq;
using Content.Shared._DV.Actions;
using Content.Shared._DV.Blob.Components;
using Content.Shared.NodeContainer;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Blob.Systems;

// TODO(Barry): Move this somewhere else
[NetSerializable, Serializable]
public enum BlobUiKey : byte
{
    Key
}

public abstract class SharedBlobSystem : EntitySystem
{
    // TODO(Barry): Look through these deps and try and figure out which ones can stay private
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _bui = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    private readonly EntProtoId _blobNode = "BlobNode";

    // Frustrating that this is not available easily, we only have ALL directions.
    protected readonly Direction[] CardinalDirections = [
        Direction.South,
        Direction.East,
        Direction.North,
        Direction.West
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobComponent, EventBlobCreateNode>(OnCreateNode);
        SubscribeLocalEvent<BlobComponent, EventBlobUpgradeNode>(OnUpgradeAction);

        SubscribeLocalEvent<BlobNodeComponent, GetBlobUpgradesEvent>(OnGetBlobUpgrades);
        SubscribeLocalEvent<BlobNodeComponent, BlobUpgradeMessage>(OnUpgradeBlobNode);
    }

    protected void AddEnergy(Entity<BlobComponent> blob, int amount)
    {
        // We're adding energy here, check if we're already above the max energy
        if (blob.Comp.Energy >= blob.Comp.MaxEnergy)
            return;

        // We're about to add more than we can hold, set it to the max energy
        blob.Comp.Energy += Math.Min(amount, blob.Comp.MaxEnergy - blob.Comp.Energy);
    }

    protected bool HasEnergy(Entity<BlobComponent> blob, int amount)
    {
        if (blob.Comp.Energy < amount)
        {
            Popup.PopupClient(Loc.GetString("blob-action-fail-energy"), blob);
            return false;
        }

        return true;
    }

    protected void RemoveEnergy(Entity<BlobComponent> blob, int amount)
    {
        // TODO(Barry): Double check this? Debug assert?
        blob.Comp.Energy -= amount;
    }

    private void OnCreateNode(Entity<BlobComponent> blob, ref EventBlobCreateNode args)
    {
        if (!TryComp<ActionCostComponent>(args.Action, out var actionCost))
            return;

        // TODO(Barry): I don't like that this uses up the cost and then might fail later on.
        //              We can have a function for checking the amount of energy available, maybe.
        if (!HasEnergy(blob, actionCost.Cost))
            return;

        if (!TrySpawnNode(blob, args.Target.AlignWithClosestGridTile()))
            return;

        RemoveEnergy(blob, actionCost.Cost);
    }

    private void OnUpgradeAction(Entity<BlobComponent> blob, ref EventBlobUpgradeNode args)
    {
        if (args.Handled || !TryComp<ActorComponent>(blob, out var actor))
            return;
        args.Handled = true;

        var ev = new GetBlobUpgradesEvent(args.Target);
        RaiseLocalEvent(blob, ref ev);

        if (ev.Upgrades.Count == 0)
        {
            // No upgrades available for this particular node, nothing to do.
            Popup.PopupClient(Loc.GetString("blob-action-upgrade-none-available"), blob);
            return;
        }

        _bui.TryToggleUi(args.Target, BlobUiKey.Key, actor.PlayerSession);
        _bui.SetUiState(args.Target, BlobUiKey.Key, new BlobUpgradeOptionsState(ev.Upgrades));
    }

    private void OnGetBlobUpgrades(Entity<BlobNodeComponent> blob, ref GetBlobUpgradesEvent args)
    {
        foreach (var proto in PrototypeManager.EnumeratePrototypes<BlobUpgradePrototype>())
        {
            if (_entityWhitelist.IsWhitelistFailOrNull(proto.AllowedFrom, args.Target))
                continue;

            args.Upgrades.Add(
                new BlobUpgradeRadial
                (
                    proto.Category,
                    proto.Tooltip,
                    proto.Sprite,
                    proto.Creates
                )
            );
        }
    }

    private void OnUpgradeBlobNode(Entity<BlobNodeComponent> node, ref BlobUpgradeMessage args)
    {
        // TODO (Barry): This is probably overly simple and needs work
        var transform = Transform(node);
        Spawn(args.ProtoId, transform.Coordinates);
        QueueDel(node);
    }

    private bool TrySpawnNode(Entity<BlobComponent> blob, EntityCoordinates coords)
    {
        // TODO(Barry): Check whether this is a valid place to put a tile.

        // Check it's got a neighbour that has a blob node on it
        /* TODO(Barry): It's probably more performant to do one lookup for an entire 9x9 grid around
                        the center point of the spawn request, and then filter those.
                        If this turns out to be a problem point, we can look into it.
        */
        var hasNeighbor = false;
        foreach (var direction in CardinalDirections)
        {
            var neighborCoord = coords.Offset(direction.ToIntVec());
            var neighborEnts = Lookup.GetEntitiesIntersecting(neighborCoord);
            if (neighborEnts.Any(HasComp<BlobNodeComponent>))
            {
                hasNeighbor = true;
                break;
            }
        }

        if (!hasNeighbor)
        {
            Popup.PopupClient(Loc.GetString("blob-action-spawn-node-fail-neighbour"), blob);
            return false;
        }

        PredictedSpawnAtPosition(_blobNode, coords);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = Timing.CurTime;
        var query = EntityQueryEnumerator<BlobComponent>();
        while (query.MoveNext(out var blob, out var comp))
        {
            if (comp.NextBlobPulse > now)
                continue;

            comp.NextBlobPulse = now + comp.BlobPulseDelay;
            AddEnergy((blob, comp), comp.EnergyPerSecond);

            // Spread out from the core if able
            // Then pulse any special nodes on the network
        }
    }
}
