using Content.Server.Actions;
using Content.Shared._DV.Blob;
using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    private readonly EntProtoId _blobNode = "BlobAntagNode";
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
        var coords = args.Target.AlignWithClosestGridTile();
        // TODO(Barry): Check for validity before placing the node
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
