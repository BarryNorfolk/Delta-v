using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Utility;

namespace Content.Client._DV.Blob.Systems;

public sealed class BlobAntagSystem : SharedBlobAntagSystem
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagNodeComponent, GetBlobUpgradesEvent>(OnGetBlobUpgrades);
    }

    private void OnGetBlobUpgrades(Entity<BlobAntagNodeComponent> blob, ref GetBlobUpgradesEvent args)
    {
        foreach (var upgrade in AvailableUpgrades)
        {
            var proto = PrototypeManager.Index(upgrade);
            if (_entityWhitelist.IsWhitelistFailOrNull(proto.AllowedFrom, args.Target))
                continue;

            args.Upgrades.Add(
                new BlobUpgradeRadial
                (
                    proto.Category,
                    proto.Tooltip,
                    proto.Sprite
                )
            );
        }
    }
}
