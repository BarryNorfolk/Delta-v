using System.Linq;
using Content.Server.Actions;
using Content.Shared._DV.Actions;
using Content.Shared._DV.Blob;
using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    private readonly EntProtoId _blobNode = "BlobAntagNode";
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagComponent, ComponentInit>(OnBlobStart);
        SubscribeLocalEvent<BlobAntagComponent, EventBlobCreateNode>(OnCreateNode);
    }

    private void OnBlobStart(Entity<BlobAntagComponent> blob, ref ComponentInit args)
    {
        foreach (var actionId in blob.Comp.InnateActions)
        {
            var actionEnt = _actions.AddAction(blob, actionId);
            blob.Comp.ActionEntities.Add(actionEnt);
        }
    }

    private void OnCreateNode(Entity<BlobAntagComponent> blob, ref EventBlobCreateNode args)
    {
        // TODO(Barry): Check whether we have the energy for this manually.
        if (!TryComp<ActionCostComponent>(args.Action, out var actionCost))
            return;

        // TODO(Barry): Move this to a TryUpdateEnergy function in shared, or something.
        if (blob.Comp.Energy < actionCost.Cost)
        {
            // TODO(Barry): You don't have 'nuff energy for this!
            return;
        }

        blob.Comp.Energy -= actionCost.Cost;

        // Can only manually spawn nods in cardinal directions
        TrySpawnNode(blob, args.Target.AlignWithClosestGridTile(), enforceCardinal: true);
    }

    private void TrySpawnNode(Entity<BlobAntagComponent> blob, EntityCoordinates coords, bool enforceCardinal = false)
    {
        // TODO(Barry): Check whether this is a valid place to put a tile.

        // Check it's got a neighbour that has a blob node on it
        /* TODO(Barry): It's probably more performant to do one lookup for an entire 9x9 grid around
                        the center point of the spawn request, and then filter those.
        */
        // TODO(Barry): Setup the cardinality enforcement here
        var hasNeighbor = false;
        foreach (var direction in DirectionExtensions.AllDirections)
        {
            var neighborCoord = coords.Offset(direction.ToIntVec());
            var neighborEnts = _lookup.GetEntitiesIntersecting(neighborCoord);
            if (neighborEnts.Any(HasComp<BlobAntagNodeComponent>))
            {
                hasNeighbor = true;
                break;
            }
        }

        if (!hasNeighbor)
        {
            // TODO(Barry): Do things that say "Oh god you can't place stuff here
            return;
        }

        SpawnAtPosition(_blobNode, coords);
    }

    // TODO(Barry): This should probably be inside shared so its predicted.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<BlobAntagComponent>();
        while (query.MoveNext(out var blob, out var comp))
        {
            if (comp.NextUpdate > now)
                continue;

            comp.NextUpdate = now + TimeSpan.FromSeconds(1);

            if (comp.Energy >= comp.MaxEnergy)
                continue;

            comp.Energy += comp.EnergyPerSecond;
        }
    }
}
