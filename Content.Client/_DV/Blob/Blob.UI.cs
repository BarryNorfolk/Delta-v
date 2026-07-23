using Robust.Shared.Utility;

namespace Content.Client._DV.Blob;

public sealed record BlobUpgradeRadial(string Category, string Tooltip, SpriteSpecifier? Sprite);

[ByRefEvent]
public record struct GetBlobUpgradesEvent(EntityUid Target)
{
    public List<BlobUpgradeRadial> Upgrades = new();
}
