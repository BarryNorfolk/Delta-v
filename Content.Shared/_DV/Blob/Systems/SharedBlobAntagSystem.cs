using Content.Shared._DV.Blob.Components;

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
    }
}
