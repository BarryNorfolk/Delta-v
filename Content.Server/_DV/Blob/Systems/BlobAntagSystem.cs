using Content.Server.Actions;
using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;

namespace Content.Server._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagComponent, ComponentInit>(OnBlobStart);
    }

    private void OnBlobStart(Entity<BlobAntagComponent> blob, ref ComponentInit args)
    {
        foreach (var actionId in blob.Comp.InnateActions)
        {
            var actionEnt = _actions.AddAction(blob, actionId);
            blob.Comp.ActionEntities.Add(actionEnt);
        }
    }
}
