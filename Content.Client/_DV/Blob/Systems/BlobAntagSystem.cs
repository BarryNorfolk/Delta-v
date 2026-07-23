using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Robust.Shared.Utility;

namespace Content.Client._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagComponent, GetBlobUpgradesEvent>(OnGetBlobUpgrades);
    }

    private void OnGetBlobUpgrades(Entity<BlobAntagComponent> blob, ref GetBlobUpgradesEvent args)
    {
        args.Upgrades.Add("Economy", new List<BlobUpgradeRadial>()
        {
            new BlobUpgradeRadial { Tooltip = "Resource Node", Sprite = new SpriteSpecifier.Texture(new ResPath(""))}
        });
    }
}
