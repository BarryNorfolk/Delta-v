using System.Linq;
using Content.Shared._DV.Actions;
using Content.Shared._DV.Blob.Components;
using Content.Shared.NodeContainer;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
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
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _bui = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

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

    private void BindNodeToCore(EntityUid node, EntityUid? core)
    {
        var nodeComp = EnsureComp<BlobNodeComponent>(node);
        nodeComp.BlobCore = core;

        Dirty(node, nodeComp);
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
        RaiseLocalEvent(args.Target, ref ev);

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

        // TODO (Barry): Is this the actual valid way to check the components on a given prototype?
        var foo = PrototypeManager.Index(args.ProtoId);
        if (foo.TryGetComponent<BlobNodePlacementLimiterComponent>(
            _factory.GetComponentName<BlobNodePlacementLimiterComponent>(), out var limitation))
        {
            if (!_factory.TryGetRegistration(limitation.Component, out var registration))
                return;

            var xform = Transform(node);

            var query = EntityManager.AllEntityQueryEnumerator(registration.Type);
            var entities = EntityManager.AllEntities(registration.Type);
            if (limitation.MaximumCount > 0 && entities.Length >= limitation.MaximumCount)
            {
                Popup.PopupClient("Too Many", node, node.Comp.BlobCore, PopupType.MediumCaution);
                return;
            }

            foreach (var otherNode in entities)
            {
                var otherXform = Transform(otherNode);
                xform.Coordinates.TryDistance(EntityManager, otherXform.Coordinates, out var distance);

                if (distance < limitation.MinTileDistance)
                {
                    Popup.PopupClient("Too close", node, node.Comp.BlobCore, PopupType.MediumCaution);
                    return;
                }
            }
        }

        var transform = Transform(node);
        var newNode = Spawn(args.ProtoId, transform.Coordinates);
        BindNodeToCore(newNode, node.Comp.BlobCore);

        // Cleanup the old node
        PredictedQueueDel(node);
    }

    private bool TrySpawnNode(Entity<BlobComponent> blob, EntityCoordinates coords)
    {
        // If we have no grid or transform, no point in doing any more expensive checks
        var xform = Transform(blob);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return false; // Somehow not on a grid

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

        // Is anything on this tile that would otherwise stop us from spreading there?
        // (I.e. A wall, table, anchored machine, etc).
        var anchoredEnts = _map.GetAnchoredEntities(xform.GridUid.Value, grid, coords);
        foreach (var ent in anchoredEnts)
        {
            // TODO (Barry): Add more checks here, because we shuoldn't be able to
            // put nodes of walls and other andhor things. But for the moment, just regular
            // checking for blob nodes is good enough.
            if (HasComp<BlobNodeComponent>(ent))
                return false; // No spreading node over another node
        }

        var newNode = PredictedSpawnAtPosition(_blobNode, coords);
        BindNodeToCore(newNode, blob);

        return true;
    }

    protected virtual void PulseNetwork(Entity<BlobComponent> blob)
    {
        // TODO (Barry): Make this just a pure virtual if there's nothing to be done on the client
        // side. We probably want to predict some visuals for the pulse though.
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = Timing.CurTime;
        var query = EntityQueryEnumerator<BlobComponent>(); // TODO(Barry): Store the query
        while (query.MoveNext(out var blob, out var comp))
        {
            if (comp.NextBlobPulse > now)
                continue;

            comp.NextBlobPulse = now + comp.BlobPulseDelay;
            AddEnergy((blob, comp), comp.EnergyPerPulse);

            // Spread out from the core if able
            // Then pulse any special nodes on the network
            PulseNetwork((blob, comp));
        }
    }
}
