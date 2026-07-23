using Content.Server.Actions;
using Content.Shared._DV.Blob;
using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    private readonly EntProtoId _blobNode = "BlobAntagNode";
    [Dependency] private readonly ActionsSystem _actions = default!;
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
        SpawnAtPosition(_blobNode, coords);
    }
}
