using Content.Shared._DV.Blob.Components;
using Content.Shared.Interaction.Components;

namespace Content.Shared._DV.Blob.Systems;

public sealed class SharedBlobAntagSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BlobAntagComponent> blob, ref MapInitEvent args)
    {
        // On startup, make sure the blob can't move. Just testing this stuff works.
        EnsureComp<BlockMovementComponent>(blob);
    }
}
